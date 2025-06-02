using UnityEngine;
using ProjectRaid.EditorTools;
using ProjectRaid.Extensions;

/// <summary>
/// ────────────────────────────────────────────────────────────────
///  PlayerLocomotionController
///  • CharacterController 이동을 처리
///  • Animator 파라미터 3개(Speed·MoveX·MoveY) 업데이트
///  • 카메라 방향 기준 이동 + 부드러운 회전
/// ────────────────────────────────────────────────────────────────
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(Animator))]
[DisallowMultipleComponent]
public class PlayerLocomotionController : MonoBehaviour
{
    [FoldoutGroup("속도", ExtendedColor.Orange)]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float acceleration = 12f;      // 가속 / 감속
    [SerializeField] private float rotationSpeed = 720f;    // 회전 속도(°/s)

    [FoldoutGroup("중력", ExtendedColor.GreenYellow)]
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundRadius = 0.25f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundMask;

    [FoldoutGroup("카메라", ExtendedColor.Cyan)]
    [SerializeField] private Transform cam;

    [FoldoutGroup("디버그/기즈모", ExtendedColor.DodgerBlue)]
    [SerializeField] private bool showGizmo = false;
    [SerializeField] private Mesh gizmoMesh;
    [SerializeField] private Color gizmoColor;

    [FoldoutGroup("디버그/로그", ExtendedColor.DodgerBlue)]
    [SerializeField] private bool showLog = false;


    public PlayerInputHandler Input { get; private set; }
    private CharacterController cc;
    private Animator anim;

    private Vector3 velocity;       // 실제 이동 속도 (월드)
    private Vector3 moveVelocity;   // 입력이 만들어내는 목표 속도

    private readonly int hashSpeed = Animator.StringToHash("Speed");
    private readonly int hashMoveX = Animator.StringToHash("MoveX");
    private readonly int hashMoveY = Animator.StringToHash("MoveY");


    #region INIT
    // ────────────────────────────────────────────────────────────────
    public void Init(PlayerController player)
    {
        if (showLog) Debug.Log("초기화 완료!");
    }
    #endregion


    #region LIFE-CYCLE
    // ────────────────────────────────────────────────────────────────
    private void Awake()
    {
        Input = GetComponent<PlayerInputHandler>();
        cc = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        if (!cam) cam = Camera.main.transform;
    }

    // ────────────────────────────────────────────────────────────────
    private void Update()
    {
        UpdateMoveInput();
        ApplyGravity();
        MoveCharacter();
        UpdateAnimator();
    }
    #endregion


    #region MOVEMENT
    // ────────────────────────────────────────────────────────────────
    private void UpdateMoveInput()
    {
        // 1) 입력 읽기
        Vector2 input = Input.MoveInput;
        input = Vector2.ClampMagnitude(input, 1f);

        // 2) 입력 벡터를 카메라 기준 월드 방향으로 변환
        Vector3 camF = cam.forward; camF.y = 0;
        Vector3 camR = cam.right; camR.y = 0;

        Vector3 wishDir = camF.normalized * input.y + camR.normalized * input.x;
        bool isMoving = wishDir.sqrMagnitude > 0.0001f;

        // 3) 목표 속도 결정
        float targetSpeed = (Input.IsRunning ? runSpeed : walkSpeed) * (isMoving ? 1f : 0f);
        Vector3 targetVelocity = wishDir.normalized * targetSpeed;

        // 4) 가속 / 감속
        moveVelocity = Vector3.MoveTowards(moveVelocity, targetVelocity, acceleration * Time.deltaTime);

        // 5) 캐릭터가 움직이는 방향으로 부드럽게 회전
        if (isMoving)
        {
            Quaternion lookRot = Quaternion.LookRotation(wishDir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRot, rotationSpeed * Time.deltaTime);
        }
    }

    // ────────────────────────────────────────────────────────────────
    private void ApplyGravity()
    {
        bool grounded = Physics.CheckSphere(groundCheck.position, groundRadius, groundMask, QueryTriggerInteraction.Ignore);

        if (grounded && velocity.y < 0f)
            velocity.y = -2f; // 바닥에 붙이는 작은 마이너스 값

        velocity.y += gravity * Time.deltaTime;
    }

    // ────────────────────────────────────────────────────────────────
    private void MoveCharacter()
    {
        // 수평 이동 + 중력 모두 CharacterController로
        Vector3 total = moveVelocity * Time.deltaTime;
        total.y += velocity.y * Time.deltaTime;
        cc.Move(total);
    }
    #endregion


    #region ANIMATION
    // ────────────────────────────────────────────────────────────────
    private void UpdateAnimator()
    {
        // 1) Speed : 0(Idle)~2(Walk)~6(Run) 구간
        float speed01 = moveVelocity.magnitude / runSpeed; // 0~1
        anim.SetFloat(hashSpeed, speed01 * runSpeed, 0.15f, Time.deltaTime);

        // 2) 방향 : 캐릭터 로컬 기준 정규화 벡터
        Vector3 localVel = transform.InverseTransformDirection(moveVelocity);
        Vector3 dir = localVel.normalized; // |dir| = 1 (또는 0)
        anim.SetFloat(hashMoveX, dir.x, 0.1f, Time.deltaTime);
        anim.SetFloat(hashMoveY, dir.z, 0.1f, Time.deltaTime);
    }
    #endregion


#if UNITY_EDITOR
    #region EDITOR
    // ────────────────────────────────────────────────────────────────
    // Gizmo로 지면 판정 영역 확인
    private void OnDrawGizmosSelected()
    {
        if (!showGizmo) return;
        if (!groundCheck) return;

        Gizmos.color = gizmoColor;

        if (gizmoMesh != null)
            Gizmos.DrawMesh(
                mesh:       gizmoMesh,
                position:   groundCheck.position,
                rotation:   transform.localRotation,
                scale:      Vector3.one * groundRadius
            );
        else
            Gizmos.DrawWireSphere(
                center:     groundCheck.position,
                radius:     groundRadius
            );
    }
    #endregion
#endif
}
