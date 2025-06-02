using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoatStunState : SubState
{
    public DoatStunState(BossStateMachine stateMachine, MainState parent) : base(stateMachine, parent)
    {
    }
    public override void Enter()
    {
        base.Enter();
        StartAnimation("Stun");

        stateMachine.boss.startStunTime = Time.time;
    }

    public override void Exit()
    {
        base.Exit();
        StopAnimation("Stun");
    }

    public override void Update()
    {
        base.Update();

        if (IsFinishStunTime())
        {
            DoatCombatState doatCombatState = parent as DoatCombatState;
            parent.ChangeSubState(doatCombatState.stateSelect);
            return;
        }
    }
}
