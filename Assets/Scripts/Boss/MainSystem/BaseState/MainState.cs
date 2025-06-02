using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class MainState : BossBaseState
{
    public IBossState currentSubState;
    public MainState(BossStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
        currentSubState?.Exit();
    }

    public override void Update()
    {
        base.Update();
        currentSubState?.Update();
    }

    public void ChangeSubState(IBossState subState)
    {
        currentSubState?.Exit();
        currentSubState = subState;
        currentSubState?.Enter();
    }
}
