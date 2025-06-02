using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BearGomNoneCombatState : BossNoneCombatStateBase
{
    public BearGomStateSelectState stateSelect;
    public BearGomWanderingState wanderingState;

    public BearGomNoneCombatState(BossStateMachine stateMachine) : base(stateMachine)
    {
        stateSelect = new BearGomStateSelectState(stateMachine, this);
        idleState = new BearGomIdleState(stateMachine, this);
        wanderingState = new BearGomWanderingState(stateMachine, this);
    }
}
