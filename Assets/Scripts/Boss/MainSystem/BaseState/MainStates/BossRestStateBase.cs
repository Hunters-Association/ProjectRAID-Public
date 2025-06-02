using UnityEngine;
using System;

public class BossRestStateBase : MainState
{
    private float inRestTime;   // 휴식에 돌입한 시간
    private float lastRecoveryTime; // 마지막으로 체력을 회복한 시간

    public Action onRestEnter;
    public Action onRestExit;

    public BossRestStateBase(BossStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();

        StopNavAgent();
        StartAnimation("Rest");

        inRestTime = Time.time;
        stateMachine.boss.bossHealth.OnHit += ChangeCombatState;
        stateMachine.boss.UnSubscribeDestructionPartsEvent();

        onRestEnter?.Invoke();
    }

    public override void Exit()
    {
        base.Exit();

        StopAnimation("Rest");
        stateMachine.boss.bossHealth.OnHit -= ChangeCombatState;

        onRestExit?.Invoke();
    }

    public override void Update()
    {
        base.Update();

        // 초당 1씩 체력 회복
        if (Time.time - lastRecoveryTime > stateMachine.boss.recoveryInterval)
        {
            lastRecoveryTime = Time.time;
            stateMachine.boss.bossHealth.hp += 1f;
        }


        // 휴식이 끝났다면 비전투 상태로 진입
        if (EndRestState())
        {
            stateMachine.ChangeState(stateMachine.states[BossMainState.NoneCombat]);
            return;
        }
    }

    // 휴식 상태에서 데미지를 입었을 시 전투 상태로 전환
    public void ChangeCombatState()
    {
        stateMachine.ChangeState(stateMachine.states[BossMainState.Combat]);
        return;
    }

    private bool EndRestState()
    {
        return (Time.time - inRestTime) > stateMachine.boss.restTime;
    }
}
