using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossDeadStateBase : MainState
{
    private float deadTime = 0f;        // 몬스터가 죽은 시간
    private float enableTime;     // 몬스터가 유지되는 시간
    private bool isInteractableActive; // 상호 작용 가능한 오브젝트가 켜졌는가?(갈무리용)
    private GameEventInt monsterKilledEvent;
    private int bossID;

    public BossDeadStateBase(BossStateMachine stateMachine) : base(stateMachine)
    {
        if(stateMachine.boss.bossData != null)
        {
            monsterKilledEvent = stateMachine.boss.bossData.monsterKilledEvent;
            bossID = stateMachine.boss.bossData.bossID;
        }
        else
        {
            Debug.LogError($"Monster {stateMachine.boss.gameObject.name} is missing MonsterData! Cannot get ID or KilledEvent.");
            bossID = -1; // 오류 값
        }

        enableTime = stateMachine.boss.enableTime;
    }

    public override void Enter()
    {
        base.Enter();

        stateMachine.boss.OffPartColliders();

        StopNavAgent();

        stateMachine.boss.animator.SetTrigger("Dead");

        deadTime = Time.time;
        isInteractableActive = false;

        stateMachine.boss.bodyCollider.enabled = false;

        // 퀘스트 설정
        if(monsterKilledEvent != null && bossID != -1)
        {
            monsterKilledEvent.Raise(bossID);
        }
        else if(bossID != -1)
        {
            Debug.LogWarning($"BOSS {stateMachine.boss.gameObject.name} (ID: {bossID}) has no MonsterKilledEvent assigned in its MonsterData!");
        }
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if (IsFinishAnimation("Dead") && !isInteractableActive)
        {
            stateMachine.boss.OnInteractableObject();
            isInteractableActive = true;

            OnDead();
        }

        // 유지 되는 시간이 지났다면 보스 사라짐
        if (Time.time - deadTime > enableTime)
        {
            stateMachine.boss.gameObject.SetActive(false);
        }
    }

    // 죽었을 때 추가로 할 행동
    public virtual void OnDead() { }
}
