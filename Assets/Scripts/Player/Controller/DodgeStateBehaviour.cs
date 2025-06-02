using System.Collections.Generic;
using UnityEngine;
using ProjectRaid.Core;
using DG.Tweening;

public class DodgeStateBehaviour : StateMachineBehaviour
{
    // ──────────────────────────────────────
    #region INSPECTOR
    [Space(15), Header("1) 루트모션"), Space(5)]
    public bool useRootMotion = true;
    [Range(0f, 1f)] public float rootMotionScale = 1f;

    [Space(15), Header("2) 히트박스"), Space(5)]
    public Vector2 hurtboxTiming = new(0.25f, 0.75f);
    #endregion
    // ──────────────────────────────────────

    private PlayerController player;
    private bool isHurtboxOn = true;

    // 1) OnStateEnter : 초기화
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        CacheRefs(animator);

        player.Stats.ConsumeStamina(20f);
        player.SetDodgeState(true);
    }

    // 2) OnStateUpdate : 실제 회피 판정 계산
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 0~1 범위로 고정 (클립 loop=off 가정)
        float t = stateInfo.normalizedTime;

        // 히트박스 On/Off
        if (t >= hurtboxTiming.x && isHurtboxOn)
        {
            isHurtboxOn = false;
            player.CombatController.HurtboxDisable();
            animator.SetBool(PlayerAnimatorParams.LockTurn, true);
            animator.SetBool(PlayerAnimatorParams.UseRootMotion, useRootMotion);
        }

        if (t >= hurtboxTiming.y && !isHurtboxOn)
        {
            isHurtboxOn = true;
            player.CombatController.HurtboxEnable();
            player.SetDodgeState(false);
            animator.SetBool(PlayerAnimatorParams.LockTurn, false);
            animator.SetBool(PlayerAnimatorParams.UseRootMotion, false);
        }
    }

    // 3) OnStateExit : 뒷정리
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 히트박스 On
        player.CombatController.HurtboxEnable();
        player.SetDodgeState(false);

        animator.ResetTrigger(PlayerAnimatorParams.DodgeTrigger);
        animator.SetBool(PlayerAnimatorParams.UseRootMotion, false);
    }

    // 4) 루트모션 스케일링
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
