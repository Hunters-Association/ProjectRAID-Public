using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossPracticeIdlePattern : ActionPattern
{
    public BossPracticeIdlePattern(Boss boss, BossBaseState state) : base(boss, state)
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
