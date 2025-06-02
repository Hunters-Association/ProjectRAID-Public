using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossPracticeLookState : SubState
{
    private Vector3 lookDir;
    private Quaternion originRot;
    public float lookPlayTime;  // 회전 누적 시간
    public float lookTime;      // 회전 총 시간

    ActionPattern attackPattern;

    public BossPracticeLookState(BossStateMachine stateMachine, MainState parent, ActionPattern attackPattern = null) : base(stateMachine, parent)
    {
        this.attackPattern = attackPattern;
    }

    public override void Enter()
    {
        base.Enter();

        // TODO : 플레이어의 위치에 따라 다른 회전 애니메이션을 플레이 해야될 수 있음
        StartAnimation("Walk");

        lookPlayTime = 0f;
        lookTime = 0.5f;

        originRot = stateMachine.boss.transform.rotation;
        lookDir = stateMachine.boss.GetTargetDir();
    }

    public override void Exit()
    {
        base.Exit();
        StopAnimation("Walk");
    }

    public override void Update()
    {
        base.Update();

        lookPlayTime += Time.deltaTime;

        float rate = lookPlayTime / lookTime;

        if (lookDir != Vector3.zero)
            stateMachine.boss.transform.rotation = Quaternion.Slerp(originRot, Quaternion.LookRotation(lookDir), rate);

        // 회전이 거의 끝났다면 다음 상태로 변환
        if (rate > 1f)
        {
            if (attackPattern != null)
            {
                parent.ChangeSubState(new BossPracticeAttackState(stateMachine, parent, attackPattern as AttackPattern));
                return;
            }
            else
            {
                BossPracticeCombatState combatState = parent as BossPracticeCombatState;
                parent.ChangeSubState(combatState.roarState);
            }
        }
    }
}
