using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 행동 패턴
public abstract class ActionPattern
{
    protected Boss boss;
    protected BossBaseState state;

    public float weight;   // 행동 가중치

    public ActionPattern(Boss boss, BossBaseState state)
    {
        this.boss = boss;
        this.state = state;
    }

    public virtual bool CanUse() { return true; }

    public virtual void Execute() { }
}
