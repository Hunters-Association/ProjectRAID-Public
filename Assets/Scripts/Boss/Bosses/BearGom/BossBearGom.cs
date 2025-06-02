using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBearGom : Boss
{
    [HideInInspector] public BearGomAnimationHandler animationHandler;

    //=================== [Projectile] ===================
    public GameObject projectileManagerObj;
    [HideInInspector]public BearGomProjectileManager projectileManager;

    //=================== [MainState] ===================
    public BossBaseState initState;
    public BossBaseState noneCombatState;
    public BossBaseState combatState;
    public BossBaseState destructState;
    public BossBaseState retreatState;
    public BossBaseState restState;
    public BossBaseState deadState;

    public override void Init()
    {
        base.Init();

        animationHandler = GetComponentInChildren<BearGomAnimationHandler>();

        stateMachine = new BossStateMachine(this);

        SetState();

        SetPhase();

        stateMachine.states = states;
        stateMachine.SetEventHandler();

        if (projectileManagerObj != null)
        {
            GameObject managerObj = Instantiate(projectileManagerObj);

            projectileManager = managerObj.GetComponent<BearGomProjectileManager>();
            projectileManager.boss = this;
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    private void SetState()
    {
        initState = new BossInitStateBase(stateMachine);
        destructState = new BossDestructionStateBase(stateMachine);
        restState = new BossRestStateBase(stateMachine);
        deadState = new BossDeadStateBase(stateMachine);

        noneCombatState = new BearGomNoneCombatState(stateMachine);
        combatState = new BearGomCombatState(stateMachine);
        retreatState = new BearGomRetreatState(stateMachine);

        states.Add(BossMainState.Init, initState);
        states.Add(BossMainState.NoneCombat, noneCombatState);
        states.Add(BossMainState.Combat, combatState);
        states.Add(BossMainState.Destruct, destructState);
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
                new BearGomLavaBreath(this, combatState) {weight = 0.25f},
                new BearGomBodyPress(this, combatState) {weight = 0.25f},
                new BearGomBite(this, combatState) {weight = 0.25f},
                new BearGomRightLegAttack(this, combatState) {weight = 0.25f},
                new BearGomFrontLegAttack(this, combatState) {weight = 0.25f},
            }
        };

        BossPhaseData phase2 = new BossPhaseData()
        {
            phaseIndex = 1,
            phaseType = "phase2",
            changeHPPercent = 0.7f,
            attackPatternList = new List<ActionPattern>()
            {
                new BearGomLavaBreath(this, combatState) {weight = 0.25f},
                new BearGomBodyPress(this, combatState) {weight = 0.25f},
                new BearGomBite(this, combatState) {weight = 0.25f},
                new BearGomRightLegAttack(this, combatState) {weight = 0.25f},
                new BearGomFrontLegAttack(this, combatState) {weight = 0.25f},
            }
        };

        BossPhaseData phase3 = new BossPhaseData()
        {
            phaseIndex = 2,
            phaseType = "phase3",
            changeHPPercent = 0.3f,
            attackPatternList = new List<ActionPattern>()
            {
                new BearGomLavaBreath(this, combatState) {weight = 0.25f},
                new BearGomBodyPress(this, combatState) {weight = 0.25f},
                new BearGomBite(this, combatState) {weight = 0.25f},
                new BearGomRightLegAttack(this, combatState) {weight = 0.25f},
                new BearGomFrontLegAttack(this, combatState) {weight = 0.25f},
            }
        };

        phases.Add(phase1);
        phases.Add(phase2);
        phases.Add(phase3);
    }
}
