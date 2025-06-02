using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoatIdle : ActionPattern
{
    public DoatIdle(Boss boss, BossBaseState state) : base(boss, state)
    {
    }

    public override void Execute()
    {
        base.Execute();

        boss.animator.SetInteger("IdleIndex", 0);
        if (state.GetAnimationNormalize("Idle") >= 1f)
        {
            boss.animator.Play("Idle", 0, 0f);
        }
    }
}
public class DoatSmell : ActionPattern
{
    public DoatSmell(Boss boss, BossBaseState state) : base(boss, state)
    {
    }

    public override void Execute()
    {
        base.Execute();

        boss.animator.SetInteger("IdleIndex", 1);
        if (state.GetAnimationNormalize("Idle") >= 1f)
        {
            boss.animator.Play("Smell", 0, 0f);
        }
    }
}
public class DoatScratch : ActionPattern
{
    public DoatScratch(Boss boss, BossBaseState state) : base(boss, state)
    {
    }

    public override void Execute()
    {
        base.Execute();

        boss.animator.SetInteger("IdleIndex", 2);
        if (state.GetAnimationNormalize("Idle") >= 1f)
        {
            boss.animator.Play("Scratch", 0, 0f);
        }
    }
}
public class DoatLookLeft : ActionPattern
{
    public DoatLookLeft(Boss boss, BossBaseState state) : base(boss, state)
    {
    }
    public override void Execute()
    {
        base.Execute();

        boss.animator.SetInteger("IdleIndex", 3);
        if (state.GetAnimationNormalize("Idle") >= 1f)
        {
            boss.animator.Play("LookLeft", 0, 0f);
        }
    }
}
public class DoatLookRight : ActionPattern
{
    public DoatLookRight(Boss boss, BossBaseState state) : base(boss, state)
    {
    }
    public override void Execute()
    {
        base.Execute();

        boss.animator.SetInteger("IdleIndex", 4);
        if (state.GetAnimationNormalize("Idle") >= 1f)
        {
            boss.animator.Play("LookRight", 0, 0f);
        }
    }
}
