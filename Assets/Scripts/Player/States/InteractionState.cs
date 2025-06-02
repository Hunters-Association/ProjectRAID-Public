using UnityEngine;
using ProjectRaid.Extensions;
using UnityEditor.Rendering;
using UnityEngine.Analytics;

/// <summary>
/// 상호작용 상태: 특정 대상과 상호작용 처리 및 대기 상태로 복귀
/// </summary>
public class InteractionState : PlayerState
{
    #region CAPABILITY OVERRIDE
    public override bool CanMove => false;
    public override bool CanRun => false;
    public override bool CanAttack => false;
    #endregion

    private readonly float interactionDuration = 1f;
    private float timer;
    private bool interactionStarted;

    public InteractionState(PlayerController player, PlayerStateMachine stateMachine)
        : base(player, stateMachine) { }

    #region LIFECYCLE
    public override void Enter()
    {
        if (player.TestMode) Debug.Log($"[{this}] - Enter");

        timer = 0f;
        interactionStarted = false;

        // player.BodyRig.DOWeight(0f, 0.25f);
        player.Animator.SetTrigger(PlayerAnimatorParams.Interact);
    }

    public override void Exit()
    {
        if (player.TestMode) Debug.Log($"[{this}] - Exit");

        // player.CurrentInteractable?.HideHighlight();
    }
    #endregion

    #region INPUT & UPDATE
    public override void Update()
    {
        timer += Time.deltaTime;

        if (!interactionStarted && timer >= 0.25f)
        {
            // 애니메이션이 어느 정도 진행된 후 상호작용 실행
            interactionStarted = true;
            var interactable = player.CurrentInteractable;

            switch (interactable.GetInteractableData().Type)
            {
                case InteractableType.Corpse:
                case InteractableType.Object:
                case InteractableType.NPC:
                default:
                    interactable?.Interact(player);
                    break;
            }
            AnalyticsManager.CallInteractEvent(interactable.GetInteractableData().name, player.transform.position.x, player.transform.position.y, $"{interactable.GetInteractableData().Type}");
        }

        if (timer >= interactionDuration)
        {
            stateMachine.ChangeState(player.IdleState);
        }
    }

    public override void HandleInput() { }
    public override void PhysicsUpdate() { }
    public override void LateUpdate() { }
    #endregion
}
