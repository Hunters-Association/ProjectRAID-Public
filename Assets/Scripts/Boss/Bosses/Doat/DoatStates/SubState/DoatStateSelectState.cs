using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoatStateSelectState : SubState
{
    List<ActionPattern> currentAttackPatternList;   // 현재 페이즈의 공격 패턴
    
    public float wanderingPercents = 0.4f;
    public float[] dodgePercents = new float[3] { 0.3f, 0.3f, 0.3f };
    public float[] phaseChargePercents = new float[3] { 0.35f, 0.35f, 1f };      // 페이즈 별 충전 퍼센트
    public float[] phaseAttackPercents = new float[3] { 0.5f, 0.7f, 0.8f };   // 페이즈 별 공격 퍼센트
    public float[] phaseRoarPercents = new float[3] { 0.02f, 0.02f, 0f };     // 페이즈 별 포효 퍼센트

    public DoatStateSelectState(BossStateMachine stateMachine, MainState parent) : base(stateMachine, parent)
    {
    }

    public override void Enter()
    {
        base.Enter();

        // 상태 선택
        SelectState();
    }

    public void SelectState()
    {
        int currentPhaseIndex = stateMachine.boss.currentPhaseIndex;
        // 부모 상태가 비전투 상태라면
        if (parent is DoatNoneCombatState)
        {
            SelectStateInNoneCombat();
            return;
        }
        else if (parent is DoatCombatState)
        {
            SelectStateInCombat(currentPhaseIndex);
            return;
        }
    }

    private void SelectStateInNoneCombat()
    {
        DoatNoneCombatState noneCombatState = parent as DoatNoneCombatState;

        if (IsWanderingState())
        {
            parent.ChangeSubState(noneCombatState.wanderingState);
            return;
        }

        parent.ChangeSubState(noneCombatState.idleState);
        return;
    }

    private void SelectStateInCombat(int currentPhaseIndex)
    {
        DoatCombatState combatState = parent as DoatCombatState;

        // 휴식 상태로 전환을 해야되는가?
        if (stateMachine.boss.isChangeRetreatState)
        {
            stateMachine.boss.isChangeRetreatState = false;
            stateMachine.ChangeState(stateMachine.states[BossMainState.Retreat]);
            return;
        }

        // 페이즈가 전환이 되는 체력인지 확인?
        // 포효 상태전환
        if (stateMachine.boss.isChangePhase)
        {
            parent.ChangeSubState(combatState.roarState);
            stateMachine.boss.isChangePhase = false;
            return;
        }

        // 충전 상태인지 확인
        if (!IsChargeState())
        {
            // 페이즈 별로 충전 확률이 다름
            if (IsChangeChargeState(currentPhaseIndex))
            {
                parent.ChangeSubState(combatState.chargeState);
                return;
            }
        }

        // 공격 상태로 전환 확인
        if (IsChangeAttackState(currentPhaseIndex))
        {
            currentAttackPatternList = stateMachine.boss.phases[currentPhaseIndex].attackPatternList;

            // 공격 패턴을 뽑아냄
            currentPattern = SetPattern(currentAttackPatternList);

            // 공격 범위 설정
            if (currentPattern is AttackPattern)
                stateMachine.boss.attackDistacne = (currentPattern as AttackPattern).attackDistance;

            // 만약 공격 범위 안에 들어왔다면 look 상태 후 공격 상태로 돌입
            if (CheckAttackDistance())
            {
                parent.ChangeSubState(new DoatLookState(stateMachine, parent, currentPattern));
                return;
            }
            // 만약 공격 범위 밖이라면 추격 상태로 전환 후 공격
            else
            {
                parent.ChangeSubState(new DoatChaseState(stateMachine, parent, currentPattern));
                return;
            }
        }

        // 포효 상태 전환 확인
        if (IsChangeRoarState(currentPhaseIndex))
        {
            parent.ChangeSubState(combatState.roarState);
            return;
        }

        // 회피 상태 전환
        //if(IsChangeDodgeState(currentPhaseIndex))
        //{
        //    parent.ChangeSubState(combatState.dodgeState);
        //    return;
        //}

        // 아무런 상태도 돌입하지 못했다면 대기 상태로 전환
        parent.ChangeSubState(combatState.idleState);
        return;
    }

    private bool IsWanderingState()
    {
        float randomValue = Random.Range(0, 1f);

        return randomValue < wanderingPercents;
    }

    private bool IsChargeState()
    {
        if(stateMachine.boss is BossDoat)
            return (stateMachine.boss as BossDoat).isChargeState;

        return false;
    }

    // 충전 상태로 전환할 수 있는가?
    private bool IsChangeChargeState(int currentPhaseIndex)
    {
        float randomValue = Random.Range(0, 1f);

        return randomValue < phaseChargePercents[currentPhaseIndex];
    }

    // 공격 상태로 전환할 수 있는가?
    private bool IsChangeAttackState(int currentPhaseIndex)
    {
        float randomValue = Random.Range(0, 1f);

        return randomValue < phaseAttackPercents[currentPhaseIndex];
    }

    // 포효 상태로 전환할 수 있는가?
    private bool IsChangeRoarState(int currentPhaseIndex)
    {
        float randomValue = Random.Range(0, 1f);

        return randomValue < phaseRoarPercents[currentPhaseIndex];
    }
    private bool IsChangeDodgeState(int currentPhaseIndex)
    {
        float randomValue = Random.Range(0, 1f);

        return randomValue < dodgePercents[currentPhaseIndex];
    }
}
