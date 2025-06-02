using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BearGomFrontLegAttack : AttackPattern
{
    public BearGomFrontLegAttack(Boss boss, BossBaseState state) : base(boss, state)
    {
        attackType = AttackType.Melee;
        attackDistance = 3f;
    }

    public override void Execute()
    {
        base.Execute();

        boss.animator.SetInteger("AttackIndex", 5);
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
