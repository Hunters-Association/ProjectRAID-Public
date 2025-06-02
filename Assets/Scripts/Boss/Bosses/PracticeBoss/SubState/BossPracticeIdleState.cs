using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossPracticeIdleState : SubState
{
    public List<ActionPattern> patternList;

    public BossPracticeIdleState(BossStateMachine stateMachine, MainState parent) : base(stateMachine, parent)
    {
        if (parent is BossPracticeNoneCombatState)
        {
            patternList = new List<ActionPattern>()
            {
                new BossPracticeIdlePattern(stateMachine.boss, this) {weight = 1.0f},
            };
        }
        else
        {
            patternList = new List<ActionPattern>()
            {
                new BossPracticeIdlePattern(stateMachine.boss, this) {weight = 1.0f},
            };
        }
    }

    public override void Enter()
    {
        base.Enter();

        currentPattern = SetPattern(patternList);

        StartAnimation("Idle");
        currentPattern?.Execute();

        StopNavAgent();
    }

    public override void Exit()
    {
        base.Exit();

        StopAnimation("Idle");
    }

    public override void Update()
    {
        base.Update();

        // Idle 애니메이션이 끝나면 부모 상태에 맞는 상태로 전환 시켜준다.
        if (IsFinishAnimation("Idle"))
        {
            if (parent is BossPracticeNoneCombatState)
            {
                BossPracticeNoneCombatState noneCombatState = parent as BossPracticeNoneCombatState;

                parent.ChangeSubState(noneCombatState.stateSelect);
            }
            else if (parent is BossPracticeCombatState)
            {
                BossPracticeCombatState combatState = parent as BossPracticeCombatState;

                parent.ChangeSubState(combatState.stateSelect);
            }

            return;
        }
    }
}
