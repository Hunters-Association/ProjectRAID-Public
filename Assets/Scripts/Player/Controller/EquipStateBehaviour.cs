using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipStateBehaviour : StateMachineBehaviour
{
    public enum Type { Equip, Unequip }

    // ──────────────────────────────────────
    #region INSPECTOR
    [Space(15), Header("1) 루트모션"), Space(5)]
    public bool useRootMotion = true;
    [Range(0f, 1f)] public float rootMotionScale = 1f;

    [Space(15), Header("2) 타이밍"), Space(5)]
    public Type type;
    [Range(0f, 1f)] public float transitionTiming = 0.5f;
    #endregion
    // ──────────────────────────────────────

    private PlayerController player;
    private bool performed = false;

    // 1) OnStateEnter : 초기화
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        CacheRefs(animator);
        animator.SetBool(PlayerAnimatorParams.UseRootMotion, useRootMotion);
        player.SetCanRun(false);
        player.SetEquipState(true);
    }

    // 2) OnStateUpdate : 베이스 레이어 전환
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        float t = stateInfo.normalizedTime % 1f;

        if (t >= transitionTiming && !performed)
        {
            performed = true;
            player.SetCanRun(false);

            switch (type)
            {
                case Type.Equip:
                    player.WeaponManager.AttachToHand();
                    animator.SetBool(PlayerAnimatorParams.InIdle, false);
                    animator.SetBool(PlayerAnimatorParams.InCombat, true);
                    animator.SetBool(PlayerAnimatorParams.InAttack, false);
                    break;

                case Type.Unequip:
                    player.WeaponManager.AttachToBack();
                    animator.SetBool(PlayerAnimatorParams.InIdle, true);
                    animator.SetBool(PlayerAnimatorParams.InCombat, false);
                    animator.SetBool(PlayerAnimatorParams.InAttack, false);
                    break;
            }
        }
    }

    // 3) OnStateExit : 뒷정리
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 히트박스 On
        player.CombatController.HurtboxEnable();

        player.SetEquipState(false);
        player.SetCanRun(true);
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
