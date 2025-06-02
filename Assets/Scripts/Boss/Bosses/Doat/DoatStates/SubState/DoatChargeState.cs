using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoatChargeState : SubState
{
    public DoatChargeState(BossStateMachine stateMachine, MainState parent) : base(stateMachine, parent)
    {
    }

    public override void Enter()
    {
        base.Enter();
        StartAnimation("Charge");
    }

    public override void Exit()
    {
        base.Exit();
        StopAnimation("Charge");
    }

    public override void Update()
    {
        base.Update();

        if(IsFinishAnimation("Charge"))
        {
            if(stateMachine.boss is BossDoat)
                (stateMachine.boss as BossDoat).ActiveCharge();

            // 충전 상태가 끝났다면 선택 상태로 전환
            DoatCombatState doatCombatState = parent as DoatCombatState;
            parent.ChangeSubState(doatCombatState.stateSelect);
            return;
        }
    }
}
