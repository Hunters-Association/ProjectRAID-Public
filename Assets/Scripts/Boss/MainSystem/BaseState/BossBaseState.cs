
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public interface IBossState
{
    public void Enter();
    public void Exit();
    public void Update();

}

public class BossBaseState : IBossState
{

    public BossStateMachine stateMachine;
    public NavMeshAgent navAgent;
    public ActionPattern currentPattern;

    public BossBaseState(BossStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
        navAgent = this.stateMachine.boss.navAgent;
    }

    public virtual void Enter()
    {
    }

    public virtual void Exit()
    {
    }
    public virtual void Update()
    {
    }

    #region 애니메이션
    public void StartAnimation(string animName)
    {
        stateMachine.boss.animator.SetBool(animName, true);
    }
    public void StopAnimation(string animName)
    {
        stateMachine.boss.animator.SetBool(animName, false);
    }
    #endregion

    #region NavMeshAgent
    public void StartNavAgent(float speed = 0)
    {
        if (speed == 0) speed = stateMachine.boss.bossData.bossSpeed;

        navAgent.speed = speed;
        navAgent.isStopped = false;
    }
    public void StopNavAgent()
    {
        navAgent.isStopped = true;
        navAgent.velocity = Vector3.zero;
    }
    #endregion

    #region Func

    // 추격이 가능한 지역에 있는지 확인
    protected bool CheckChaseArea()
    {
        Transform targetTr = stateMachine.boss.target;

        if (targetTr == null) return false;

        NavMeshHit hit;

        return NavMesh.SamplePosition(targetTr.position, out hit, 1.0f, NavMesh.AllAreas);
    }

    // 감지 거리 안에 있는지 확인
    protected bool CheckDetectDistance()
    {
        Transform targetTr = stateMachine.boss.target;
        Transform bossTr = stateMachine.boss.transform;

        float detectDistance = stateMachine.boss.detectDistance;

        if (targetTr == null) return false;

        // 만약 타겟이 감지 거리 안에 있다면 true 반환
        if ((targetTr.position - bossTr.position).sqrMagnitude < detectDistance * detectDistance)
        {
            return true;
        }

        return false;
    }

    // 보스와 플레이어 사이에 장애물이 있는지 판단
    protected bool CheckObstacle()
    {
        Transform targetTr = stateMachine.boss.target;
        Transform bossTr = stateMachine.boss.transform;

        if (targetTr == null) return false;

        Vector3 dir = targetTr.position - bossTr.position;

        Ray ray = new Ray(bossTr.position, dir);

        if (Physics.Raycast(ray, dir.magnitude, LayerMask.GetMask("Obstacle")))
        {
            return true;
        }

        return false;
    }

    protected bool CheckAttackDistance()
    {
        Transform targetTr = stateMachine.boss.target;
        Transform bossTr = stateMachine.boss.transform;

        float attackDistance = stateMachine.boss.attackDistacne;

        if (targetTr == null) return false;

        // 만약 타겟이 어택거리 안에 있다면 true 반환
        if ((targetTr.position - bossTr.position).sqrMagnitude < attackDistance * attackDistance)
        {
            return true;
        }

        return false;
    }

    public float GetAnimationNormalize(string tag)
    {
        AnimatorStateInfo currentInfo = stateMachine.boss.animator.GetCurrentAnimatorStateInfo(0);
        AnimatorStateInfo nextInfo = stateMachine.boss.animator.GetNextAnimatorStateInfo(0);

        // 전환되고 있을 때 && 다음 애니메이션이 tag라면
        if (stateMachine.boss.animator.IsInTransition(0) && nextInfo.IsTag(tag))
        {
            return nextInfo.normalizedTime;
        }
        // 전환되고 있지 않을 때 && 현재 애니메이션이 tag라면
        else if (!stateMachine.boss.animator.IsInTransition(0) && currentInfo.IsTag(tag))
        {
            return currentInfo.normalizedTime;
        }
        else
        {
            return 0;
        }
    }

    public bool IsFinishAnimation(string tag)
    {
        float normalizeTime = GetAnimationNormalize(tag);

        return (normalizeTime > 1f);
    }

    public bool IsFinishStunTime() => Time.time - stateMachine.boss.startStunTime >= stateMachine.boss.stunTime;

    // 배회할 위치 반환 함수
    public Vector3 SetWanderingPosition()
    {
        float range = 50f;

        float searchRadius = 3.0f;

        // 최대 시도 횟수
        int maxAttempCount = 50;

        NavMeshHit navMeshHit;
        RaycastHit rayHit;
        for (int i = 0; i < maxAttempCount; i++)
        {
            Vector3 randomPos = stateMachine.boss.transform.position + UnityEngine.Random.insideUnitSphere * range;

            // 아래 방향
            if (Physics.Raycast(randomPos, Vector3.down, out rayHit, range))
            {
                if (NavMesh.SamplePosition(rayHit.point, out navMeshHit, searchRadius, NavMesh.AllAreas))
                {
                    return navMeshHit.position;
                }
            }

            // 위 방향
            if (Physics.Raycast(randomPos, Vector3.up, out rayHit, range))
            {
                if (NavMesh.SamplePosition(rayHit.point, out navMeshHit, searchRadius, NavMesh.AllAreas))
                {
                    return navMeshHit.position;
                }
            }
        }

        return stateMachine.boss.transform.position;
    }

    public ActionPattern SetPattern(List<ActionPattern> patternList)
    {
        List<ActionPattern> currentPatterList = new List<ActionPattern>();

        for (int i = 0; i < patternList.Count; i++)
        {
            if (patternList[i].CanUse())
                currentPatterList.Add(patternList[i]);
        }

        float totalWeight = 0;

        for (int i = 0; i < currentPatterList.Count; i++)
        {
            totalWeight += currentPatterList[i].weight;
        }

        float randomValue = UnityEngine.Random.Range(0, totalWeight);
        float currentWeight = 0;
        for (int i = 0; i < currentPatterList.Count; i++)
        {
            currentWeight += patternList[i].weight;
            if (randomValue < currentWeight)
            {
                return currentPatterList[i];
            }
        }

        return null;
    }

    #endregion
}
