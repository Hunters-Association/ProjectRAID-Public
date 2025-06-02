using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BearGomBodyPress : AttackPattern
{
    private float speed = 15f;

    public BearGomBodyPress(Boss boss, BossBaseState state) : base(boss, state)
    {
        attackType = AttackType.Melee;
        attackDistance = 10f;
    }
    public override void Execute()
    {
        base.Execute();

        Debug.Log("바디 프레스");
        boss.animator.SetInteger("AttackIndex", 1);

        boss.bodyCollider.enabled = false;

        float bodyPressDistance = 50f;

        state.StartNavAgent(speed);
        // 돌진 위치 지정
        Vector3 dir = boss.target.position - boss.transform.position;
        dir = new Vector3(dir.x, 0, dir.z).normalized;
        Vector3 position = boss.transform.position + (dir * bodyPressDistance);
        boss.navAgent.SetDestination(position);

        // 히트 콜라이더 켜주기
        if(boss is BossBearGom)
        {
            BossBearGom bearGom = boss as BossBearGom;
            bearGom.animationHandler.bodyPressAttack.gameObject.SetActive(true);
        }
    }
    public override bool IsFinish()
    {
        if(boss.navAgent.remainingDistance <= 0.5f)
        {
            // 히트 콜라이더 꺼주기
            if (boss is BossBearGom)
            {
                BossBearGom bearGom = boss as BossBearGom;
                bearGom.animationHandler.bodyPressAttack.gameObject.SetActive(false);
                state.StopNavAgent();
                boss.bodyCollider.enabled = true;
            }
            return true;
        }

        return false;
    }
    public override bool IsNextAttack(out AttackPattern attackPattern)
    {
        float randomValue = Random.Range(0, 1f);

        if (randomValue < 0.5f)
        {
            attackPattern = new BearGomBodyPress(boss, state);
            return true;
        }

        attackPattern = null;
        return false;
    }
}
