public class LaviesRoarState : SubState
{
    public LaviesRoarState(BossStateMachine stateMachine, MainState parent) : base(stateMachine, parent)
    {
    }

    public override void Enter()
    {
        base.Enter();

        StopNavAgent();

        // 포효 애니메이션 실행
        StartAnimation("Roar");
    }

    public override void Update()
    {
        base.Update();

        if (IsFinishAnimation("Roar"))
        {
            if (parent is LaviesCombatState)
            {
                LaviesCombatState combatState = parent as LaviesCombatState;

                parent.ChangeSubState(combatState.stateSelect);
            }
            else if (parent is LaviesRetreatState)
            {
                // 후퇴 상태에서 호출된 포효라면 둥지로 돌아가는 상태로 전환
                LaviesRetreatState retreatState = parent as LaviesRetreatState;
                parent.ChangeSubState(retreatState.returnNest);
            }
        }
    }

    public override void Exit()
    {
        base.Exit();

        if (stateMachine.previousState != null && stateMachine.previousState is BossRestStateBase)
        {
            stateMachine.boss.SubscribeDestructionPartsEvent();
            stateMachine.previousState = null;
        }

        StopAnimation("Roar");
    }
}
