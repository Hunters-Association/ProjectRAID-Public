using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaviesBreath : ActionPattern
{
    public LaviesBreath(Boss boss, BossBaseState state) : base(boss, state)
    {
    }

    public override void Execute()
    {
        base.Execute();

        boss.animator.SetInteger("IdleIndex", 0);
        if (state.GetAnimationNormalize("Idle") >= 1f)
        {
            boss.animator.Play("Breath", 0, 0f);
        }
    }
}

public class LaviesLookAround : ActionPattern
{
    public LaviesLookAround(Boss boss, BossBaseState state) : base(boss, state)
    {
    }

    public override void Execute()
    {
        base.Execute();

        boss.animator.SetInteger("IdleIndex", 1);
        if (state.GetAnimationNormalize("Idle") >= 1f)
        {
            boss.animator.Play("LookAround", 0, 0f);
        }
    }
}
