using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoatWanderingState : SubState
{
    public DoatWanderingState(BossStateMachine stateMachine, MainState parent) : base(stateMachine, parent)
    {
    }

    public override void Enter()
    {
        base.Enter();

        navAgent.destination = SetWanderingPosition();

        StartNavAgent(stateMachine.boss.GetWalkSpeed());
        StartAnimation("Walk");
    }

    public override void Exit()
    {
        base.Exit();
        StopNavAgent();
        StopAnimation("Walk");
    }

    public override void Update()
    {
        base.Update();

        if (navAgent.remainingDistance < 1f)
        {
            DoatNoneCombatState doatNoneCombatState = parent as DoatNoneCombatState;
            parent.ChangeSubState(doatNoneCombatState.stateSelect);
            return;
        }
    }
}
