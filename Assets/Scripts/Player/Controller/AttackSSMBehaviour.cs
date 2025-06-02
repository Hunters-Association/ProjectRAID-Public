using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackSSMBehaviour : StateMachineBehaviour
{
    private PlayerController player;

    // Attack SSM 전체를 빠져나갈 때 호출
    public override void OnStateMachineExit(Animator animator, int layerIndex)
    {
        CacheRefs(animator);

        animator.SetBool(PlayerAnimatorParams.AttackQueued, false);
        animator.SetBool(PlayerAnimatorParams.UseRootMotion, false);
        animator.SetBool(PlayerAnimatorParams.LockTurn, false);
        // animator.SetBool(PlayerAnimatorParams.CanCombo, false);

        animator.ResetTrigger(PlayerAnimatorParams.AttackTrigger);
        animator.ResetTrigger(PlayerAnimatorParams.RunTrigger);

        // if (player.CurrentState is AttackState)
        //     player.StateMachine.ChangeState(player.CombatState);
    }

    #region PRIVATE-HELPER
    private void CacheRefs(Animator animator)
    {
        if (player == null)
            player = animator.GetComponentInParent<PlayerController>();
    }
    #endregion
}
