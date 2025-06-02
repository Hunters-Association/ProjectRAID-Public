using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

public class BossPracticeNoneCombatState : BossNoneCombatStateBase
{
    public BossPracticeStateSelectState stateSelect;

    public BossPracticeNoneCombatState(BossStateMachine stateMachine) : base(stateMachine)
    {
        stateSelect = new BossPracticeStateSelectState(stateMachine, this);
        idleState = new BossPracticeIdleState(stateMachine, this);
    }
}
