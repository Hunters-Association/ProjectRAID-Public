using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BearGomRoarState : SubState
{
    public BearGomRoarState(BossStateMachine stateMachine, MainState parent) : base(stateMachine, parent)
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
            if (parent is BearGomCombatState)
            {
                BearGomCombatState combatState = parent as BearGomCombatState;

                parent.ChangeSubState(combatState.stateSelect);
            }
            else if (parent is BearGomRetreatState)
            {
                // 후퇴 상태에서 호출된 포효라면 둥지로 돌아가는 상태로 전환
                BearGomRetreatState retreatState = parent as BearGomRetreatState;
                parent.ChangeSubState(retreatState.returnNest);
            }
        }
    }

    public override void Exit()
    {
        base.Exit();
        StopAnimation("Roar");
    }
}
