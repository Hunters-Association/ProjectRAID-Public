using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaviesNoneCombatState : BossNoneCombatStateBase
{
    public LaviesStateSelectState stateSelect;
    public LaviesWanderingState wanderingState;

    public LaviesNoneCombatState(BossStateMachine stateMachine) : base(stateMachine)
    {
        stateSelect = new LaviesStateSelectState(stateMachine, this);
        idleState = new LaviesIdleState(stateMachine, this);
        wanderingState = new LaviesWanderingState(stateMachine, this);
    }
}
