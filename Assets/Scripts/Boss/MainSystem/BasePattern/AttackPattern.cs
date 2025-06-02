using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AttackType
{
    Melee,      // 근접
    Range,      // 원거리
    Count
}

public class AttackPattern : ActionPattern
{
    public AttackType attackType;

    public float attackDistance;

    public AttackPattern(Boss boss, BossBaseState state) : base(boss, state)
    {
    }

    public virtual bool IsFinish()
    {
        return false;
    }

    // 다음 연속 공격이 있니?
    public virtual bool IsNextAttack(out AttackPattern attackPattern)
    {
        attackPattern = null;
        return false;
    }
}
