using System.Collections.Generic;

public class BossLavies : Boss
{
    public BossBaseState initState;
    public BossBaseState noneCombatState;
    public BossBaseState combatState;
    public BossBaseState desctructState;
    public BossBaseState retreatState;
    public BossBaseState restState;
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
        restState = new BossRestStateBase(stateMachine);
        deadState = new BossDeadStateBase(stateMachine);
        desctructState = new BossDestructionStateBase(stateMachine);

        noneCombatState = new LaviesNoneCombatState(stateMachine);
        combatState = new LaviesCombatState(stateMachine);
        retreatState = new LaviesRetreatState(stateMachine);

        states.Add(BossMainState.Init, initState);
        states.Add(BossMainState.NoneCombat, noneCombatState);
        states.Add(BossMainState.Combat, combatState);
        states.Add(BossMainState.Destruct, desctructState);
        states.Add(BossMainState.Retreat, retreatState);
        states.Add(BossMainState.Rest, restState);
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
                new LaviesJumpAttack(this, combatState) {weight = 1.0f}
            }
        };

        phases.Add(phase1);
    }
}
