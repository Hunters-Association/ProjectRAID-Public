using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class BearGomCombatState : BossCombatStateBase
{
    public BearGomStateSelectState stateSelect;
    public BearGomIdleState idleState;
    public BearGomStunState stunState;

    public BearGomCombatState(BossStateMachine stateMachine) : base(stateMachine)
    {
        stateSelect = new BearGomStateSelectState(stateMachine, this);
        idleState = new BearGomIdleState(stateMachine, this);
        roarState = new BearGomRoarState(stateMachine, this);
        stunState = new BearGomStunState(stateMachine, this);
    }
}
