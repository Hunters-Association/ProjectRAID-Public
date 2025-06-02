using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DoatCombatState : BossCombatStateBase
{
    BossDoat doat;

    public DoatStateSelectState stateSelect;
    public DoatIdleState idleState;
    public DoatChargeState chargeState;
    public DoatStunState stunState;
    public DoatDodgeState dodgeState;

    public DoatCombatState(BossStateMachine stateMachine) : base(stateMachine)
    {
        if (stateMachine.boss is BossDoat)
        {
            doat = (stateMachine.boss as BossDoat);
        }

        roarState = new DoatRoarState(stateMachine, this);
        lookState = new DoatLookState(stateMachine, this);
        stateSelect = new DoatStateSelectState(stateMachine, this);
        idleState = new DoatIdleState(stateMachine, this);
        chargeState = new DoatChargeState(stateMachine, this);
        stunState = new DoatStunState(stateMachine, this);
        dodgeState = new DoatDodgeState(stateMachine, this);
    }

    public override void Exit()
    {
        base.Exit();

        doat.CancleParticle();
    }
}
