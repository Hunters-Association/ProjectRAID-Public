using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MorvenIdleState : MonsterBaseState
{
    private Morven morven;
    private float timeUntilNextAction = 0f;
    private float decideActionInterval = 2.0f;
    

    public MorvenIdleState(Monster contextMonster) : base(contextMonster)
    {
        morven = contextMonster as Morven;
        if (morven == null) Debug.LogError("MorvenIdleState created with non-Morven monster!");
    }

    public override void EnterState()
    {
        if (morven == null) return;
        morven.StopMovement();
        timeUntilNextAction = Random.Range(decideActionInterval * 0.5f, decideActionInterval * 1.5f);
        // 온순화 상태 로직은 유지 가능 (필요하다면)
    }

    public override void UpdateState()
    {
        if (morven == null) return;

        // 플레이어 감지 시 Attack 상태 전환 (유지)
        if (morven.DetectPlayer())
        {
            morven.ChangeState(MonsterState.Attack);
            return;
        }

        // 다음 행동 결정 시간 체크 (유지)
        timeUntilNextAction -= Time.deltaTime;
        if (timeUntilNextAction <= 0)
        {
            DecideNextIdleAction();
            timeUntilNextAction = Random.Range(decideActionInterval * 0.5f, decideActionInterval * 1.5f);
        }
    }

    // ★★★ DecideNextIdleAction 메서드 단순화 ★★★
    private void DecideNextIdleAction()
    {
        // Idle 상태에서는 이제 배회(Wander) 또는 다른 비전투 행동만 고려
        WanderInstead(); // 예: 무조건 배회 시도
        // 또는 확률적으로 숨쉬기 애니메이션 재생 등 추가 가능
    }
    // ★★★★★★★★★★★★★★★★★★★★★★★★★★★

    private void WanderInstead()
    {
        if (monster == null) return; // monster 참조 사용
        if (monster.GetWanderPositionNavMesh(monster.WanderRadius, out Vector3 wanderPos))
        {
            monster.StartMovement(wanderPos);
        }
    }

    public override void ExitState()
    {
        if (monster != null) monster.StopMovement();
    }

    public override void OnTakeDamage(DamageInfo info)
    {
        if (monster == null) return;
        monster.EvaluateNewAttacker(info.attacker);
        monster.ChangeState(MonsterState.Attack);
    }
}
