using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class BossPracticeCombatState : BossCombatStateBase
{
    public BossPracticeStateSelectState stateSelect;
    public BossPracticeIdleState idleState;

    public BossPracticeCombatState(BossStateMachine stateMachine) : base(stateMachine)
    {
        roarState = new BossPracticeRoarState(stateMachine, this);
        lookState = new BossPracticeLookState(stateMachine, this);
        stateSelect = new BossPracticeStateSelectState(stateMachine, this);
        idleState = new BossPracticeIdleState(stateMachine, this);
    }
}
