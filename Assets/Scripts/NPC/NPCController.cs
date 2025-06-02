using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

public class NPCController : MonoBehaviour,IDamageable
{
    [Header("Data (Assign in Inspector)")]
    public NPCData npcData;

    [Header("Player Following Status")]
    [SerializeField] // 디버깅용
    private bool _isActivelyFollowingPlayer = false; // 플레이어가 '함께하기'를 눌렀는지 여부
    public bool IsActivelyFollowingPlayer => _isActivelyFollowingPlayer;

    [Header("Weapon Status")]
    [SerializeField] // 디버깅용으로 Inspector에서 볼 수 있게
    private bool _isWeaponDrawn = false; // 현재 칼을 뽑은 상태인지
    public bool IsWeaponDrawn => _isWeaponDrawn;

    public const string Draw = "Draw";    
    public const string Sheathe = "Sheathe"; 
    public const string IsWeaponDrawne = "IsWeaponDrawn";

    private Vector3 _initialPosition; // NPC의 원래 스폰/대기 위치
    public Vector3 InitialPosition => _initialPosition;
    private Quaternion _initialRotation; // NPC의 원래 방향
    public Quaternion InitialRotation => _initialRotation;

    [Header("Runtime Status")]
    [SerializeField] // 인스펙터에서 보기 위함 (디버깅용)
    private int _currentHp;
    public int CurrentHp => _currentHp;
    public bool IsInCombatParty { get; private set; } = false;
    public bool IsFainted { get; private set; } = false;
    public bool IsFullyInitialized { get; private set; } = false;

    [Header("Required Components (Auto-fetched or Assign)")]
    public Animator npcAnimator;
    public NavMeshAgent navMeshAgent;
    public NPCAffinity affinityComponent;
    //public NPCSkillUser skillUserComponent;
    public NPCStateMachine stateMachineComponent;
    public NPCInteractionHandler interactionHandlerComponent;
    // public NPCDialogueSystem dialogueComponent;

    private SkillData _currentSkillForAnimationEvent; // ★★★ 현재 애니메이션 이벤트가 처리해야 할 스킬 ★★★
    private GameObject _currentTargetForAnimationEvent;

    // 이벤트
    public event System.Action OnFainted;
    public event System.Action OnRevived;
    public event System.Action<NPCController, bool> OnCombatEligibilityChanged;
    public event System.Action<NPCController, bool> OnFollowingStatusChanged;


    protected virtual void Awake()
    {
        // 컴포넌트 찾기 또는 할당 확인
        npcAnimator = GetComponentInChildren<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        affinityComponent = GetComponent<NPCAffinity>();
        //skillUserComponent = GetComponent<NPCSkillUser>();
        stateMachineComponent = GetComponent<NPCStateMachine>();
        interactionHandlerComponent = GetComponent<NPCInteractionHandler>();

        // 필수 컴포넌트 null 체크
        if (npcData == null)
        {
            Debug.LogError($"[{gameObject.name}] NPCData is not assigned!", this);
            enabled = false;
            return;
        }
        if (npcAnimator == null)
        { Debug.LogError($"[{gameObject.name}] Animator component not found!", this); enabled = false; return; }
        if (navMeshAgent == null && npcData.npcType != NPCType.Merchant) { Debug.LogWarning($"[{gameObject.name}] NavMeshAgent component not found. NPC might not move.", this); } // 상인 등은 이동 안할 수 있음
        if (affinityComponent == null)
        {
            Debug.LogError($"[{gameObject.name}] NPCAffinity component not found!", this);
            enabled = false;
            return;
        }
        if (stateMachineComponent == null)
        {
            Debug.LogError($"[{gameObject.name}] NPCStateMachine component not found!", this);
            enabled = false;
            return;
        }
        if (interactionHandlerComponent == null)
        {
            Debug.LogWarning($"[{gameObject.name}] NPCInteractionHandler component not found. NPC might not be interactable.", this);
        }
        _initialPosition = transform.position; // 초기 위치 저장
        _initialRotation = transform.rotation; // 초기 방향 저장
    }

    protected virtual void Start()
    {
        InitializeNPC();
    }

    public virtual void InitializeNPC()
    {
        _currentHp = npcData.baseMaxHp;
        IsInCombatParty = false; // 초기에는 파티 참여 안 함
        IsFainted = false;

        affinityComponent.Initialize(this);
        if (GameManager.Instance != null)
        {
            NPCSkillUser centralSkillUser = GameManager.Instance.GetComponent<NPCSkillUser>();
            if (centralSkillUser != null)
            {
                // Debug.LogWarning($"[NPCController InitializeNPC] NPC [{npcData?.npcName}]에 대해 GameManager의 NPCSkillUser.RegisterAndInitializeNpc 호출 시도.");
                centralSkillUser.RegisterAndInitializeNpc(this);
            }
            else
            {
                Debug.LogError($"[NPCController InitializeNPC] GameManager에 NPCSkillUser 컴포넌트를 찾을 수 없습니다! NPC [{npcData?.npcName}] 스킬 초기화 실패.");
            }
        }
        else
        {
            Debug.LogError($"[NPCController InitializeNPC] GameManager 인스턴스를 찾을 수 없습니다! NPC [{npcData?.npcName}] 스킬 초기화 실패.");
        }
        stateMachineComponent.Initialize(this);
        if (interactionHandlerComponent != null)
        {
            interactionHandlerComponent.Initialize(this);
        }
        _isActivelyFollowingPlayer = false; // 초기에는 따라다니지 않음
        transform.position = _initialPosition; // 초기화 시 원래 위치로
        transform.rotation = _initialRotation; // 초기화 시 원래 방향으로

        IsFullyInitialized = true; // 초기화 완료 플래그
        // Debug.LogWarning($"[NPCController InitializeNPC] NPC: {npcData?.npcName} 초기화 완료. IsInCombatParty: {IsInCombatParty}, IsActivelyFollowingPlayer: {IsActivelyFollowingPlayer}");
    }
    protected virtual void Update()
    {
        // 현재 상태의 Execute() 호출 (NPCStateMachine이 있다면 그쪽에서 처리할 수도 있음)
        // 또는 stateMachineComponent가 null일 경우를 대비해 null 체크 후 호출
        stateMachineComponent?.CurrentState?.Execute(); // 예시: NPCStateMachine을 통해 현재 상태의 Execute 호출

        // 매 프레임 애니메이터 파라미터 업데이트
        UpdateAnimatorParameters();
    }
    public void TriggerSkillEffect() // SkillData.skillEffectEventName에 "TriggerSkillEffect"를 사용한다고 가정
    {
        if (_currentSkillForAnimationEvent == null)
        {
            Debug.LogWarning($"[{npcData?.npcName}] TriggerSkillEffect 호출되었으나, _currentSkillForAnimationEvent가 null입니다.");
            return;
        }

        if (GameManager.Instance != null)
        {
            NPCSkillUser centralSkillUser = GameManager.Instance.GetComponent<NPCSkillUser>();
            if (centralSkillUser != null)
            {
                Debug.Log($"[{npcData?.npcName}] 애니메이션 이벤트 발생! 스킬 [{_currentSkillForAnimationEvent.skillName}] 효과 적용 시도. 타겟: {_currentTargetForAnimationEvent?.name ?? "없음"}");
                centralSkillUser.ExecuteSkillEffectInternal(this, _currentSkillForAnimationEvent, _currentTargetForAnimationEvent);
            }
            else Debug.LogError($"[{npcData?.npcName}] GameManager에 NPCSkillUser가 없습니다 (TriggerSkillEffect).");
        }
        else Debug.LogError($"[{npcData?.npcName}] GameManager 인스턴스가 없습니다 (TriggerSkillEffect).");

        // 선택적: 스킬 효과 적용 후 이 변수들을 초기화할 수 있으나,
        // 한 애니메이션에서 여러 번 이벤트가 발생할 가능성도 고려해야 함.
        // 보통은 다음 스킬 사용 시점에 _currentSkillForAnimationEvent가 갱신됨.
        // _currentSkillForAnimationEvent = null;
        // _currentTargetForAnimationEvent = null;
    }
    public void PrepareForSkillAnimationEvent(SkillData skill, GameObject target)
    {
        _currentSkillForAnimationEvent = skill;
        _currentTargetForAnimationEvent = target;
        Debug.Log($"[{npcData?.npcName}] 스킬 애니메이션 이벤트 준비: 스킬 [{skill?.skillName}], 타겟 [{target?.name ?? "없음"}]");
    }
    protected virtual void UpdateAnimatorParameters()
    {
        if (npcAnimator == null)
        {
            return;
        }

        bool hasSpeedParam = false;
        foreach (AnimatorControllerParameter param in npcAnimator.parameters)
        {
            if (param.name == "Speed" && param.type == AnimatorControllerParameterType.Float)
            {
                hasSpeedParam = true;
                break;
            }
        }

        if (!hasSpeedParam)
        {
            // Debug.LogWarning($"[{gameObject.name}] Animator에 'Speed' Float 파라미터가 없습니다. 이동 애니메이션이 작동하지 않을 수 있습니다.");
            return;
        }

        float currentSpeed = 0f;

        if (navMeshAgent != null && navMeshAgent.enabled && navMeshAgent.isOnNavMesh)
        {
            currentSpeed = navMeshAgent.velocity.magnitude;
        }
        currentSpeed= Mathf.Clamp(currentSpeed, 0f, 4f);
        // Animator의 "Speed" 파라미터에 현재 속도 값을 설정합니다.
        // 블렌드 트리는 이 값을 사용하여 Idle과 Run 애니메이션 사이를 블렌딩합니다.
        npcAnimator.SetFloat("Speed", currentSpeed, 0.1f, Time.deltaTime); // 0.1f는 부드러운 전환을 위한 damp time

        // 디버깅 로그 (필요시 주석 해제)
        // if (currentSpeed > 0.01f || (npcAnimator.GetFloat("Speed") > 0.01f && currentSpeed <= 0.01f))
        // {
        //     Debug.Log($"[{gameObject.name}] Updated Animator Speed: {currentSpeed:F2}");
        // }
    }
    /// <summary>
    /// 스킬 애니메이션이 완료되었을 때 애니메이션 이벤트에 의해 호출됩니다.
    /// NPC의 상태를 다음 상태(예: CombatIdle)로 변경합니다.
    /// </summary>
    public void OnSkillAnimationComplete() 
    {
        if (stateMachineComponent != null)
        {
            // 현재 상태가 UsingSkill일 때만 CombatIdle로 변경 (안전장치)
            // 또는 어떤 스킬을 사용했는지에 따라 다른 상태로 갈 수도 있음
            if (stateMachineComponent.CurrentState is NPCUsingSkillState)
            {
                Debug.Log($"[{npcData?.npcName}] 스킬 애니메이션 완료 (OnSkillAnimationComplete 이벤트). CombatIdle 상태로 전환.");
                npcAnimator.SetInteger("SkillNum",0);
                stateMachineComponent.ChangeState(NPCState.CombatIdle);
            }
            else
            {
                Debug.LogWarning($"[{npcData?.npcName}] OnSkillAnimationComplete 호출되었으나 현재 상태가 UsingSkill이 아님: {stateMachineComponent.CurrentState?.GetType().Name}");
            }
        }
    }

    public virtual void UpdateCombatPartyStatus(bool canJoinPotential)
    {
        Debug.Log($"[NPCController] UpdateCombatPartyStatus 호출됨. NPC: {npcData?.npcName}, canJoinPotential: {canJoinPotential}, 이전 IsInCombatParty: {IsInCombatParty}");
        bool previousPotential = IsInCombatParty; // IsInCombatParty는 여전히 '조건상 참여 가능한지'
        IsInCombatParty = canJoinPotential;

        if (previousPotential != IsInCombatParty)
        {
            Debug.Log($"NPC [{npcData.npcName}] 전투 참여 '가능' 상태 변경: {IsInCombatParty}");
            // UI 업데이트를 위해 이벤트 발행 (예: QuestLogUI 등에서 이 NPC의 '함께하기' 버튼 상태 업데이트)
            OnCombatEligibilityChanged?.Invoke(this, IsInCombatParty); // 새로운 이벤트 (아래 정의)

            if (!IsInCombatParty && _isActivelyFollowingPlayer)
            {
                // 전투 참여 자격이 없어졌는데 따라다니고 있었다면 강제로 해제
                SetPlayerFollowingStatus(false);
            }
        }
    }

    public void SetPlayerFollowingStatus(bool shouldFollow)
    {
        if (_isActivelyFollowingPlayer == shouldFollow) return;

        if (shouldFollow && !IsInCombatParty) // '함께하기'를 눌렀는데 전투 참여 자격이 없다면
        {
            Debug.LogWarning($"NPC [{npcData.npcName}]는 현재 전투에 참여할 수 없어 함께할 수 없습니다.");
            // UI 등에서 이 버튼이 활성화되지 않도록 하는 것이 우선이지만, 방어 코드
            return;
        }

        _isActivelyFollowingPlayer = shouldFollow;
        Debug.Log($"NPC [{npcData.npcName}] 플레이어 실제 추종 상태 변경: {_isActivelyFollowingPlayer}");

        if (_isActivelyFollowingPlayer)
        {
            stateMachineComponent.ChangeState(NPCState.FollowPlayer);
        }
        else
        {
            stateMachineComponent.ChangeState(NPCState.ReturningToPost);
        }
        OnFollowingStatusChanged?.Invoke(this, _isActivelyFollowingPlayer);
    }

    public virtual void TakeDamage(DamageInfo info) // 공격자 정보도 받으면 좋음
    {
        if (IsFainted || info.damageAmount <= 0) return; // 이미 기절했거나 데미지가 없으면 무시

        // 필요하다면 여기서 info.attacker (공격자) 정보를 활용할 수 있습니다.
        // 예: 특정 공격자에게는 덜 아프게 맞는다거나, 어그로 관리 등

        // 실제 적용될 데미지 계산 (방어력, 저항 등은 여기서 또는 DamageInfo 생성 시점에 미리 계산될 수 있음)
        int damageToApply = Mathf.FloorToInt(info.damageAmount); // 여기서는 DamageInfo의 값을 그대로 사용

        _currentHp -= damageToApply;
        Debug.Log($"NPC [{npcData?.npcName ?? gameObject.name}] took {damageToApply} damage from [{info.attacker?.name ?? "Unknown Attacker"}]. HP: {_currentHp}/{npcData?.baseMaxHp ?? _currentHp + damageToApply}");


        if (_currentHp <= 0)
        {
            _currentHp = 0;
            Faint();
        }
        else
        {            
             if (npcAnimator != null)
            {
                npcAnimator.SetTrigger("Hit");
            }

            // 상태 머신에 피격 이벤트 전달 (공격자 정보도 함께 전달)
            stateMachineComponent?.OnDamaged(info); // info.attacker를 전달
        }
    }

    protected virtual void Faint()
    {
        if (IsFainted) return;

        IsFainted = true;
        IsInCombatParty = false; // 기절 시 파티에서 이탈 (또는 비활성화)
        if (npcAnimator != null)
            // {
            //     // npcAnimator.SetTrigger("FaintTrigger"); // 또는 animator.Play("FaintState");
            // }
            stateMachineComponent.ChangeState(NPCState.Fainted); // FSM 상태 변경
        OnFainted?.Invoke();
        Debug.Log($"NPC [{npcData.npcName}] fainted.");
        // TODO: 일정 시간 후 부활 또는 다른 부활 조건 처리
    }

    public virtual void Revive(int healthRestoreAmount)
    {
        if (!IsFainted) return;

        IsFainted = false;
        _currentHp = Mathf.Clamp(healthRestoreAmount, 1, npcData.baseMaxHp);
        // 호감도에 따라 전투 참여 가능 여부 다시 판단
        affinityComponent.TriggerAffinityLevelCheck(); // 호감도 레벨 체크 강제 실행
        // stateMachineComponent.ChangeState(NPCState.Idle); // 부활 후 기본 상태로
        OnRevived?.Invoke();
        Debug.Log($"NPC [{npcData.npcName}] revived with {_currentHp} HP.");
    }

    // 다른 시스템에서 NPC의 스탯(예: 호감도로 인한 보너스)을 적용할 때 사용
    public virtual void ApplyStatModifier(StatModifierData modifier)
    {
        // TODO: 실제 스탯 시스템과 연동
        // 예: if (modifier.statToModify == StatModifierData.StatType.Health) npcData.baseMaxHp += (int)modifier.value;
        Debug.Log($"NPC [{npcData.npcName}] 스탯 보너스 적용 시도: {modifier.statToModify} +{modifier.value}");
    }
    public void RequestDrawWeapon()
    {
        if (!_isWeaponDrawn && npcAnimator != null) // 아직 안 뽑았을 때만
        {
            Debug.Log($"[{npcData?.npcName}] RequestDrawWeapon 호출. 칼을 뽑습니다.");
            npcAnimator.SetTrigger(Draw);
            _isWeaponDrawn = true;
            // 선택적: Animator의 Bool 파라미터도 업데이트
            // npcAnimator.SetBool(AP_IS_WEAPON_DRAWN, true);
        }
        else if (_isWeaponDrawn)
        {
            // Debug.Log($"[{npcData?.npcName}] RequestDrawWeapon: 이미 칼을 뽑은 상태입니다.");
        }
        else if (npcAnimator == null) Debug.LogWarning("Animator가 없습니다 (RequestDrawWeapon).");
    }

    /// <summary>
    /// 무기를 넣도록 요청하고 애니메이션을 트리거합니다.
    /// </summary>
    public void RequestSheatheWeapon()
    {
        if (_isWeaponDrawn && npcAnimator != null) // 뽑았을 때만
        {
            Debug.Log($"[{npcData?.npcName}] RequestSheatheWeapon 호출. 칼을 넣습니다.");
            npcAnimator.SetTrigger(Sheathe);
            _isWeaponDrawn = false;
            // 선택적: Animator의 Bool 파라미터도 업데이트
            // npcAnimator.SetBool(AP_IS_WEAPON_DRAWN, false);
        }
        else if (!_isWeaponDrawn)
        {
            // Debug.Log($"[{npcData?.npcName}] RequestSheatheWeapon: 이미 칼을 넣은 상태입니다.");
        }
        else if (npcAnimator == null) Debug.LogWarning("Animator가 없습니다 (RequestSheatheWeapon).");
    }
    public static float GetColliderRadius(Collider col)
    {
        if (col == null) return 0f;

        if (col is CapsuleCollider capsule)
        {
            // CapsuleCollider의 경우, 키가 큰 방향의 축을 제외한 반경을 사용
            // 보통 x 또는 z 스케일 중 하나를 사용하거나, 평균을 낼 수 있습니다.
            // 여기서는 간단히 radius를 사용합니다. 더 정확하게는 bounds를 사용할 수도 있습니다.
            return capsule.radius * Mathf.Max(capsule.transform.lossyScale.x, capsule.transform.lossyScale.z);
        }
        else if (col is SphereCollider sphere)
        {
            return sphere.radius * GetMaxAbsScale(sphere.transform.lossyScale);
        }
        else if (col is BoxCollider box)
        {
            // BoxCollider의 경우, 어떤 면을 기준으로 할지 정의해야 합니다.
            // 여기서는 가장 넓은 면의 절반을 기준으로 하거나, 평균적인 반경을 계산할 수 있습니다.
            // 간단하게는 bounds의 extents 중 x, z의 평균을 사용할 수 있습니다.
            Vector3 extents = box.bounds.extents; // 월드 스케일이 적용된 크기의 절반
            return (extents.x + extents.z) / 2f;
            // 또는 return Mathf.Max(extents.x, extents.z);
        }
        // 다른 타입의 콜라이더에 대한 처리 추가 가능

        // 기본적으로 bounds를 사용하여 대략적인 반경 계산
        // (정확하지 않을 수 있으나 범용적)
        Bounds bounds = col.bounds;
        return (bounds.extents.x + bounds.extents.z) / 2f; // XZ 평면에서의 평균 반경
    }

    private static float GetMaxAbsScale(Vector3 scale)
    {
        return Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
    }
}
