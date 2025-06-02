using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoatBiteAttack : AttackPattern
{
    public DoatBiteAttack(Boss boss, BossBaseState state) : base(boss, state)
    {
        attackType = AttackType.Melee;
        attackDistance = 3f;
    }

    public override void Execute()
    {
        base.Execute();

        boss.animator.SetInteger("AttackIndex", 7);
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
