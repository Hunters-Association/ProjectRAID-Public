using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SubState : BossBaseState
{
    public MainState parent;

    public SubState(BossStateMachine stateMachine, MainState parent) : base(stateMachine)
    {
        this.parent = parent;
    }

    public override void Enter()
    {
        base.Enter();

        //Debug.Log(this.ToString() + "상태");
    }

    public override void Exit()
    {
        base.Exit();
    }
}

public class DoatSubState : BossBaseState
{
    public MainState parent;
    public BossDoat bossDoat;
    public DoatSubState(BossStateMachine stateMachine, MainState parent) : base(stateMachine)
    {
        this.parent = parent;
        bossDoat = stateMachine.boss as BossDoat;
    }

    public override void Enter()
    {
        base.Enter();

        //Debug.Log(this.ToString() + "상태 진입!");
    }

    public override void Exit()
    {
        base.Exit();

        //Debug.Log(this.ToString() + "상태 종료!");
    }
}