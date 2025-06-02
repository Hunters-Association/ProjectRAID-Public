using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaviesJumpAttack : AttackPattern
{
    public LaviesJumpAttack(Boss boss, BossBaseState state) : base(boss, state)
    {
        attackType = AttackType.Melee;
        attackDistance = 2.5f;
    }

    public override void Execute()
    {
        base.Execute();

        boss.animator.SetInteger("AttackIndex", 0);
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
