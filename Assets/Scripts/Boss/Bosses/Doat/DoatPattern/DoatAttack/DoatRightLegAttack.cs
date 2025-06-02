using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoatRightLegAttack : AttackPattern
{
    public DoatRightLegAttack(Boss boss, BossBaseState state) : base(boss, state)
    {
        attackType = AttackType.Melee;
        attackDistance = 3f;
    }

    public override void Execute()
    {
        base.Execute();

        boss.animator.SetInteger("AttackIndex", 2);
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

    public override bool IsNextAttack(out AttackPattern attackPattern)
    {
        float randomValue = Random.Range(0, 1f);

        if (randomValue < 0.5f)
        {
            attackPattern = new DoatFrontAllLegAttack(boss, state);
            return true;
        }

        attackPattern = null;
        return false;
    }
}
