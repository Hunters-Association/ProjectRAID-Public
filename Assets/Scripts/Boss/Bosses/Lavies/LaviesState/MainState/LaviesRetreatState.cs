using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class LaviesRetreatState : BossRetreatStateBase
{
    public LaviesRetreatState(BossStateMachine stateMachine) : base(stateMachine)
    {
        roarState = new LaviesRoarState(stateMachine, this);
        returnNest = new LaviesReturnNestState(stateMachine, this);
    }
}
