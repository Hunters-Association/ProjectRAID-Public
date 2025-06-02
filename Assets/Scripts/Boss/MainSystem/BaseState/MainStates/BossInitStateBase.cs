using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class BossInitStateBase : MainState
{
    public BossInitStateBase(BossStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();

        StartAnimation("Init");
        stateMachine.boss.OffPartColliders();
    }

    public override void Exit()
    {
        base.Exit();

        StopAnimation("Init");
        stateMachine.boss.OnPartColliders();
    }

    public override void Update()
    {
        base.Update();

        if (IsFinishAnimation("Init"))
        {
            stateMachine.ChangeState(stateMachine.states[BossMainState.NoneCombat]);
            return;
        }
    }
}
