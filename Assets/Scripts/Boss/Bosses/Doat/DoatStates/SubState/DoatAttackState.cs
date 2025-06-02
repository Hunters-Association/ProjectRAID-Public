using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 공격 애니메이션을 실행 시켜줄 상태
public class DoatAttackState : SubState
{

    private AttackPattern attackPattern;


    public DoatAttackState(BossStateMachine stateMachine, MainState parent, AttackPattern attackPattern) : base(stateMachine, parent)
    {
        this.attackPattern = attackPattern;
    }

    public override void Enter()
    {
        base.Enter();

        StartAnimation("Attack");
        attackPattern?.Execute();
    }

    public override void Exit()
    {
        base.Exit();

        StopAnimation("Attack");

        stateMachine.boss.bodyCollider.enabled = true;
    }

    public override void Update()
    {
        base.Update();

        if(attackPattern.IsFinish())
        {
            // 연속 공격이 있다면 다음 패턴 실행
            if(attackPattern.IsNextAttack(out AttackPattern nextAttackPattern))
            {
                parent.ChangeSubState(new DoatAttackState(stateMachine, parent, nextAttackPattern));
            }
            // 연속공격이 없다면 상태 선택
            else
            {
                DoatCombatState doatCombatState = parent as DoatCombatState;

                parent.ChangeSubState(doatCombatState.stateSelect);
            }

            return;
        }
    }
}
