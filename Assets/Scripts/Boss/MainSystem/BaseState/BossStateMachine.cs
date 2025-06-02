using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossStateMachine
{
    public Boss boss;

    public IBossState previousState;
    public IBossState currentState;

    public Dictionary<BossMainState, BossBaseState> states;

    public BossStateMachine(Boss boss)
    {
        this.boss = boss;
    }

    public void ChangeState(IBossState state)
    {
        Debug.Log($"{currentState?.ToString()} -> {state?.ToString()}");
        currentState?.Exit();
        previousState = currentState;
        currentState = state;
        boss.currentState = (BossBaseState)state;
        currentState?.Enter();
    }

    public void Update()
    {
        currentState?.Update();
    }

    public void SetEventHandler()
    {
        boss.bossHealth.OnFirstHit -= OnFirstHit;
        boss.bossHealth.OnDead -= OnDead;
        boss.bossHealth.OnRetreat -= OnRetreat;

        if (boss.monsterType == MonsterBehaviorType.Passive)
            boss.bossHealth.OnFirstHit += OnFirstHit;

        boss.bossHealth.OnDead += OnDead;
        boss.bossHealth.OnRetreat += OnRetreat;
    }

    #region 이벤트 핸들러
    // 체력이 0이되어 죽었을 때
    public void OnDead()
    {
        ChangeState(states[BossMainState.Dead]);
    }

    // 도망칠 체력(위험한 상태)가 되었을 때
    public void OnRetreat()
    {
        boss.isChangeRetreatState = true;
    }

    public void OnFirstHit()
    {
        ChangeState(states[BossMainState.Combat]);
    }

    // 파괴가 되었을 때
    public void OnDestruct()
    {
        if(boss.bossHealth.hp <= 0)
            return;

        ChangeState(states[BossMainState.Destruct]);
    }

    public void OnCutting()
    {
        ChangeState(states[BossMainState.Cut]);
    }

    #endregion
}
