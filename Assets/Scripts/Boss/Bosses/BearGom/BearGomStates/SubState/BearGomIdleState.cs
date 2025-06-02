using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BearGomIdleState : SubState
{
    public List<ActionPattern> patternList;

    public BearGomIdleState(BossStateMachine stateMachine, MainState parent) : base(stateMachine, parent)
    {
        if (parent is BearGomCombatState)
        {
            patternList = new List<ActionPattern>()
            {
                new BearGomBreath(stateMachine.boss, this) {weight = 0.45f},
            };
        }
        else
        {
            patternList = new List<ActionPattern>()
            {
                new BearGomBreath(stateMachine.boss, this) {weight = 0.45f},
                new BearGomSmell(stateMachine.boss, this) {weight = 0.45f},
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
            if (parent is BearGomNoneCombatState)
            {
                BearGomNoneCombatState noneCombatState = parent as BearGomNoneCombatState;

                parent.ChangeSubState(noneCombatState.stateSelect);
            }
            else if (parent is BearGomCombatState)
            {
                BearGomCombatState combatState = parent as BearGomCombatState;

                parent.ChangeSubState(combatState.stateSelect);
            }

            return;
        }
    }
}
