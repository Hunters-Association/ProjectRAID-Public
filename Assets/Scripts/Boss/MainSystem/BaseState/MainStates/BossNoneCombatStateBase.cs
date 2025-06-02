using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class BossNoneCombatStateBase : MainState
{
    public SubState idleState;

    public BossNoneCombatStateBase(BossStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();
        ChangeSubState(idleState);
    }

    public override void Update()
    {
        base.Update();

        // 전투 상태 전환 조건 확인
        if (CheckCombatStateCondition())
        {
            stateMachine.ChangeState(stateMachine.states[BossMainState.Combat]);
        }
    }

    private bool CheckCombatStateCondition()
    {
        return CheckChaseArea()
            && CheckDetectDistance() 
            && !CheckObstacle() 
            && stateMachine.boss.targetStat.CurrentHealth > 0;
    }

    public override void Exit()
    {
        base.Exit();
    }
}
