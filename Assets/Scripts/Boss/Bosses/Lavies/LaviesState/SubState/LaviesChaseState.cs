using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaviesChaseState : SubState
{
    private ActionPattern attackPattern;

    public LaviesChaseState(BossStateMachine stateMachine, MainState parent, ActionPattern pattern) : base(stateMachine, parent)
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

        navAgent.SetDestination(stateMachine.boss.target.position);

        if (CheckAttackDistance())
        {
            parent.ChangeSubState(new LaviesLookState(stateMachine, parent, attackPattern));
            return;
        }
    }
}
