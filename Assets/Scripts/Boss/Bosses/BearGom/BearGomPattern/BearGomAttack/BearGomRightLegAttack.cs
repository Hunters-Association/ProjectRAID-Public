using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BearGomRightLegAttack : AttackPattern
{
    public BearGomRightLegAttack(Boss boss, BossBaseState state) : base(boss, state)
    {
        attackType = AttackType.Melee;
        attackDistance = 3f;
    }

    public override void Execute()
    {
        base.Execute();

        boss.animator.SetInteger("AttackIndex", 4);
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
            attackPattern = new BearGomFrontLegCombo(boss, state);
            return true;
        }

        attackPattern = null;
        return false;
    }
}
