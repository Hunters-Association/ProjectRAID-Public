using UnityEngine;
using ProjectRaid.EditorTools;

public class PlayerAnimationEventRelay : MonoBehaviour
{
    [FoldoutGroup("레퍼런스", ExtendedColor.Orange)]
    [SerializeField] private PlayerController player;

    [FoldoutGroup("오디오", ExtendedColor.Orange)]
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField][Range(0, 1)] private float audioVolume = 0.5f;
    [SerializeField] private CharacterController controller;

    /// <summary>
    /// 애니메이션 이벤트: 발걸음에 맞게 사운드 재생
    /// </summary>
    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            if (footstepClips != null && footstepClips.Length > 0)
            {
                var index = Random.Range(0, footstepClips.Length);

                if (controller != null)
                {
                    AudioSource.PlayClipAtPoint(footstepClips[index], transform.TransformPoint(controller.center), audioVolume);
                }
                else
                {
                    AudioSource.PlayClipAtPoint(footstepClips[index], transform.position, audioVolume);
                }
            }
        }
    }

    #region ANIMATION EVENTS
    // ────────────────────────────────────────────────────────────────────────────────────────────────────
    public void OnAttackAnimationStart()
    {
        if (player.CurrentState is not AttackState attackState) return;
        
        // attackState.OnAttackStart();
        Debug.Log("\"공격\" 애니메이션 - 시작");
    }

    public void OnPlayAttackSound()
    {
        if (player.CurrentState is not AttackState attackState) return;

        attackState.PlayAttackSound();
        Debug.Log("\"공격\" 애니메이션 - SFX 재생");
    }

    public void OnPlayAttackEffect()
    {
        if (player.CurrentState is not AttackState attackState) return;

        attackState.PlayAttackSound();
        Debug.Log("\"공격\" 애니메이션 - VFX 재생");
    }

    public void OnAttackAnimationEnd()
    {
        if (player.CurrentState is not AttackState attackState) return;

        attackState.OnAttackEnd();
        Debug.Log("\"공격\" 애니메이션 - 종료");
    }

    // ────────────────────────────────────────────────────────────────────────────────────────────────────
    public void OnDrawAnimationEnd()
    {
        (player.CurrentState as CombatState)?.OnDrawAnimationEnd();
    }

    public void OnAttachWeaponToHand()
    {
        player.WeaponManager.AttachToHand();
    }

    public void OnAttachWeaponToBack()
    {
        player.WeaponManager.AttachToBack();
    }

    public void OnEnterCombat()
    {
        // player.WeaponManager.SetCombatState(true);
    }

    public void OnExitCombat()
    {
        // player.WeaponManager.SetCombatState(false);
    }

    // ────────────────────────────────────────────────────────────────────────────────────────────────────
    public void OnDodgeStart()
    {
        // player.SetDodgeState(true);
    }

    public void OnDodgeEnd()
    {
        // player.SetDodgeState(false);
    }

    // ────────────────────────────────────────────────────────────────────────────────────────────────────
    // public void OnDisableBodyRig()
    //     => player.rigController.SetRigWeight(RigType.Body, 0f);
    // public void OnEnableBodyRig()
    //     => player.rigController.SetRigWeight(RigType.Body, 1f);

    // public void OnDisableHandRig()
    //     => player.rigController.SetRigWeight(RigType.Hand, 0f);
    // public void OnEnableHandRig()
    //     => player.rigController.SetRigWeight(RigType.Hand, 1f);

    // public void OnDisableWeaponAim()
    //     => player.rigController.SetConstraintWeight(WeaponConstraintType.WeaponAim, 0f);
    // public void OnEnableWeaponAim()
    //     => player.rigController.SetConstraintWeight(WeaponConstraintType.WeaponAim, 1f);

    // public void OnDisableWeaponPosition()
    //     => player.rigController.SetConstraintWeight(WeaponConstraintType.WeaponPosition, 0f);
    // public void OnEnableWeaponPosition()
    //     => player.rigController.SetConstraintWeight(WeaponConstraintType.WeaponPosition, 1f);

    // public void OnDisableHandIK(WeaponManager.HandType type)
    //     => player.rigController.SetConstraintWeight(type, 0f);
    // public void OnEnableHandIK(WeaponManager.HandType type)
    //     => player.rigController.SetConstraintWeight(type, 1f);
    #endregion
}
