using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoatRoarState : SubState
{
    public DoatRoarState(BossStateMachine stateMachine, MainState parent) : base(stateMachine, parent)
    {
    }

    public override void Enter()
    {
        base.Enter();

        StopNavAgent();

        // 포효 애니메이션 실행
        StartAnimation("Roar");
        Debug.Log("포효!");
    }

    public override void Update()
    {
        base.Update();

        if (IsFinishAnimation("Roar"))
        {
            if(parent is DoatCombatState)
            {
                DoatCombatState doatCombatState = parent as DoatCombatState;

                parent.ChangeSubState(doatCombatState.stateSelect);
            }
            else if(parent is DoatRetreatState)
            {
                // 후퇴 상태에서 호출된 포효라면 둥지로 돌아가는 상태로 전환
                DoatRetreatState doatRetreatState = parent as DoatRetreatState;
                parent.ChangeSubState(doatRetreatState.returnNest);
            }
        }
    }

    public override void Exit()
    {
        base.Exit();

        if (stateMachine.previousState != null && stateMachine.previousState is BossRestStateBase)
        {
            stateMachine.boss.SubscribeDestructionPartsEvent();
            stateMachine.previousState = null;
        }

        StopAnimation("Roar");
    }
}
