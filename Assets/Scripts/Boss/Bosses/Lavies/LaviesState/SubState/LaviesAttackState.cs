using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LaviesAttackState : SubState
{
    private AttackPattern attackPattern;


    public LaviesAttackState(BossStateMachine stateMachine, MainState parent, AttackPattern attackPattern) : base(stateMachine, parent)
    {
        this.parent = parent;

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
    }

    public override void Update()
    {
        base.Update();

        if (attackPattern.IsFinish())
        {
            // 연속 공격이 있다면 다음 패턴 실행
            if (attackPattern.IsNextAttack(out AttackPattern nextAttackPattern))
            {
                parent.ChangeSubState(new LaviesAttackState(stateMachine, parent, nextAttackPattern));
            }
            // 연속공격이 없다면 상태 선택
            else
            {
                LaviesCombatState combatState = parent as LaviesCombatState;

                parent.ChangeSubState(combatState.stateSelect);
            }

            return;
        }
    }
}
