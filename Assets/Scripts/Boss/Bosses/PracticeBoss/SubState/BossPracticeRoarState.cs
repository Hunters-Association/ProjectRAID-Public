using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossPracticeRoarState : SubState
{
    public BossPracticeRoarState(BossStateMachine stateMachine, MainState parent) : base(stateMachine, parent)
    {
    }

    public override void Enter()
    {
        base.Enter();

        StopNavAgent();

        // 포효 애니메이션 실행
        StartAnimation("Roar");
    }

    public override void Update()
    {
        base.Update();

        if (IsFinishAnimation("Roar"))
        {
            if (parent is BossPracticeCombatState)
            {
                BossPracticeCombatState combatState = parent as BossPracticeCombatState;

                parent.ChangeSubState(combatState.stateSelect);
            }
        }
    }

    public override void Exit()
    {
        base.Exit();

        StopAnimation("Roar");
    }
}
