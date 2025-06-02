using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Chicken : Monster
{
    [Header("치킨 전용 설정")]
    [Tooltip("피격 시 뒤로 밀려날 거리")]
    public float knockbackDistance = 1.5f;

    [Tooltip("저체력으로 간주할 HP 비율 (0.0 ~ 1.0). 이 비율 이하가 되면 스폰 지점으로 도망갑니다.")]
    public float lowHealthThreshold = 0.1f; // 예: 10%

    [Tooltip("Idle 상태에서 배회할 때, 기본 배회 반경(MonsterData 또는 Monster 클래스)에 곱할 배율입니다.")]
    public float wanderRadiusMultiplier = 1.5f; // 예: 1.5배 더 넓게 배회

    [Tooltip("넉백 후 이동을 재개하기까지 걸리는 시간(초)")]
    public float knockbackRecoveryTime = 0.3f; // ★ 넉백 후 0.3초 뒤 이동 재개

    // 내부 상태 추적용 변수
    private Coroutine knockbackCoroutine = null; // 현재 넉백 처리 코루틴 참조

    // --- 초기화 관련 ---

    /// <summary>
    /// Awake는 게임 오브젝트가 활성화될 때 호출됩니다.
    /// 부모 클래스의 Awake를 호출하여 기본 컴포넌트를 초기화하고,
    /// NavMeshAgent 컴포넌트를 가져와 치킨에 맞게 일부 설정을 조정합니다.
    /// </summary>
    protected override void Awake()
    {
        // 부모 초기화 필수
        base.Awake();
        agent = GetComponent<NavMeshAgent>();
        if (agent != null) { agent.speed = MoveSpeed; /* ... Agent 추가 설정 ... */ }
        else Debug.LogError("Chicken 프리팹에 NavMeshAgent 없음!", this);
    }

    protected override void Update()
    {
        base.Update();
        float speed = agent.velocity.magnitude;
        animator.SetFloat("Speed", speed);
    }
    /// <summary>
    /// 몬스터 초기화 시 부모 초기화.
    /// </summary>
    public override void InitializeMonster()
    {
        base.InitializeMonster();
        // 초기화 시 진행 중인 넉백 코루틴이 있다면 중지
        if (knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
            knockbackCoroutine = null;
            // Agent가 멈춰있을 수 있으므로 초기 Idle 상태 진입 시 다시 정지됨
        }
    }

    /// <summary>
    /// 닭이 데미지를 받을 때 호출, 넉백 및 이동 재개 로직이 추가
    /// </summary>
    public override void TakeDamage(DamageInfo info) // <<< override 추가, 파라미터 DamageInfo
    {
        // 죽었거나 스폰 지점으로 도망 중이면 무시
        if (currentStateEnum == MonsterState.Dead || currentStateEnum == MonsterState.FleeToSpawn)
        {
            return;
        }

        // 데미지가 0 이하이면 무시
        if (info.damageAmount <= 0) return;

        PlayHitSound(); // 부모 클래스 함수

        // 체력 감소
        int damageToApply = Mathf.FloorToInt(info.damageAmount);
        currentHp -= damageToApply;        

        // 죽음 처리
        if (currentHp <= 0)
        {
            currentHp = 0;
            ChangeState(MonsterState.Dead); // 죽음 상태로 전환
            
        }
        else // 아직 살아있으면
        {
            animator.SetTrigger("Is_D_Hit");
            // 공격자 정보 갱신
            if (info.attacker != null) CheckAttacker(info.attacker);

            // 1. 온순화 상태(isPacified)에서 피격 처리
            if (isPacified)
            {
                Debug.Log($"Chicken {gameObject.name} was pacified, resetting state and becoming hostile.");
                ResetPacifiedState(); // 온순화 해제
                EvaluateNewAttacker(info.attacker); // 공격자를 타겟으로
                ChangeState(MonsterState.Attack); // 공격 상태로 전환
                return; // 다른 반응 안 함
            }

            // 2. 저체력 도망 처리 (온순화 아닐 때)
            if (monsterData != null && (float)currentHp / monsterData.maxHp <= lowHealthThreshold)
            {
                Debug.Log($"Chicken {gameObject.name} has low health. Changing state to FleeToSpawn.");
                ChangeState(MonsterState.FleeToSpawn); // 스폰 지점으로 도망 상태 전환
                return; // 다른 반응 안 함
            }

            // 3. 일반 피격 처리 (온순화X, 저체력X 일 때) - 상태별 반응 호출 또는 직접 처리

            // ★ 방법 A: 상태 객체에 반응 위임 (권장) ★
            // currentStateObject?.OnTakeDamage(info);

            // ★ 방법 B: 여기서 직접 상태 전환 로직 구현 (기존 코드 방식) ★
            if (currentStateEnum == MonsterState.Idle)
            {
                Debug.Log($"Chicken {gameObject.name} damaged while Idle. Evaluating attacker and changing to Attack state.");
                EvaluateNewAttacker(info.attacker);
                if (GetCurrentTarget() != null) ChangeState(MonsterState.Attack);
            }
            // TODO: 필요하다면 다른 상태(Chase 등)일 때의 반응도 여기에 추가
        }
    }

    private IEnumerator KnockbackCoroutine(GameObject attacker)
    {
        // 1. 넉백 방향 계산
        Vector3 knockbackDir = (transform.position - attacker.transform.position).normalized;
        knockbackDir.y = 0; // 수평으로만

        // 2. 이동 중지 (넉백 중에는 스스로 움직이지 않음)
        StopMovement(); // agent.isStopped = true;

        // 3. 넉백 이동 적용 (Move 메서드는 물리 영향을 받지 않고 강제 이동)
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            // 여기서는 즉시 이동
            agent.Move(knockbackDir * knockbackDistance);
            // Warp는 NavMesh 위로 순간이동시키므로 넉백 느낌이 안 살 수 있음
            // agent.Warp(transform.position + knockbackDir * knockbackDistance);
        }

        // 4. 넉백 회복 시간만큼 대기
        yield return new WaitForSeconds(knockbackRecoveryTime);

        // 5. 이동 재개 (Agent가 다시 경로를 따라가도록 허용)
        //    단, 현재 상태가 여전히 공격/추적 등을 해야 하는 상태일 때만 재개
        if (currentStateEnum == MonsterState.Attack || currentStateEnum == MonsterState.Idle || currentStateEnum == MonsterState.Flee) // Dead, FleeToSpawn 제외
        {
            // Debug.Log("넉백 회복 후 이동 재개");
            if (agent != null && agent.enabled) agent.isStopped = false;
        }

        knockbackCoroutine = null; // 코루틴 참조 해제
    }


    // --- 배회 반경 재정의 ---
    public override float WanderRadius => base.WanderRadius * wanderRadiusMultiplier;

    // --- 리셋 ---
    public override void ResetMonster() { base.ResetMonster(); }
}
