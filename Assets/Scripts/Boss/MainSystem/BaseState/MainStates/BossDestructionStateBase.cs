
public class BossDestructionStateBase : MainState
{
    public BossDestructionStateBase(BossStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();

        StopNavAgent();

        // 부위 파괴 애니메이션 실행
        StartAnimation("Destruct");
        stateMachine.boss.OnDestructionEffects();
    }

    public override void Update()
    {
        base.Update();

        if (IsFinishAnimation("Destruct"))
        {
            if (CheckChaseArea())
                stateMachine.ChangeState(stateMachine.states[BossMainState.Combat]);
            else
                stateMachine.ChangeState(stateMachine.states[BossMainState.NoneCombat]);
            return;
        }
    }

    public override void Exit()
    {
        base.Exit();
        StopAnimation("Destruct");
    }
}
