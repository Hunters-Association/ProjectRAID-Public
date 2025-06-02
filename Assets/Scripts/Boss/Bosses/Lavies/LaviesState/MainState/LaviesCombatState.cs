using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaviesCombatState : BossCombatStateBase
{
    public LaviesStateSelectState stateSelect;
    public LaviesIdleState idleState;

    public LaviesCombatState(BossStateMachine stateMachine) : base(stateMachine)
    {
        roarState = new LaviesRoarState(stateMachine, this);
        lookState = new LaviesLookState(stateMachine, this);
        stateSelect = new LaviesStateSelectState(stateMachine, this);
        idleState = new LaviesIdleState(stateMachine, this);
    }
}
