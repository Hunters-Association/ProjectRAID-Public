using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoatChaseState : SubState
{
    private ActionPattern attackPattern;

    public DoatChaseState(BossStateMachine stateMachine, MainState parent, ActionPattern pattern) : base(stateMachine, parent)
    {
        this.attackPattern = pattern;
    }

    public override void Enter()
    {
        base.Enter();
        StartAnimation("Walk");

        StartNavAgent(stateMachine.boss.GetRunSpeed());
    }

    public override void Exit()
    {
        base.Exit();
        StopAnimation("Walk");
        StopNavAgent();
    }

    public override void Update()
    {
        base.Update();

        navAgent.destination = stateMachine.boss.target.position;

        if (CheckAttackDistance())
        {
            parent.ChangeSubState(new DoatLookState(stateMachine, parent, attackPattern));
            return;
        }
    }
}
