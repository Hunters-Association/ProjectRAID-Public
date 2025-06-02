using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class BossRetreatStateBase : MainState
{
    public SubState roarState;
    public SubState returnNest;

    public BossRetreatStateBase(BossStateMachine stateMachine) : base(stateMachine)
    {
    }


    public override void Enter()
    {
        base.Enter();

        ChangeSubState(roarState);
    }

    public override void Update()
    {
        base.Update();
    }

    public override void Exit()
    {
        base.Exit();
    }
}
