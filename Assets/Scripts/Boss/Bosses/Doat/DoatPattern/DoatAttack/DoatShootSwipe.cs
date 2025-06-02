using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoatShootSwipe : AttackPattern
{
    public DoatShootSwipe(Boss boss, BossBaseState state) : base(boss, state)
    {
        attackType = AttackType.Range;
        attackDistance = 15f;
    }

    public override void Execute()
    {
        base.Execute();

        boss.animator.SetInteger("AttackIndex", 6);
    }
    public override bool CanUse()
    {
        if (boss is BossDoat)
        {
            BossDoat doat = boss as BossDoat;

            return doat.isChargeState == true;
        }

        return false;
    }

    public override bool IsFinish()
    {
        if (state.IsFinishAnimation("Attack"))
        {
            return true;
        }
        else
            return false;
    }
}
