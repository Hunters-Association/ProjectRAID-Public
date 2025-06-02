using System.Collections.Generic;

public class DoatIdleState : SubState
{
    public List<ActionPattern> patternList;

    BossDoat doat;

    public DoatIdleState(BossStateMachine stateMachine, MainState parent) : base(stateMachine, parent)
    {
        if(parent is DoatNoneCombatState)
        {
            patternList = new List<ActionPattern>()
            {
                new DoatIdle(stateMachine.boss, this) { weight = 0.2f },
                new DoatSmell(stateMachine.boss, this) { weight = 0.2f },
                new DoatScratch(stateMachine.boss, this) { weight = 0.2f },
                new DoatLookLeft(stateMachine.boss, this) { weight = 0.2f },
                new DoatLookRight(stateMachine.boss, this) { weight = 0.2f },
            };
        }
        else
        {
            patternList = new List<ActionPattern>()
            {
                new DoatIdle(stateMachine.boss, this) { weight = 0.2f },
            };
        }

        doat = stateMachine.boss as BossDoat;
    }

    public override void Enter()
    {
        base.Enter();

        currentPattern = SetPattern(patternList);

        StartAnimation("Idle");
        currentPattern?.Execute();

        doat.animationHandler.OnGrowlSFX();

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
            if(parent is DoatNoneCombatState)
            {
                DoatNoneCombatState noneCombatState = parent as DoatNoneCombatState;

                parent.ChangeSubState(noneCombatState.stateSelect);
            }
            else if(parent is DoatCombatState)
            {
                DoatCombatState combatState = parent as DoatCombatState;

                parent.ChangeSubState(combatState.stateSelect);
            }

            return;
        }
    }
}
