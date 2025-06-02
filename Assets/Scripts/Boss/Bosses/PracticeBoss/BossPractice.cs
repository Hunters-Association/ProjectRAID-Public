using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossPractice : Boss
{
    public BossBaseState initState;
    public BossBaseState noneCombatState;
    public BossBaseState combatState;
    public BossBaseState deadState;

    public override void Init()
    {
        base.Init();

        stateMachine = new BossStateMachine(this);

        SetState();

        SetPhase();

        stateMachine.states = states;
        stateMachine.SetEventHandler();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        target = null;
    }

    private void SetState()
    {
        initState = new BossInitStateBase(stateMachine);
        deadState = new BossDeadStateBase(stateMachine);
        noneCombatState = new BossPracticeNoneCombatState(stateMachine);
        combatState = new BossPracticeCombatState(stateMachine);

        states.Add(BossMainState.Init, initState);
        states.Add(BossMainState.NoneCombat, noneCombatState);
        states.Add(BossMainState.Combat, combatState);
        states.Add(BossMainState.Dead, deadState);
    }

    // 페이즈 세팅
    private void SetPhase()
    {
        BossPhaseData phase1 = new BossPhaseData()
        {
            phaseIndex = 0,
            phaseType = "phase1",
            changeHPPercent = 1.0f,
            attackPatternList = new List<ActionPattern>()
            {
                new BossPracticeAttackPattern(this, combatState) {weight = 1.0f}
            }
        };

        phases.Add(phase1);
    }
}
