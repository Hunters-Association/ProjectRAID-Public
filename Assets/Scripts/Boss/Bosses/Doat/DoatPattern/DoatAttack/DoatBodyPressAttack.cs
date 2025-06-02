using UnityEngine;

public class DoatBodyPressAttack : AttackPattern
{
    private float speed = 15f;

    public BossHitbox bodyPressHitbox;
    public BossDoat doat;

    public DoatBodyPressAttack(Boss boss, BossBaseState state) : base(boss, state)
    {
        attackType = AttackType.Melee;
        attackDistance = 10f;

        if (boss is BossDoat)
        {
            doat = boss as BossDoat;
            bodyPressHitbox = doat.animationHandler.bodyPressAttack;
        }
    }

    public override void Execute()
    {
        base.Execute();

        doat.animationHandler.OnBodyPressAttack();

        boss.animator.SetInteger("AttackIndex", 4);

        float bodyPressDistance = 40f;

        // 돌진 위치 지정
        Vector3 dir = boss.target.position - boss.transform.position;
        dir = new Vector3(dir.x, 0, dir.z).normalized;
        Vector3 position = boss.transform.position + (dir * bodyPressDistance);

        // 바디 콜라이더 꺼주기
        boss.bodyCollider.enabled = false;

        // 네비메쉬 목표 지정 설정해주기
        state.StartNavAgent(speed);
        boss.navAgent.SetDestination(position);

        // 히트 콜라이더 켜주기
        bodyPressHitbox.gameObject.SetActive(true);
    }
    public override bool IsFinish()
    {
        if (boss.navAgent.remainingDistance <= 0.5f)
        {
            // 히트 콜라이더 꺼주기
            bodyPressHitbox.gameObject.SetActive(false);
            state.StopNavAgent();
            boss.bodyCollider.enabled = true;

            return true;
        }

        return false;
    }

    public override bool IsNextAttack(out AttackPattern attackPattern)
    {
        float randomValue = Random.Range(0, 1f);

        if (randomValue < 0.25f)
        {
            attackPattern = new DoatBodyPressAttack(boss, state);
            return true;
        }

        attackPattern = null;
        return false;
    }
}
