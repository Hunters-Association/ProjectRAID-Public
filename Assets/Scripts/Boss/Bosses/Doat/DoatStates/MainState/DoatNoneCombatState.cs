using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoatNoneCombatState : BossNoneCombatStateBase
{
    public DoatStateSelectState stateSelect;
    public DoatWanderingState wanderingState;

    public DoatNoneCombatState(BossStateMachine stateMachine) : base(stateMachine)
    {
        stateSelect = new DoatStateSelectState(stateMachine, this);
        idleState = new DoatIdleState(stateMachine, this);
        wanderingState = new DoatWanderingState(stateMachine, this);
    }
}
