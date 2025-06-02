using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DoatReturnNestState : SubState
{
    public DoatReturnNestState(BossStateMachine stateMachine, MainState parent) : base(stateMachine,parent)
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
            BossDoat doat = (stateMachine.boss as BossDoat);
            doat?.eyeAnimator.SetInteger(doat?.eyeParam, 0);
            stateMachine.ChangeState(stateMachine.states[BossMainState.Rest]);
            return;
        }
    }
}
