using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BearGomLavaBreath : AttackPattern
{
    public BearGomLavaBreath(Boss boss, BossBaseState state) : base(boss, state)
    {
        attackType = AttackType.Range;
        attackDistance = 15f;
    }

    public override void Execute()
    {
        base.Execute();

        Debug.Log("용암 브레스");
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
