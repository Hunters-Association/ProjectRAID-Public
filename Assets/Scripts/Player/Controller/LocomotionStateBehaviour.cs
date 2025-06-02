using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using ProjectRaid.EditorTools;

public class LocomotionStateBehaviour : StateMachineBehaviour
{
    // ──────────────────────────────────────
    #region INSPECTOR
    [Space(15), Header("1) 루트모션"), Space(5)]
    public bool useRootMotion = false;
    [Range(0f, 1f)] public float rootMotionScale = 1f;

    public bool isCombatLocomotion;
    public bool enableLeftHandIK = false;
    // public bool lockTurn = false;
    #endregion
    // ──────────────────────────────────────

    private PlayerController player;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        CacheRefs(animator);
        animator.SetBool(PlayerAnimatorParams.UseRootMotion, false);
        // animator.SetBool(PlayerAnimatorParams.AttackQueued, false);
        animator.SetBool(PlayerAnimatorParams.InAttack, false);
        animator.SetBool(PlayerAnimatorParams.LockTurn, false);

        animator.ResetTrigger(PlayerAnimatorParams.AttackTrigger);
        animator.ResetTrigger(PlayerAnimatorParams.RunTrigger);

        if (isCombatLocomotion && player.CurrentState is not CombatState)
            player.StateMachine.ChangeState(player.CombatState);
        
        if (!isCombatLocomotion && player.CurrentState is not IdleState)
            player.StateMachine.ChangeState(player.IdleState);

        DOTween.Kill(player.WeaponManager.ComboUI);
        player.WeaponManager.ComboUI.alpha = 0f;

        if (isCombatLocomotion && enableLeftHandIK)
            player.SetHandIK(true);
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // if (animator.GetBool(PlayerAnimatorParams.UseRootMotion))
        //     animator.SetBool(PlayerAnimatorParams.UseRootMotion, false);

        // if (animator.GetBool(PlayerAnimatorParams.AttackQueued))
        //     animator.SetBool(PlayerAnimatorParams.AttackQueued, false);

        // if (animator.GetBool(PlayerAnimatorParams.InAttack))
        //     animator.SetBool(PlayerAnimatorParams.InAttack, false);

        // if (animator.GetBool(PlayerAnimatorParams.LockTurn))
        //     animator.SetBool(PlayerAnimatorParams.LockTurn, false);

        // if (player.CurrentState is AttackState)
        //     player.StateMachine.ChangeState(player.CombatState);

        // if (player.WeaponManager.ComboUI.alpha > 0f)
        // {
        //     DOTween.Kill(player.WeaponManager.ComboUI);
        //     player.WeaponManager.ComboUI.alpha = 0f;
        // }

        // if (isCombatLocomotion && !player.WeaponManager.IsWeaponInHand)
        // {
        //     player.Animator.SetBool(PlayerAnimatorParams.InIdle, true);
        //     player.Animator.SetBool(PlayerAnimatorParams.InCombat, false);
        // }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.ResetTrigger(PlayerAnimatorParams.Hit);
        animator.SetFloat(PlayerAnimatorParams.HitDirection, 0f);

        if (isCombatLocomotion)
            player.SetHandIK(false);
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
