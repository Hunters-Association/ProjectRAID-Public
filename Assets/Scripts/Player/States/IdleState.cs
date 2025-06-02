using System;
using UnityEngine;
using ProjectRaid.Extensions;

/// <summary>
/// 기본 상태: 이동 가능, 공격/상호작용 가능
/// </summary>
public class IdleState : PlayerState
{
    #region CAPABILITY OVERRIDE
    public override bool CanAttack => false;
    #endregion

    private bool hasReleasedAimOnce = false;

    // private bool isDodgeFinished = true;

    public IdleState(PlayerController player, PlayerStateMachine stateMachine)
        : base(player, stateMachine) { }

    #region LIFECYCLE
    public override void Enter()
    {
        if (player.TestMode) Debug.Log($"[{this}] - Enter");

        hasReleasedAimOnce = false;

        player.Animator.SetFloat(PlayerAnimatorParams.Speed, 0f);
        player.Animator.SetFloat(PlayerAnimatorParams.MotionSpeed, 0f);

        // player.Animator.ResetTrigger(PlayerAnimatorParams.Equip);
        if (player.Animator.GetBool(PlayerAnimatorParams.InCombat) && !player.IsInEquip)
            player.Animator.SetTrigger(PlayerAnimatorParams.Unequip);

        // player.BodyRig.DOWeight(0f, 0.25f);
        player.SwitchCamera(CameraType.Default);

        player.Animator.SetBool(PlayerAnimatorParams.InIdle, true);
        player.Animator.SetBool(PlayerAnimatorParams.InCombat, false);
        player.Animator.SetBool(PlayerAnimatorParams.InAttack, false);
    }

    public override void Exit()
    {
        if (player.TestMode) Debug.Log($"[{this}] - Exit");

        // TODO: 일반 상태만 가지는 요소 숨기기 (비 전투 UI 등)
    }
    #endregion

    #region INPUT & UPDATE
    public override void HandleInput()
    {
        if (player.IsInEquip) return;

        var dodgeStamina = 20f;

        if (!player.IsInDodge && !player.IsInEquip && player.InputHandler.IsDodgePressed && player.Stats.Runtime.CurrentStamina >= dodgeStamina)
        {
            // isDodgeFinished = false;
            player.Hurtbox.enabled = false;
            player.Animator.SetTrigger(PlayerAnimatorParams.DodgeTrigger);

            player.Stats.ConsumeStamina(dodgeStamina);
            Debug.Log("IS - 회피 실행됨!");
            return;
        }

        if (player.InputHandler.IsInteractPressed && player.CurrentInteractable != null)
        {
            stateMachine.ChangeState(player.InteractionState);
            return;
        }

        if (player.Weapon != null)
        {
            if (player.InputHandler.IsAttackPressed)
            {
                player.SetCanRun(false);
                stateMachine.ChangeState(player.CombatState); // 공격 전 전투 상태로 먼저 전이
                return;
            }

            if (!player.IsAiming)
            {
                hasReleasedAimOnce = true;
                return;
            }

            if (!hasReleasedAimOnce) return;

            player.SetCanRun(false);
            stateMachine.ChangeState(player.CombatState);
        }
    }

    public override void Update() { }
    public override void PhysicsUpdate() { }
    public override void LateUpdate() { }
    #endregion

    #region ANIMATION EVENT HOOKS
    /// <summary> 애니메이션 이벤트 훅: 회피 완료 </summary>
    [Obsolete]
    public void OnDodgeAnimationEnd()
    {
        // player.Hurtbox.enabled = true;
        // isDodgeFinished = true;
    }
    #endregion
}
