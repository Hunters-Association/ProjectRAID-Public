using ProjectRaid.EditorTools;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

// 선공 가능한 전기 두꺼비
public class BossDoat : Boss
{
    [FoldoutGroup("Animation", ExtendedColor.Blue)]
    public DoatAnimationHandler animationHandler;
    public Animator eyeAnimator;

    public readonly string eyeParam = "Eyes";

    //=================== [Charge] ===================
    [FoldoutGroup("충전 상태", ExtendedColor.Red)]
    public bool isChargeState = false;      // 충전 상태인가?
    [HideInInspector] public float lastChargeTime;    // 마지막으로 충전된 시간
    [HideInInspector] public float chargeCoolTime;    // 충전 쿨타임
    [HideInInspector] public float chargeHoldingTime; // 충전 유지 시간
    public GameObject chargeParticle;       // 충전 이펙트
    public ParticleSystem breathParticle;   // 브레스 이펙트

    //=================== [MainState] ===================
    public BossBaseState initState;
    public BossBaseState noneCombatState;
    public BossBaseState combatState;
    public BossBaseState destructState;
    public BossBaseState retreatState;
    public BossRestStateBase restState;
    public BossBaseState deadState;

    //=================== [Hitboxes] ===================


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

        // 충전 상태 초기화
        isChargeState = false;
        lastChargeTime = 0f;
        chargeParticle.SetActive(false);

        OpenEye();
    }

    private void SetState()
    {
        initState = new BossInitStateBase(stateMachine);
        destructState = new BossDestructionStateBase(stateMachine);
        restState = new BossRestStateBase(stateMachine);

        noneCombatState = new DoatNoneCombatState(stateMachine);
        combatState = new DoatCombatState(stateMachine);
        retreatState = new DoatRetreatState(stateMachine);
        deadState = new DoatDeadState(stateMachine);

        states.Add(BossMainState.Init, initState);
        states.Add(BossMainState.NoneCombat, noneCombatState);
        states.Add(BossMainState.Combat, combatState);
        states.Add(BossMainState.Destruct, destructState);
        states.Add(BossMainState.Retreat, retreatState);
        states.Add(BossMainState.Rest, restState);
        states.Add(BossMainState.Dead, deadState);

        // 휴식이 끝나면 눈이 떠지도록 설정
        restState.onRestEnter += CancleCharge;
        restState.onRestExit += OpenEye;
    }

    private void SetPhase()
    {
        BossPhaseData phase1 = new BossPhaseData()
        {
            phaseIndex = 0,
            phaseType = "phase1",
            changeHPPercent = 1.0f,
            setData = () => { chargeCoolTime = 60f; chargeHoldingTime = 30f; },
            attackPatternList = new List<ActionPattern>()
            {
                new DoatTailAttack(this, combatState) {weight = 0.25f},
                new DoatLeftLegAttack(this, combatState) {weight = 0.25f},
                new DoatRightLegAttack(this, combatState) {weight = 0.25f},
                new DoatFrontAllLegAttack(this, combatState) {weight = 0.25f},
                new DoatBodyPressAttack(this, combatState) {weight = 0.25f},
                new DoatShoot(this, combatState){weight = 0.25f},
                new DoatShootSwipe(this, combatState){weight = 0.25f},
                new DoatBiteAttack(this, combatState) {weight = 0.25f},
            }
        };

        BossPhaseData phase2 = new BossPhaseData()
        {
            phaseIndex = 1,
            phaseType = "phase2",
            changeHPPercent = 0.7f,
            setData = () => { chargeCoolTime = 60f; chargeHoldingTime = 30f; },
            attackPatternList = new List<ActionPattern>()
            {
                new DoatTailAttack(this, combatState) {weight = 0.25f},
                new DoatLeftLegAttack(this, combatState) {weight = 0.25f},
                new DoatRightLegAttack(this, combatState) {weight = 0.25f},
                new DoatFrontAllLegAttack(this, combatState) {weight = 0.25f},
                new DoatBodyPressAttack(this, combatState) {weight = 0.25f},
                new DoatShoot(this, combatState){weight = 0.25f},
                new DoatShootSwipe(this, combatState){weight = 0.25f},
                new DoatBiteAttack(this, combatState) {weight = 0.25f},
            }
        };

        BossPhaseData phase3 = new BossPhaseData()
        {
            phaseIndex = 2,
            phaseType = "phase3",
            changeHPPercent = 0.3f,
            setData = () => { chargeCoolTime = -1f; chargeHoldingTime = -1f; },
            attackPatternList = new List<ActionPattern>()
            {
                new DoatTailAttack(this, combatState) {weight = 0.25f},
                new DoatLeftLegAttack(this, combatState) {weight = 0.25f},
                new DoatRightLegAttack(this, combatState) {weight = 0.25f},
                new DoatFrontAllLegAttack(this, combatState) {weight = 0.25f},
                new DoatBodyPressAttack(this, combatState) {weight = 0.25f},
                new DoatShoot(this, combatState){weight = 0.25f},
                new DoatShootSwipe(this, combatState){weight = 0.25f},
                new DoatBiteAttack(this, combatState) {weight = 0.25f},
            }
        };

        phase1.setData?.Invoke();
        phase2.setData?.Invoke();
        phase3.setData?.Invoke();

        phases.Add(phase1);
        phases.Add(phase2);
        phases.Add(phase3);
    }

    protected override void Update()
    {
        base.Update();

        UpdateChargeState();
    }

    public override void OnDestructionEffects()
    {
        base.OnDestructionEffects();
        animationHandler.OnDestructionSFX();
    }

    public override void OffHitEffects()
    {
        base.OffHitEffects();

        animationHandler.OffSfxSound();
        animationHandler.OffBreathParticleEnd();
    }

    public override void InitPartColliders()
    {
        base.InitPartColliders();

        //for (int i = 0; i < bossBodyParts.Length; i++)
        //{
        //    if (bossBodyParts[i] is BossBodyDestructionPart)
        //    {
        //        bossBodyParts[i].onDestPart += animationHandler.OnDestructionSFX;
        //    }
        //    else if (bossBodyParts[i] is BossBodyDestructionListPart)
        //    {
        //        bossBodyParts[i].onDestPart += animationHandler.OnDestructionSFX;
        //    }
        //    else if (bossBodyParts[i] is BossBodyCuttingPart)
        //    {
        //        bossBodyParts[i].onDestPart += animationHandler.OnDestructionSFX;
        //    }
        //    else if (bossBodyParts[i] is BossBodyCuttingListParts)
        //    {
        //        bossBodyParts[i].onDestPart += animationHandler.OnDestructionSFX;
        //    }
        //}
    }

    private void UpdateChargeState()
    {
        if (!isChargeState) return;

        // 유지 시간이 지났을 경우 충전 상태를 다시 돌려놓는다.
        // 유지 시간이 -1이면 무한대
        if (Time.time - lastChargeTime > chargeCoolTime && chargeHoldingTime != -1f)
        {
            CancleCharge();
        }
    }

    public void ActiveCharge()
    {
        isChargeState = true;
        lastChargeTime = Time.time;
        chargeParticle.SetActive(true);
    }

    public void CancleParticle()
    {
        if(breathParticle.isPlaying)
            breathParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    public void CancleCharge()
    {
        isChargeState = false;
        chargeParticle.SetActive(false);
    }

    public void OpenEye()
    {
        eyeAnimator.SetInteger(eyeParam, 1);
    }
    public void CloseEye()
    {
        eyeAnimator.SetInteger(eyeParam, 0);
    }
}
