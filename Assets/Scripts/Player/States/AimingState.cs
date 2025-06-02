using UnityEngine;
using ProjectRaid.Extensions;
using DG.Tweening;

/// <summary>
/// 조준 상태: 느린 이동, 조준 방향 회전
/// </summary>
public class AimingState : PlayerState
{
    #region CAPABILITY OVERRIDE
    public override bool CanRun => false;
    #endregion

    private readonly string aimingLayer = "Aiming Layer";

    public AimingState(PlayerController player, PlayerStateMachine stateMachine)
        : base(player, stateMachine) { }

    #region LIFECYCLE
    public override void Enter()
    {
        if (player.TestMode) Debug.Log($"[{this}] - Enter");

        player.Animator.SetFloat(PlayerAnimatorParams.Speed, 0f);
        player.Animator.SetFloat(PlayerAnimatorParams.MotionSpeed, 0f);
        // player.Animator.SetBool(PlayerAnimatorParams.Attack, false);
        player.Animator.SetBool(PlayerAnimatorParams.Aim, true);

        if (player.IsAiming)
        {
            player.BodyRig.DOWeight(1f, 0.25f);
            player.AimingVolume.DOWeight(1f, 0.25f).SetEase(Ease.OutSine);
            player.Animator.DOWeight(player.Animator.GetLayerIndex(aimingLayer), 1f, 0.25f);
        }

        player.SwitchCamera(CameraType.Aim);
    }

    public override void Exit()
    {
        if (player.TestMode) Debug.Log($"[{this}] - Exit");

        if (player.IsAiming)
        {
            // player.Animator.SetBool(PlayerAnimatorParams.Attack, true);
        }
        else
        {
            player.Animator.SetBool(PlayerAnimatorParams.Aim, false);
            player.BodyRig.DOWeight(0f, 0.25f);
            player.AimingVolume.DOWeight(0f, 0.2f).SetEase(Ease.InSine);
        }
    }
    #endregion

    #region INPUT & UPDATE
    public override void HandleInput()
    {
        if (!player.IsAiming)
        {
            stateMachine.ChangeState(player.CombatState);
            return;
        }

        if (player.InputHandler.IsAttackPressed)
        {
            stateMachine.ChangeState(player.AttackState);
            return;
        }

        if (player.InputHandler.IsInteractPressed && player.CurrentInteractable != null)
        {
            // 조준 상태에서는 상호작용 불가
            // stateMachine.ChangeState(player.InteractionState);
            return;
        }
    }

    public override void Update() { }
    public override void PhysicsUpdate() { }
    public override void LateUpdate() { }
    #endregion
}
