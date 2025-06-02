
using System.Collections.Generic;
using UnityEngine;

public class LaviesIdleState : SubState
{
    public List<ActionPattern> patternList;

    public LaviesIdleState(BossStateMachine stateMachine, MainState parent) : base(stateMachine, parent)
    {
        if (parent is LaviesNoneCombatState)
        {
            patternList = new List<ActionPattern>()
            {
                new LaviesBreath(stateMachine.boss, this) {weight = 0.45f},
                new LaviesLookAround(stateMachine.boss, this) {weight = 0.45f},
            };
        }
        else
        {
            patternList = new List<ActionPattern>()
            {
                new LaviesBreath(stateMachine.boss, this) {weight = 0.4f},
                new LaviesLookAround(stateMachine.boss, this) {weight = 0.4f},
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
            if (parent is LaviesNoneCombatState)
            {
                LaviesNoneCombatState noneCombatState = parent as LaviesNoneCombatState;

                parent.ChangeSubState(noneCombatState.stateSelect);
            }
            else if (parent is LaviesCombatState)
            {
                LaviesCombatState combatState = parent as LaviesCombatState;

                parent.ChangeSubState(combatState.stateSelect);
            }

            return;
        }
    }
}
