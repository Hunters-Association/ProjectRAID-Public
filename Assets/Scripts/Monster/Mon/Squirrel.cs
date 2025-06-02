using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(NavMeshAgent))]
public class Squirrel : Monster
{
    // --- 초기화 ---
    protected override void Awake()
    {
        base.Awake();
        agent = GetComponent<NavMeshAgent>();
        if (agent != null) agent.speed = MoveSpeed;
    }
    protected override void Update()
    {
        base.Update();
        float speed = agent.velocity.magnitude;
        animator.SetFloat("Speed", speed);
    }
    public override void InitializeMonster()
    {
        base.InitializeMonster();
        if (agent != null)
        {
            agent.enabled = true;
            if (!agent.Warp(spawnPosition))
            { }
            agent.isStopped = true;
        }
    }

    // --- 피격 처리 (Flee 우선) ---
    public override void TakeDamage(DamageInfo info)
    {
        if (currentStateEnum == MonsterState.Dead || info.damageAmount <= 0) return; // 이미 죽었거나 도망 중이면 무시
        PlayHitSound();
        int damageToApply = Mathf.FloorToInt(info.damageAmount);
        currentHp -= damageToApply;
        if (currentHp <= 0)
        {
            currentHp = 0;
            ChangeState(MonsterState.Dead); // 죽음 상태로 전환
        }
        else // 아직 살아있다면
        {
            // 공격자 정보 갱신 (선택적이지만, 누가 때렸는지 기록은 필요할 수 있음)
            if (info.attacker != null)
            {
                CheckAttacker(info.attacker);
                // 다람쥐는 보통 특정 타겟을 추격하지 않으므로 EvaluateNewAttacker는 불필요할 수 있음
                // monster.EvaluateNewAttacker(info.attacker);
            }

            // ★ 무조건 Flee 상태로 전환 ★
            // (현재 상태가 이미 Flee거나 FleeToSpawn이어도 다시 Flee 상태 진입 로직 실행)
            Debug.Log($"{monsterData?.name ?? gameObject.name} was damaged. Changing state to Flee.");
            ChangeState(MonsterState.Flee);
        } 
    }

    // --- 나머지 ---
    // 부모 것 사용
    public override void ResetMonster()
    {
        if (agent != null && !agent.enabled)
            agent.enabled = true; base.ResetMonster();
    } // isPacified도 부모가 리셋
}
