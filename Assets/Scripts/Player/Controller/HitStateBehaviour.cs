using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitStateBehaviour : StateMachineBehaviour
{
    // ──────────────────────────────────────
    #region INSPECTOR
    [Space(15), Header("1) 루트모션"), Space(5)]
    public bool useRootMotion = true;
    [Range(0f, 1f)] public float rootMotionScale = 1f;
    public bool lockTurn = false;
    #endregion
    // ──────────────────────────────────────

    private PlayerController player;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        CacheRefs(animator);
        animator.SetBool(PlayerAnimatorParams.UseRootMotion, useRootMotion);
        // animator.SetBool(PlayerAnimatorParams.AttackQueued, false);
        animator.SetBool(PlayerAnimatorParams.LockTurn, lockTurn);

        animator.ResetTrigger(PlayerAnimatorParams.AttackTrigger);
        animator.ResetTrigger(PlayerAnimatorParams.RunTrigger);

        if (player.Animator.GetBool(PlayerAnimatorParams.InIdle))
            player.StateMachine.ChangeState(player.IdleState);
        else
            player.StateMachine.ChangeState(player.CombatState);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.ResetTrigger(PlayerAnimatorParams.Hit);
        animator.SetFloat(PlayerAnimatorParams.HitDirection, 0f);
        animator.SetBool(PlayerAnimatorParams.UseRootMotion, false);
    }

    public override void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!useRootMotion || rootMotionScale <= 0f) return;

        // 애니메이터가 계산한 deltaPosition을 원하는 스케일로 적용
        Vector3 delta = animator.deltaPosition * rootMotionScale;
        delta.y = 0f;
        player.CharacterController.Move(delta);
    }

    #region PRIVATE-HELPER
    private void CacheRefs(Animator animator)
    {
        if (player == null)
            player = animator.GetComponentInParent<PlayerController>();
    }
    #endregion
}
