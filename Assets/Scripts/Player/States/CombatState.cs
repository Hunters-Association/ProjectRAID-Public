using UnityEngine;
using ProjectRaid.Extensions;
using System;

/// <summary>
/// 전투 상태: 무기 꺼내기 애니메이션 재생 후 공격/조준 입력 처리
/// </summary>
public class CombatState : PlayerState
{
    private bool isDrawFinished = false;
    // private bool isDodgeFinished = true;
    private bool queuedAttack = false;
    private bool queuedAim = false;
    private bool hasReleasedAimOnce = false;

    public CombatState(PlayerController player, PlayerStateMachine stateMachine)
        : base(player, stateMachine) { }

    #region LIFECYCLE
    public override void Enter()
    {
        if (player.TestMode) Debug.Log($"[{this}] - Enter");

        isDrawFinished = false;
        queuedAttack = false;
        queuedAim = false;
        hasReleasedAimOnce = false;
        // player.Animator.ResetTrigger(PlayerAnimatorParams.Equip);
        // player.Animator.ResetTrigger(PlayerAnimatorParams.Unequip);

        if (player.Animator.GetBool(PlayerAnimatorParams.InIdle) && !player.IsInEquip)
        {
            // player.ResetInteractionUI();
            player.Animator.SetTrigger(PlayerAnimatorParams.Equip);
        }
        else
        {
            isDrawFinished = true;
        }

        // player.BodyRig.DOWeight(0f, 0.25f);
        player.SwitchCamera(CameraType.Combat);
        player.Animator.SetBool(PlayerAnimatorParams.InIdle, false);
        player.Animator.SetBool(PlayerAnimatorParams.InCombat, true);
        player.Animator.SetBool(PlayerAnimatorParams.InAttack, false);
    }

    public override void Exit()
    {
        if (player.TestMode) Debug.Log($"[{this}] - Exit");
        // player.Animator.SetBool(PlayerAnimatorParams.InCombat, false);
    }
    #endregion

    #region INPUT & UPDATE
    public override void HandleInput()
    {
        if (player.IsInEquip) return;

        if (player.InputHandler.IsAttackPressed ||
            player.InputHandler.IsChargeHolded)
        {
            queuedAttack = true;
        }

        var dodgeStamina = 20f;

        // if (isDrawFinished)
        {
            if (!player.IsInDodge && !player.IsInEquip && player.InputHandler.IsDodgePressed && player.Stats.Runtime.CurrentStamina >= dodgeStamina)
            {
                // isDodgeFinished = false;
                player.Hurtbox.enabled = false;
                player.Animator.SetTrigger(PlayerAnimatorParams.DodgeTrigger);

                // player.Stats.ConsumeStamina(dodgeStamina);
                // Debug.Log($"CS - 회피 실행됨! {player.IsInDodge} | {player.InputHandler.IsDodgePressed} | {player.Stats.Runtime.CurrentStamina >= dodgeStamina}");
                return;
            }

            if (player.InputHandler.IsInteractPressed)
            {
                stateMachine.ChangeState(player.IdleState);
                return;
            }

            if (!player.IsAiming)
            {
                hasReleasedAimOnce = true;
                return;
            }

            if (hasReleasedAimOnce) queuedAim = true;
        }
    }

    public override void Update()
    {
        if (player.IsInEquip) return;

        if (queuedAttack)
        {
            stateMachine.ChangeState(player.AttackState);
        }
        else if (queuedAim)
        {
            if (player.Weapon.GetWeaponData().CanAim())
            {
                stateMachine.ChangeState(player.AimingState);
            }
            else
            {
                stateMachine.ChangeState(player.IdleState);
            }
        }
    }

    public override void PhysicsUpdate() { }
    public override void LateUpdate() { }
    #endregion

    #region ANIMATION EVENT HOOKS
    /// <summary> 애니메이션 이벤트 훅: 무기 꺼내기 완료 </summary>
    public void OnDrawAnimationEnd()
    {
        // isDrawFinished = true;
    }

    /// <summary> 애니메이션 이벤트 훅: 회피 완료 </summary>
        [Obsolete]
    public void OnDodgeAnimationEnd()
    {
        // player.Hurtbox.enabled = true;
        // isDodgeFinished = true;
    }
    #endregion
}
