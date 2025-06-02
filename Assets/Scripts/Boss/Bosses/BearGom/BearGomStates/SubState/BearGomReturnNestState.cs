using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BearGomReturnNestState : SubState
{
    public BearGomReturnNestState(BossStateMachine stateMachine, MainState parent) : base(stateMachine, parent)
    {
    }
    public override void Enter()
    {
        base.Enter();
        StartNavAgent(stateMachine.boss.GetRunSpeed());

        StartAnimation("Walk");

        navAgent.destination = stateMachine.boss.nest.position;
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

        if (navAgent.remainingDistance < 0.1f)
        {
            // 휴식 상태로 전환
            stateMachine.ChangeState(stateMachine.states[BossMainState.Rest]);
            return;
        }
    }
}
