using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BearGomBreath : ActionPattern
{
    public BearGomBreath(Boss boss, BossBaseState state) : base(boss, state)
    {
    }

    public override void Execute()
    {
        base.Execute();

        boss.animator.SetInteger("IdleIndex", 0);
        if(state.GetAnimationNormalize("Idle") >= 1f)
        {
            boss.animator.Play("Breath", 0, 0f);
        }
    }
}

public class BearGomLookAround : ActionPattern
{
    public BearGomLookAround(Boss boss, BossBaseState state) : base(boss, state)
    {
    }

    public override void Execute()
    {
        base.Execute();

        boss.animator.SetInteger("IdleIndex", 1);
        boss.animator.Play("LookAround", 0, 0f);
    }
}
public class BearGomSmell : ActionPattern
{
    public BearGomSmell(Boss boss, BossBaseState state) : base(boss, state)
    {
    }

    public override void Execute()
    {
        base.Execute();

        boss.animator.SetInteger("IdleIndex", 1);
        if (state.GetAnimationNormalize("Idle") >= 1f)
        {
            boss.animator.Play("Smell", 0, 0f);
        }
    }
}

// 나무에 비비는 패턴
public class BearGomRubTree : ActionPattern
{
    public BearGomRubTree(Boss boss, BossBaseState state) : base(boss, state)
    {
    }

    public override void Execute()
    {
        base.Execute();

        boss.animator.SetInteger("IdleIndex", 3);
        boss.animator.Play("RubTree", 0, 0f);
    }
}

public class BearGomSit : ActionPattern
{
    public BearGomSit(Boss boss, BossBaseState state) : base(boss, state)
    {
    }

    public override void Execute()
    {
        base.Execute();

        boss.animator.SetInteger("IdleIndex", 4);
        boss.animator.Play("Sit", 0, 0f);
    }
}
