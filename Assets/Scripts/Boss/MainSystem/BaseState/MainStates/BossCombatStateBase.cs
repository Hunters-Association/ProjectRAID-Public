using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class BossCombatStateBase : MainState
{
    public SubState lookState;
    public SubState roarState;
    public BossCombatStateBase(BossStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();

        ChangeSubState(roarState);
    }

    public override void Update()
    {
        base.Update();

        // 비전투 상태 전환 조건 확인
        if (CheckNoneCombatStateCondition())
            stateMachine.ChangeState(stateMachine.states[BossMainState.NoneCombat]);
    }

    private bool CheckNoneCombatStateCondition()
    {
        return !CheckChaseArea() || stateMachine.boss.targetStat.CurrentHealth <= 0;
    }

    public override void Exit()
    {
        base.Exit();
        stateMachine.boss.OffHitBoxes();
        stateMachine.boss.OffHitEffects();
    }
}
