using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaviesStateSelectState : SubState
{
    List<ActionPattern> currentAttackPatternList;   // 현재 페이즈의 공격 패턴

    public float wanderingPercents = 0.1f;
    public float[] phaseAttackPercents = new float[] { 0.5f };   // 페이즈 별 공격 퍼센트

    public LaviesStateSelectState(BossStateMachine stateMachine, MainState parent) : base(stateMachine, parent)
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
        if (parent is LaviesNoneCombatState)
        {
            SelectStateInNoneCombat();
            return;
        }
        else if (parent is LaviesCombatState)
        {
            SelectStateInCombat(currentPhaseIndex);
            return;
        }
    }
    private void SelectStateInNoneCombat()
    {
        LaviesNoneCombatState noneCombatState = parent as LaviesNoneCombatState;

        if (IsWanderingState())
        {
            parent.ChangeSubState(noneCombatState.wanderingState);
            return;
        }

        parent.ChangeSubState(noneCombatState.idleState);
        return;
    }
    private bool IsWanderingState()
    {
        float randomValue = Random.Range(0, 1f);

        return randomValue < wanderingPercents;
    }

    private void SelectStateInCombat(int currentPhaseIndex)
    {
        LaviesCombatState combatState = parent as LaviesCombatState;

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
                parent.ChangeSubState(new LaviesLookState(stateMachine, parent, currentPattern));
                return;
            }
            // 만약 공격 범위 밖이라면 추격 상태로 전환 후 공격
            else
            {
                parent.ChangeSubState(new LaviesChaseState(stateMachine, parent, currentPattern));
                return;
            }
        }

        // 아무런 상태도 돌입하지 못했다면 대기 상태로 전환
        parent.ChangeSubState(combatState.idleState);
        return;
    }

    // 공격 상태로 전환할 수 있는가?
    private bool IsChangeAttackState(int currentPhaseIndex)
    {
        float randomValue = Random.Range(0, 1f);

        return randomValue < phaseAttackPercents[currentPhaseIndex];
    }
}
