public class DoatDeadState : BossDeadStateBase
{
    BossDoat doat;

    public DoatDeadState(BossStateMachine stateMachine) : base(stateMachine)
    {
        if (stateMachine.boss is BossDoat)
        {
            doat = (stateMachine.boss as BossDoat);
        }
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void OnDead()
    {
        base.OnDead();
        
        doat.CancleCharge();
        doat.eyeAnimator.SetInteger(doat.eyeParam, 0);
    }
}
