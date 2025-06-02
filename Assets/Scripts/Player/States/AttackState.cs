using UnityEngine;
using ProjectRaid.Core;
using ProjectRaid.Data;
using ProjectRaid.Extensions;
using DG.Tweening;
using System;

/// <summary>
/// 공격 상태: 입력 잠금, 애니메이션 재생 후 Idle 복귀
/// </summary>
public class AttackState : PlayerState
{
    #region CAPABILITY OVERRIDE
    // public override bool CanMove => false;
    // public override bool CanRun => false;
    public override bool CanInteract => false;
    #endregion

    // private bool receivedNextInput = false;
    // private bool isAnimFinished = false;
    private bool canCharge = false;
    private bool canCombo = false;
    private int currentCombo = 0;
    private int maxCombo = 4;

    private WeaponData WeaponData => player.WeaponManager.CurrentData;

    public AttackState(PlayerController player, PlayerStateMachine stateMachine)
        : base(player, stateMachine) { }

    #region LIFECYCLE
    public override void Enter()
    {
        if (player.TestMode) Debug.Log($"[{this}] - Enter");

        maxCombo = WeaponData.MaxCombo;
        canCharge = WeaponData.SupportsCharge;

        // 초기값 세팅
        // currentCombo = 1;
        // PlayCombo();

        player.Animator.SetTrigger(PlayerAnimatorParams.AttackTrigger);
        // player.BodyRig.DOWeight(1f, 0.25f);

        // 전투 카메라로 전환 (조준 상태면 유지)
        if (player.PreviousState is not AimingState)
        {
            player.SwitchCamera(CameraType.Combat);
        }
        player.Animator.SetBool(PlayerAnimatorParams.InIdle, false);
        player.Animator.SetBool(PlayerAnimatorParams.InCombat, false);
        player.Animator.SetBool(PlayerAnimatorParams.InAttack, true);
    }

    public override void Exit()
    {
        if (player.TestMode) Debug.Log($"[{this}] - Exit");

        // AttackIndex 리셋 → 0 은 Idle 로 간주
        // player.Animator.SetInteger(PlayerAnimatorParams.AttackIndex, 0);
        // player.Animator.SetBool(PlayerAnimatorParams.InAttack, false);
        InitFlags();
    }
    #endregion

    #region INPUT & UPDATE
    public override void HandleInput()
    {
        if (canCombo && player.InputHandler.IsAttackPressed)
        {
            // receivedNextInput = true;
        }
    }

    public override void Update()
    {
        if (!player.Animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack") &&
            !player.Animator.GetNextAnimatorStateInfo(0).IsTag("Attack"))
        {
            stateMachine.ChangeState(player.CombatState);
        }
        
        // if (IsAnimDone())
        //     stateMachine.ChangeState(player.PreviousState is AimingState
        //         ? player.AimingState : player.CombatState);
    }

    public override void PhysicsUpdate() { }
    public override void LateUpdate() { }
    #endregion

    private bool IsAnimDone()
    {
        var info = player.Animator.GetCurrentAnimatorStateInfo(0);
        return info.normalizedTime >= 1f && !player.Animator.GetBool(PlayerAnimatorParams.AttackQueued);
    }

    #region FLAG MANAGEMENT
    private void InitFlags()
    {
        // receivedNextInput = false;
        // isAnimFinished = false;
        canCombo = false;
    }
    #endregion

    #region ANIMATION CONTROL
    private void PlayCombo()
    {
        // 입력 플래그 초기화
        InitFlags();

        player.Animator.SetInteger(PlayerAnimatorParams.AttackIndex, currentCombo);

        Debug.Log($"{currentCombo}단 콤보 발동!");

        // 차지 공격 지원 무기 & 첫 타이면 Charge Bool 세팅 예시
        if (canCharge && currentCombo == 1)
        {
            // bool isHolding = player.InputHandler.IsAttackPressed; // 간단 예시 – 길게 누르면 차지로 전환하도록 확장 가능
            // player.Animator.SetBool(PlayerAnimatorParams.Charge, isHolding);
        }
    }
    #endregion

    #region ANIMATION EVENT HOOKS
    /// <summary> 애니메이션 이벤트 훅: 타격 판정 시작 </summary>
    [Obsolete]
    public void OnAttackStart()
    {
        Debug.LogWarning("[AttackState] 오래된 메서드 사용중!! - 'OnAttackStart()' is Obsolete");
        
        // player.WeaponManager.CurrentWeapon.ResetHitRegistry();
        // player.WeaponManager.CurrentWeapon.SetHitbox(true);
    }

    /// <summary> 애니메이션 이벤트 훅: 콤보 입력창 오픈 </summary>
    [Obsolete]
    public void EnableComboInput()
    {
        Debug.LogWarning("[AttackState] 오래된 메서드 사용중!! - 'EnableComboInput()' is Obsolete");
    }

    /// <summary> 애니메이션 이벤트 훅: 공격 효과음 재생 </summary>
    public void PlayAttackSound()
    {
        player.WeaponManager.PlayWeaponSFX(WeaponSFX.Attack);
    }

    /// <summary> 애니메이션 이벤트 훅: 공격 효과음 재생 </summary>
    public void PlayAttackEffect()
    {
        // player.WeaponManager.PlayWeaponSFX(WeaponSFX.Attack);
    }

    /// <summary> 애니메이션 이벤트 훅: 현재 공격 종료 </summary>
    public void OnAttackEnd()
    {
        // var weapon = player.WeaponManager.CurrentWeapon;
        // if (weapon.GetWeaponData().Class is not WeaponClass.Rifle)
        // {
        //     weapon.SetHitbox(false);
        // }
        // isAnimFinished = true;
        // player.WeaponManager.ComboUI.DOFade(0f, 0.2f);
    }
    #endregion
}
