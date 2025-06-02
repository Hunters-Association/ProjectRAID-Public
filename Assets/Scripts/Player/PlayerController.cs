using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Animations.Rigging;
using Cinemachine;
using TMPro;
using DG.Tweening;
using ProjectRaid.EditorTools;
using ProjectRaid.Extensions;
using System.Linq;


#if UNITY_EDITOR
using UnityEditor;
#endif

public enum CameraType { Default, Combat, Aim }

/// <summary>
/// 플레이어 캐릭터 제어 및 상태 관리 메인 클래스
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInputHandler))]
[DisallowMultipleComponent]
public sealed class PlayerController : MonoBehaviour, IDamageable
{
    #region INSPECTOR
    [FoldoutGroup("플레이어 설정", ExtendedColor.White)]
    [SerializeField] private float moveSpeed = 2f;                          // 걷기 속도
    [SerializeField] private float runSpeed = 6f;                           // 달리기 속도
    [SerializeField] private float speedChangeRate = 10f;                   // 속도 변화율
    [SerializeField] private float rotationSmoothTime = 0.12f;              // 회전 변화율

    [FoldoutGroup("플레이어 설정/카메라", ExtendedColor.White)]
    [SerializeField] private Transform cameraOrientation;                   // 카메라 방향
    [SerializeField] private Transform aimingModeLookAt;                    // 조준 카메라 LookAt 타겟
    [SerializeField] private CinemachineBrain mainCamera;                   // 시네머신 브레인
    [SerializeField] private CinemachineVirtualCameraBase defaultCamera;    // 기본 FreeLook 카메라
    [SerializeField] private CinemachineVirtualCameraBase combatCamera;     // 전투 FreeLook 카메라
    [SerializeField] private CinemachineVirtualCameraBase aimCamera;        // 조준 FreeLook 카메라
    [SerializeField] private float cameraAngleOverride = 0f;                // 카메라 각도 보정

    [FoldoutGroup("플레이어 설정/스탯", ExtendedColor.White)]
    [SerializeField] private StatController stats;                          // 스탯 컨트롤러
    [SerializeField] private BuffSystem buffSystem;                         // 버프 시스템

    [FoldoutGroup("플레이어 설정/UI", ExtendedColor.White)]
    [SerializeField] private PlayerHUD playerHUD;                           // HUD
    [SerializeField] private InventoryUI inventoryUI;                       // 인벤토리 UI
    [SerializeField] private GaugeBarView screenHP;                         // 전체화면 체력 UI

    [FoldoutGroup("플레이어 설정/메테리얼", ExtendedColor.White)]
    [SerializeField] private List<Renderer> toonRenderers = new();          // ASP 툰 셰이더 메테리얼 리스트
    [SerializeField] private uint defaultRenderingLayerMask;
    [SerializeField] private uint outlineRenderingLayerMask;

    [FoldoutGroup("전투/무기", ExtendedColor.Crimson)]
    [SerializeField] private PlayerCombatController playerCombatController; // 전투 컨트롤러
    [SerializeField] private WeaponManager weaponManager;                   // 무기 매니저
    [SerializeField] private Rig bodyRig;                                   // 조준 방향 회전을 위한 Rig
    [SerializeField] private Rig weaponRig;                                 // 무기 컨트롤 Rig
    [SerializeField] private Rig handRig;                                   // 왼손 위치 보정용 Rig
    [SerializeField] private Volume aimingVolume;                           // 조준 포스트 프로세싱 볼륨
    [SerializeField] private GameObject aim;                                // 조준점 오브젝트

    [FoldoutGroup("전투/효과", ExtendedColor.Crimson)]
    [SerializeField] private float knockbackDuration = 0.2f;                // 넉백 지속 시간
    [SerializeField] private float knockbackPower = 5f;                     // 넉백 힘
    [SerializeField] private float invincibleDuration = 1f;                 // 피격 후 무적 시간

    [FoldoutGroup("상호작용", ExtendedColor.GreenYellow)]
    [SerializeField] private float interactRange = 2f;                      // 상호작용 거리
    [SerializeField] private LayerMask interactableLayer;                   // 상호작용 가능한 레이어
    [SerializeField] private CanvasGroup interactionUI;                     // 상호작용 UI (프롬프트 포함)
    [SerializeField] private TextMeshProUGUI interactionPrompt;             // 상호작용 프롬프트 텍스트

    [FoldoutGroup("컴포넌트", ExtendedColor.SeaGreen)]
    [SerializeField] private CharacterController characterController;       // 캐릭터 컨트롤러
    [SerializeField] private CapsuleCollider hurtboxCollider;               // Hurtbox 콜라이더
    [SerializeField] private IKTargetFollower iKTargetFollower;             // IK 타겟 팔로워
    [SerializeField] private PlayerInputHandler inputHandler;               // 인풋 핸들러
    [SerializeField] private PlayerEffectController vfxController;          // 이펙트 컨트롤러
    [SerializeField] private AudioSource audioSource;                       // 오디오 소스
    [SerializeField] private Animator animator;                             // 애니메이터

    [FoldoutGroup("디버그/기즈모", ExtendedColor.DodgerBlue)]
    [SerializeField] private bool showGizmo = false;

    [FoldoutGroup("디버그/로그", ExtendedColor.DodgerBlue)]
    [SerializeField] private bool moreLogs = false;                         // 테스트 모드 (더 많은 Debug.Log 출력)

    [field: FoldoutGroup("디버그/read-only/FSM", ExtendedColor.DodgerBlue)]
    [field: SerializeField] public string CurrentStateName { get; set; }
    [field: SerializeField] public string PreviousStateName { get; set; }

    [field: FoldoutGroup("디버그/read-only/전투", ExtendedColor.DodgerBlue)]
    [field: SerializeField] public List<GameObject> CurrentHostileMobs { get; private set; } = new();
    [field: SerializeField] public string CurrentHitPattern { get; private set; }

    [field: FoldoutGroup("디버그/read-only/기타", ExtendedColor.DodgerBlue)]
    [field: SerializeField] public BaseUI CurrentUI { get; set; }
    [field: SerializeField] public GameObject CurrentInteractableObject { get; private set; }
    #endregion

    #region RUNTIME
    private Vector3 knockbackDirection;                                     // 넉백 방향
    private float knockbackTimer = 0f;                                      // 넉백 타이머
    private float invincibleTimer = 0f;                                     // 무적 시간 타이머
    private bool isInvincible = false;                                      // 현재 무적 상태 여부
    private bool canRun = true;                                             // 실제로 달릴 수 있는 상황인지 확인
    private bool wasRun = false;                                            // 이전 프레임 달리기 여부 추적
    private bool hasReleasedRunOnce = false;                                // 스태미나 소진 후 달리기 키를 다시 눌렀는지 확인
    private bool isOutlineActive = false;                                   // 아웃라인 활성화됐는지지 확인

    private PlayerStateMachine stateMachine;                                // 상태 머신
    private IdleState idle;                                                 // 대기 상태
    private CombatState combat;                                             // 전투 상태
    private AimingState aiming;                                             // 조준 상태
    private AttackState attack;                                             // 공격 상태
    private DeadState dead;                                                 // 사망 상태
    private InteractionState interact;                                      // 상호작용 상태

    private float speed;                                                    // 현재 이동 속도
    private float animationBlend;                                           // 애니메이션 블렌딩
    private float targetRotation;                                           // 목표 회전
    private float rotationVelocity;                                         // 회전 속도

    private CinemachineVirtualCameraBase currentCamera;                     // 현재 활성화된 카메라 저장
    private CinemachineFreeLook CurrentFreeLook => currentCamera as CinemachineFreeLook;
    private float currentDitherValue = 0f;
    #endregion

    #region PROPERTIES (public read-only)
    public Vector2 MoveInput => inputHandler.MoveInput;
    public Vector2 LookInput => inputHandler.LookInput;
    public bool IsMoving => MoveInput != Vector2.zero;
    public bool IsRunning => inputHandler.IsRunning;
    public bool IsAiming => inputHandler.IsAiming;
    public bool IsInteractPressed => inputHandler.IsInteractPressed;
    public bool IsInventoryPressed => inputHandler.IsInventoryPressed;
    public bool IsInvincible => isInvincible;
    public bool IsInDodge { get; private set; }
    public bool IsInEquip { get; private set; }

    public Rig BodyRig => bodyRig;
    public Rig WeaponRig => weaponRig;
    public Rig HandRig => handRig;
    public Volume AimingVolume => aimingVolume;

    public InventoryUI InventoryUI => inventoryUI;

    public StatController Stats => stats;
    public PlayerHUD PlayerHUD => playerHUD;
    public BuffSystem BuffSystem => buffSystem;
    public GaugeBarView ScreenHP => screenHP;

    public PlayerCombatController CombatController => playerCombatController;
    public WeaponManager WeaponManager => weaponManager;
    public AttackComponent Weapon => WeaponManager.CurrentWeapon;
    public CharacterController CharacterController => characterController;
    public CapsuleCollider Hurtbox => hurtboxCollider;
    public IKTargetFollower IKTargetFollower => iKTargetFollower;
    public PlayerInputHandler InputHandler => inputHandler;
    public PlayerEffectController VfxController => vfxController;
    public AudioSource AudioSource => audioSource;
    public Animator Animator => animator;
    public List<IHostile> CurrentHostiles { get; private set; }
    public IInteractable CurrentInteractable { get; private set; }
    public string InteractionKeyName
    {
        get
        {
            var action = inputHandler.PlayerInput.actions["Interact"];
            var binding = action.bindings[0];
            var path = binding.effectivePath;

            if (!string.IsNullOrEmpty(path))
            {
                var parts = path.Split('/');
                if (parts.Length > 1) return parts[1];
            }

            return "Unknown";
        }
    }

    public int ComboIndex { get; set; } = 0;

    public bool TestMode => moreLogs;
    public PlayerStateMachine StateMachine => stateMachine;
    public PlayerState CurrentState { get; set; }
    public PlayerState PreviousState { get; set; }
    public IdleState IdleState => idle;
    public CombatState CombatState => combat;
    public AimingState AimingState => aiming;
    public AttackState AttackState => attack;
    public DeadState DeadState => dead;
    public InteractionState InteractionState => interact;
    #endregion

    private Tween fadeTween;

    private readonly StringBuilder sb = new(64);

    private readonly int DitheringPropID = Shader.PropertyToID("_Dithering");
    private MaterialPropertyBlock mpb;

    private const string KEY_TAG_PREFIX = "<style=\"Prompt\">";
    private const string KEY_TAG_SUFFIX = "</style>";

    private const float GRAVITY = 9.8f;
    private const float THRESHOLD = 0.01f;

    #region LIFECYCLE
    private void Awake()
    {
        mpb = new MaterialPropertyBlock();

        InitStats();
        InitFSM();
        CursorManager.SetCursorState(false);
    }

    private void Start()
    {
        interactionUI.alpha = 0f;
        stateMachine.Initialize(idle);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (GameManager.Instance.Inventory.TryAdd(3001, 5)) Debug.Log("3001 x 5");
            if (GameManager.Instance.Inventory.TryAdd(3501, 2)) Debug.Log("3501 x 2");
            if (GameManager.Instance.Inventory.TryAdd(3503, 2)) Debug.Log("3503 x 2");
        }

        if (inputHandler.IsEscapePressed && CurrentUI != null) CurrentUI.OnHide();

        TickKnockback();
        TickInvincible();
        TickStamina();
        CheckForInteractables();

        if (knockbackTimer <= 0)
        {
            if (UIManager.Instance != null && !UIManager.Instance.IsBlockingUIActive())
                stateMachine.CurrentState.HandleInput();

            stateMachine.CurrentState.Update();
            Move();
        }

        bool isIdle = CurrentState is IdleState || CurrentState is InteractionState;

        if (isOutlineActive == isIdle)
        {
            isOutlineActive = !isIdle;
            var maskToApply = isIdle ? defaultRenderingLayerMask : outlineRenderingLayerMask;

            foreach (var renderer in toonRenderers)
            {
                if (renderer == null) continue;
                
                renderer.renderingLayerMask = maskToApply;

                if (isIdle)
                {
                    renderer.GetPropertyBlock(mpb);
                    float startValue = mpb.GetFloat(DitheringPropID);

                    DOVirtual.DelayedCall(0.5f, () =>
                    {
                        DOTween.To(() => startValue, x =>
                        {
                            startValue = x;

                            renderer.GetPropertyBlock(mpb);
                            mpb.SetFloat(DitheringPropID, startValue);
                            renderer.SetPropertyBlock(mpb);

                        }, 0f, 0.5f).SetEase(Ease.OutCubic);
                    });
                }
            }
        }
    }

    private void FixedUpdate() => stateMachine.CurrentState.PhysicsUpdate();

    private void LateUpdate()
    {
        stateMachine.CurrentState.LateUpdate();

        CameraRotation();
        CameraVerticalWeight();
    }

    private void OnDestroy()
    {
        // stats.Runtime.OnDie -= HandleDie;

        stateMachine.OnChangePreviousState -= HandlePreviousStateChanged;
        stateMachine.OnChangeCurrentState -= HandleCurrentStateChanged;
    }
    #endregion

    #region STAT
    private void InitStats()
    {
        stats.ApplyBaseStats();
        playerHUD.Bind(stats, buffSystem);
        // stats.Runtime.OnDie += HandleDie;
    }

    // private void HandleDie() => stateMachine.ChangeState(DeadState);
    #endregion

    #region FSM
    private void InitFSM()
    {
        stateMachine = new PlayerStateMachine();
        idle = new IdleState(this, stateMachine);
        combat = new CombatState(this, stateMachine);
        aiming = new AimingState(this, stateMachine);
        attack = new AttackState(this, stateMachine);
        dead = new DeadState(this, stateMachine);
        interact = new InteractionState(this, stateMachine);

        stateMachine.OnChangePreviousState += HandlePreviousStateChanged;
        stateMachine.OnChangeCurrentState += HandleCurrentStateChanged;
    }

    private void HandlePreviousStateChanged(PlayerState state)
    {
        PreviousState = state;
        PreviousStateName = state.GetType().Name;
    }

    private void HandleCurrentStateChanged(PlayerState state)
    {
        CurrentState = state;
        CurrentStateName = state.GetType().Name;
    }
    #endregion

    #region MOVEMENT
    private void Move()
    {
        float targetSpeed = 0f;

        if (CurrentState.CanMove && IsMoving)
        {
            bool isOutOfStamina = stats.Runtime.CurrentStamina <= 0f;

            if (isOutOfStamina)
            {
                animator.ResetTrigger(PlayerAnimatorParams.DodgeTrigger);
                canRun = false;
            }
            else
            {
                if (!IsRunning)
                {
                    hasReleasedRunOnce = true;
                }

                if (IsRunning && hasReleasedRunOnce)
                {
                    hasReleasedRunOnce = false;
                    canRun = true;
                }
            }

            bool canActuallyRun = CurrentState.CanRun && IsRunning && canRun && !isOutOfStamina;
            targetSpeed = canActuallyRun ? runSpeed : moveSpeed;

            if (canActuallyRun)
            {
                stats.ConsumeStamina(stats.StaminaConsumeRate * Time.deltaTime);
            }

            if (wasRun && !canActuallyRun)
            {
                stats.TriggerStaminaRegenDelay();
            }
            wasRun = canActuallyRun;
        }

        float currentSpeed = new Vector3(characterController.velocity.x, 0f, characterController.velocity.z).magnitude;
        float inputMagnitude = inputHandler.IsCurrentDeviceMouse ? 1f : MoveInput.magnitude;
        float speedOffset = 0.1f;

        if (Mathf.Abs(currentSpeed - targetSpeed) > speedOffset)
        {
            speed = Mathf.Lerp(currentSpeed, targetSpeed * inputMagnitude, Time.deltaTime * speedChangeRate);
            speed = Mathf.Round(speed * 1000f) / 1000f;
        }
        else
        {
            speed = targetSpeed;
        }

        animationBlend = Mathf.Lerp(animationBlend, targetSpeed, Time.deltaTime * speedChangeRate);
        if (animationBlend < THRESHOLD) animationBlend = 0f;

        if (CurrentState is AimingState || CurrentState is AttackState)
        {
            // 조준 상태: 캐릭터의 회전이 카메라의 방향을 따라가도록 설정
            if (animator.GetBool(PlayerAnimatorParams.UseRootMotion))
            {
                if (!animator.GetBool(PlayerAnimatorParams.LockTurn))
                {
                    Vector3 dir = new Vector3(MoveInput.x, 0f, MoveInput.y).normalized;
                    if (dir.sqrMagnitude > 0f)
                    {
                        targetRotation = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y;
                        float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity, rotationSmoothTime);
                        transform.rotation = Quaternion.Euler(0f, rotation, 0f);
                    }
                }
            }
        }
        else if (IsMoving && CurrentState != DeadState)
        {
            // 일반 상태: 입력 방향 + 카메라 기준으로 회전
            Vector3 direction = new Vector3(MoveInput.x, 0f, MoveInput.y).normalized;
            if (direction.sqrMagnitude > 0f)
            {
                targetRotation = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity, rotationSmoothTime);
                transform.rotation = Quaternion.Euler(0f, rotation, 0f);
            }
        }

        Vector3 camForward = mainCamera.transform.forward;
        Vector3 camRight = mainCamera.transform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = (camForward * MoveInput.y + camRight * MoveInput.x).normalized;

        if (characterController.enabled)
        {
            if (animator.GetBool(PlayerAnimatorParams.UseRootMotion))
            {
                characterController.Move(GRAVITY * Time.deltaTime * Vector3.down);
            }
            else
            {
                characterController.Move((moveDir * speed + Vector3.down * GRAVITY) * Time.deltaTime);
            }
        }
        animator.SetFloat(PlayerAnimatorParams.Speed, animationBlend);
        animator.SetFloat(PlayerAnimatorParams.MotionSpeed, inputMagnitude);
    }

    /// <summary>
    /// 매프레임 달리기 소모 + StatController 재생 호출
    /// </summary>
    private void TickStamina()
    {
        stats.TickStamina(Time.deltaTime);
    }
    #endregion

    #region CAMERA
    /// <summary>
    /// 등록된 FreeLook 카메라 중 입력된 타입의 카메라가 있다면 전환
    /// </summary>
    public void SwitchCamera(CameraType type)
    {
        var cam = type switch
        {
            CameraType.Default => defaultCamera,
            CameraType.Combat => combatCamera,
            CameraType.Aim => aimCamera,
            _ => null,
        };

        if (cam == null || cam == currentCamera) return;

        if (currentCamera != null) SyncCamera(currentCamera, cam);

        SetPriority(cam);
        currentCamera = cam;
    }

    private void SetPriority(CinemachineVirtualCameraBase active)
    {
        defaultCamera.Priority = 0;
        combatCamera.Priority = 0;
        aimCamera.Priority = 0;

        active.Priority = 10;
    }

    private void SyncCamera(CinemachineVirtualCameraBase from, CinemachineVirtualCameraBase to)
    {
        if (from == null || to == null) return;

        from.VirtualCameraGameObject.transform.GetPositionAndRotation(out Vector3 pos, out Quaternion rot);
        if (to is CinemachineVirtualCamera toCam) toCam.ForceCameraPosition(pos, rot);
        else if (to is CinemachineFreeLook toFreeLook)
        {
            toFreeLook.ForceCameraPosition(pos, rot);
            if (from == combatCamera && to == aimCamera) toFreeLook.m_XAxis.Value -= cameraAngleOverride;
            else if (from == aimCamera && to == combatCamera) toFreeLook.m_XAxis.Value += cameraAngleOverride;
        }
    }

    private void CameraRotation()
    {
        Vector3 viewDir = transform.position - new Vector3(mainCamera.transform.position.x, transform.position.y, mainCamera.transform.position.z);
        cameraOrientation.forward = viewDir.normalized;
    }

    private void CameraVerticalWeight()
    {
        // FreeLook가 아니면 디더링을 서서히 0으로 복귀
        var freeLook = CurrentFreeLook;
        float targetDither;

        if (freeLook == null)
        {
            targetDither = 0f;  // Screen-space UI 등 다른 카메라일 땐 완전 노출
        }
        else
        {
            // 현 FreeLook의 Y축 기반 목표 디더링 계산
            float yVal = Mathf.Clamp01(freeLook.m_YAxis.Value);
            float weight = Mathf.InverseLerp(0f, 0.15f, yVal);
            targetDither = 1f - weight;
        }

        // 상태 전용 보정 (Idle이면 0으로, Combat/Aim이면 위 결과 그대로)
        bool isIdle = CurrentState is IdleState || CurrentState is InteractionState;
        if (isIdle) targetDither = 0f;

        // 부드럽게 보간
        currentDitherValue = Mathf.Lerp(
            currentDitherValue,
            targetDither,
            Time.deltaTime * 5f);

        ApplyDither(currentDitherValue);
    }

    private void ApplyDither(float value)
    {
        if (toonRenderers == null || toonRenderers.Count == 0) return;

        foreach (var r in toonRenderers)
        {
            if (!r) continue;
            r.GetPropertyBlock(mpb);
            mpb.SetFloat(DitheringPropID, value);
            r.SetPropertyBlock(mpb);
        }
    }
    #endregion

    #region DAMAGE
    /// <summary>
    /// IDamageable 구현
    /// </summary>
    public void TakeDamage(DamageInfo info)
    {
        if (stateMachine.CurrentState is DeadState || isInvincible) return;

        // ── 1) 실질 데미지 처리 ────────────────────────────────
        stats.TakeDamage(info.damageAmount);

        var questID = QuestManager.Instance.PlayerQuestDataManager.TrackedQuestID;
        if (questID == 0 && QuestManager.Instance.PlayerQuestDataManager.QuestData.activeQuests.Count > 0)
            questID = QuestManager.Instance.PlayerQuestDataManager.QuestData.activeQuests.Keys.Max();
        if (questID == 0) questID = -1;

        var attackerMonster = info.attacker.GetComponent<Monster>();
        var attackerBoss = info.attacker.GetComponent<Boss>();
        if (attackerMonster == null && attackerBoss == null) return;
        int attackerID = attackerMonster != null
            ? attackerMonster.monsterData.monsterID
            : attackerBoss.bossData.bossID;

        if (attackerBoss != null)
        {
            string currentPattern = (((attackerBoss.stateMachine.currentState as MainState)
                ?.currentSubState as SubState)
                    ?.currentPattern as AttackPattern)
                        ?.GetType().Name ?? "None";

            AnalyticsManager.GetDamagedEvent(
                questID,
                weaponManager.CurrentData.ID,
                (int)weaponManager.CurrentData.Class,
                currentPattern,
                info.damageAmount
            );
        }
        else
        {
            AnalyticsManager.GetDamagedEvent(
                questID,
                weaponManager.CurrentData.ID,
                (int)weaponManager.CurrentData.Class,
                "None",
                info.damageAmount
            );
        }

        if (!animator.GetBool(PlayerAnimatorParams.SuperStance))
        {
            // ── 2) HitDirection 계산 ─────────────────────────────
            Vector3 toAttacker = info.attacker.transform.position - transform.position;
            toAttacker.y = 0f;
            float hitDir = Vector3.SignedAngle(transform.forward, toAttacker, Vector3.up);
            animator.SetFloat(PlayerAnimatorParams.HitDirection, hitDir);

            // ── 3) 넉백 벡터 & 물리 반응 ───────────────────────────
            knockbackDirection = -toAttacker.normalized;
            knockbackTimer = knockbackDuration;
        }

        // ── 4) 무적·리액션 트리거 ────────────────────────────
        isInvincible = true;
        invincibleTimer = invincibleDuration;

        if (stats.Runtime.CurrentHealth <= 0)
        {
            stats.Runtime.CurrentHealth = 0;
            stateMachine.ChangeState(dead);

            AnalyticsManager.CharacterDie(
                questID,
                attackerID,
                transform.position.x,
                transform.position.y
            );
        }
        else
        {
            animator.ResetTrigger(PlayerAnimatorParams.DodgeTrigger);
            if (!animator.GetBool(PlayerAnimatorParams.SuperStance))
                animator.SetTrigger(PlayerAnimatorParams.Hit);
            // GameManager.Instance.HitStop.DoHitStop();
            // GameManager.Instance.CameraShake.Shake();
        }

        // var pattern = (info.attacker.GetComponent<BossDoat>().states[BossMainState.NoneCombat] as DoatNoneCombatState)?.stateSelect?.attackPattern;
        // if (pattern != null)
        // {
        //     CurrentHitPattern = pattern.GetType().Name;
        //     // TODO: 패턴 추적 코드 작성
        // }
    }

    /// <summary>
    /// 넉백 처리
    /// </summary>
    private void TickKnockback()
    {
        if (knockbackTimer > 0)
        {
            knockbackTimer -= Time.deltaTime;
            characterController.Move(Time.deltaTime * knockbackPower * knockbackDirection);
        }
    }

    /// <summary>
    /// 피격 무적 처리
    /// </summary>
    private void TickInvincible()
    {
        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;
            if (invincibleTimer <= 0f) isInvincible = false;
        }
    }
    #endregion

    #region INTERACT
    /// <summary>
    /// 주변 상호작용 대상 탐지 및 하이라이트 처리
    /// </summary>
    private void CheckForInteractables()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactRange, interactableLayer);
        IInteractable nearest = null;
        float minDistance = float.MaxValue;

        foreach (Collider hit in hits)
        {
            if (hit.TryGetComponent(out IInteractable interactable))
            {
                float distance = Vector3.Distance(transform.position, hit.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = interactable;
                }
            }
        }

        if (nearest != CurrentInteractable)
        {
            CurrentInteractable?.HideHighlight();
            nearest?.ShowHighlight();
            CurrentInteractable = nearest;

            CurrentInteractableObject = CurrentInteractable switch
            {
                MonoBehaviour mb => mb.gameObject,
                _ => null
            };

            if (interactionPrompt != null)
            {
                string keyName;
                string displayName;
                string actionText;

                if (nearest != null)
                {
                    fadeTween?.Kill();
                    fadeTween = interactionUI.DOFade(1f, 0.5f).SetEase(Ease.OutCubic);
                    InteractableData data = CurrentInteractable?.GetInteractableData();

                    keyName = FormatKeyName(InteractionKeyName);
                    displayName = data.NameText;

                    if (data.ActionText.Replace(" ", "") == string.Empty)
                    {
                        actionText = data.Type switch
                        {
                            InteractableType.Object => "사용하기",
                            InteractableType.NPC => "대화하기",
                            InteractableType.Corpse => "코어 회수하기",
                            InteractableType.LostArticle => "줍기",
                            _ => string.Empty,
                        };
                    }
                    else
                    {
                        actionText = data.ActionText;
                    }

                    SetPrompt(keyName, displayName, actionText);
                    interactionPrompt.enabled = true;
                }
                else
                {
                    ResetInteractionUI();
                }
            }
        }
    }

    public void ResetInteractionUI()
    {
        fadeTween?.Kill();
        fadeTween = interactionUI.DOFade(0f, 0.5f).SetEase(Ease.OutCubic);
        interactionPrompt.text = string.Empty;
        interactionPrompt.enabled = false;

        CurrentInteractable = null;
    }
    #endregion

    #region COMBAT
    public void RegisterHostile(IHostile hostile)
    {
        if (hostile == null || CurrentHostiles.Contains(hostile))
            return;

        CurrentHostiles.Add(hostile);
        hostile.OnAggro();

        GameObject hostileMob = hostile switch
        {
            Monster monster => monster.gameObject,
            Boss boss => boss.gameObject,
            _ => null
        };

        if (hostileMob == null || CurrentHostileMobs.Contains(hostileMob))
            return;

        CurrentHostileMobs.Add(hostileMob);
    }

    public void UnregisterHostile(IHostile hostile)
    {
        if (hostile == null)
            return;

        if (CurrentHostiles.Remove(hostile))
        {
            hostile.OnLoseAggro();
        }

        GameObject hostileMob = hostile switch
        {
            Monster monster => monster.gameObject,
            Boss boss => boss.gameObject,
            _ => null
        };

        if (hostileMob != null)
            return;

        CurrentHostileMobs.Remove(hostileMob);
    }
    #endregion

    #region UTILITY
    public void SetCanRun(bool value)
    {
        canRun = value;
    }

    public void SetDodgeState(bool value)
    {
        IsInDodge = value;
        isInvincible = value;
        
        Debug.Log($"[PlayerController] 회피 {(value ? "시작" : "종료")}");
    }

    public void SetEquipState(bool value)
    {
        IsInEquip = value;
    }

    public void SetHandIK(bool value)
    {
        DOTween.Kill(handRig);
        handRig.DOWeight(value ? 1f : 0f, 0.25f)
            .SetEase(Ease.OutCubic);
    }

    public string FormatKeyName(string rawName)
    {
        if (string.IsNullOrEmpty(rawName))
            return "Unknown";

        var sb = new StringBuilder();
        foreach (char c in rawName)
        {
            if (char.IsUpper(c)) sb.Append(' ');
            sb.Append(c);
        }
        return System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(sb.ToString());
    }

    public void SetPrompt(string keyName, string displayName, string actionText)
    {
        sb.Clear();

        // 1) 키 가이드
        sb.Append($"[{keyName}]")
          .Append("   ")
          .Append(KEY_TAG_PREFIX);

        // 2) 대상 이름
        if (!string.IsNullOrEmpty(displayName))
        {
            sb.Append(displayName)
              .Append("  ");
        }

        // 3) 액션 설명
        sb.Append(actionText)
          .Append(KEY_TAG_SUFFIX);

        interactionPrompt.SetText(sb);
    }
    #endregion

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showGizmo) return;
        if (EditorApplication.isPlaying && (stateMachine == null || stateMachine.CurrentState != idle)) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            center: characterController.center,
            radius: interactRange
        );
    }
#endif
}
