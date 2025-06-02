using UnityEngine;
using DG.Tweening;

/// <summary>
/// 사망 상태: 입력 불가, 사망 애니메이션 재생
/// </summary>
public class DeadState : PlayerState
{
    #region CAPABILITY OVERRIDE
    public override bool CanMove => false;
    public override bool CanRun => false;
    public override bool CanAttack => false;
    public override bool CanInteract => false;
    #endregion

    public DeadState(PlayerController player, PlayerStateMachine stateMachine)
        : base(player, stateMachine) { }

    #region LIFECYCLE
    public override void Enter()
    {
        if (player.TestMode) Debug.Log($"[{this}] - Enter");

        player.Animator.SetTrigger(PlayerAnimatorParams.Dead);

        // player.BodyRig.DOWeight(0f, 0.25f);
        player.SwitchCamera(CameraType.Default);

        // TODO: 상태 진입 시 처리할 내용이 있다면 여기에 작성
        // UI 호출, 리스폰 대기 등

        DOVirtual.DelayedCall(3f, () =>
        {
            GameManager.Instance.ReloadScene();
        });
    }

    public override void Exit()
    {
        if (player.TestMode) Debug.Log($"[{this}] - Exit");

        // TODO: 상태 종료 시 처리할 내용이 있다면 여기에 작성
        // 인풋 시스템 재활성화, 리스폰 관련 초기화 등
    }
    #endregion

    #region INPUT & UPDATE
    public override void HandleInput() { }
    public override void Update() { }
    public override void PhysicsUpdate() { }
    public override void LateUpdate() { }
    #endregion
}
