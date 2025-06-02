using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BearGomRetreatState : BossRetreatStateBase
{
    public BearGomRetreatState(BossStateMachine stateMachine) : base(stateMachine)
    {
        roarState = new BearGomRoarState(stateMachine, this);
        returnNest = new BearGomReturnNestState(stateMachine, this);
    }
}
