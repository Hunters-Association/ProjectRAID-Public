using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DoatRetreatState : BossRetreatStateBase
{
    public DoatRetreatState(BossStateMachine stateMachine) : base(stateMachine)
    {
        roarState = new DoatRoarState(stateMachine, this);
        returnNest = new DoatReturnNestState(stateMachine, this);
    }
}
