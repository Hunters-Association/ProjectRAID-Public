using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BearGomStunState : SubState
{
    public BearGomStunState(BossStateMachine stateMachine, MainState parent) : base(stateMachine, parent)
    {
    }
    public override void Enter()
    {
        base.Enter();
        StartAnimation("Stun");
    }

    public override void Exit()
    {
        base.Exit();
        StopAnimation("Stun");
    }

    public override void Update()
    {
        base.Update();

        if (IsFinishAnimation("Stun"))
        {
            BearGomCombatState combatState = parent as BearGomCombatState;
            parent.ChangeSubState(combatState.stateSelect);
            return;
        }
    }
}
