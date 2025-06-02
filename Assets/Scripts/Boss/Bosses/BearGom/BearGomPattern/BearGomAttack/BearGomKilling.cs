using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 즉사 패턴
public class BearGomKilling : AttackPattern
{
    public BearGomKilling(Boss boss, BossBaseState state) : base(boss, state)
    {
        attackType = AttackType.Range;
        attackDistance = 15f;
    }
    public override void Execute()
    {
        base.Execute();

        // 즉사기 실행
        boss.StartCoroutine(Killing());
    }

    private IEnumerator Killing()
    {
        // 준비 애니메이션
        boss.animator.SetInteger("AttackIndex", 2);
        yield return new WaitUntil(() => state.IsFinishAnimation("KillingReady"));

        // 점프 애니메이션
        boss.animator.SetInteger("AttackIndex", 3);
        yield return new WaitUntil(() => state.IsFinishAnimation("KillingJump"));

        // 성공 실패 조건 확인
        // 성공 시 즉사 패턴 애니메이션
        boss.animator.SetInteger("AttackIndex", 11);
        yield return new WaitUntil(() => state.IsFinishAnimation("Killing"));

        // 실패 시 실패 애니메이션
        boss.animator.SetInteger("AttackIndex", 4);
        yield return new WaitUntil(() => state.IsFinishAnimation("KillingFail"));
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
