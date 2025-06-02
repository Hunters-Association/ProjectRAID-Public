using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using ProjectRaid.EditorTools;

public enum PlayerAction
{
    Aim,
    Attack,
    Dodge,
    Escape,
    Interact,
    Inventory,
    Look,
    Move,
    Quest,
    Run,
    Scan,
    Charge,
}

[Serializable]
public class PlayerActionBinding
{
    public PlayerAction type;
    public InputActionReference reference;
}

/// <summary>
/// 플레이어 입력을 캡슐화하여 이벤트 기반으로 처리하는 헬퍼 클래스
/// </summary>
[RequireComponent(typeof(PlayerInput))]
public class PlayerInputHandler : MonoBehaviour
{
    [FoldoutGroup("인풋 액션", ExtendedColor.DodgerBlue)]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PlayerInputMap playerInputMap;

    [FoldoutGroup("마우스 입력 설정", ExtendedColor.Silver)]
    [SerializeField] private List<CinemachineInputProvider> inputProviders = new();
    [SerializeField] private InputActionReference lookActionReference;
    [SerializeField, Range(0f, 1f)] private float cursorInputMultiplier = 1f;

    private PlayerController player;

    public PlayerInput PlayerInput => playerInput;

    /// <summary>
    /// 달리기 입력 상태 (외부에서 접근 가능)
    /// </summary>
    public bool IsRunning { get; private set; }

    /// <summary>
    /// 조준 입력 상태 (외부에서 접근 가능)
    /// </summary>
    public bool IsAiming { get; private set; }

    /// <summary>
    /// 현재 프레임 이동 입력값
    /// </summary>
    public Vector2 MoveInput
    {
        get
        {
            if (actionMap.TryGetValue(PlayerAction.Move, out var act))
                return act.ReadValue<Vector2>();

            Debug.LogWarning("[PlayerInputHandler] Move 액션이 없습니다. Vector2.zero 반환");
            return Vector2.zero;
        }
    }

    /// <summary>
    /// 현재 프레임 화면 회전값
    /// </summary>
    public Vector2 LookInput
    {
        get
        {
            if (actionMap.TryGetValue(PlayerAction.Look, out var act))
                return act.ReadValue<Vector2>() * (Time.timeScale * cursorInputMultiplier);

            Debug.LogWarning("[PlayerInputHandler] Look 액션이 없습니다. Vector2.zero 반환");
            return Vector2.zero;
        }
    }

    /// <summary>
    /// 이번 프레임에 달리기 입력이 있었는지 여부
    /// </summary>
    public bool IsRunPressed => WasPerformedThisFrame(PlayerAction.Run);

    /// <summary>
    /// 이번 프레임에 스캔 입력이 있었는지 여부
    /// </summary>
    public bool IsScanPressed => WasPerformedThisFrame(PlayerAction.Scan);

    /// <summary>
    /// 이번 프레임에 회피 입력이 있었는지 여부
    /// </summary>
    public bool IsDodgePressed => WasPerformedThisFrame(PlayerAction.Dodge);

    /// <summary>
    /// 이번 프레임에 공격 입력이 있었는지 여부
    /// </summary>
    public bool IsAttackPressed => WasPerformedThisFrame(PlayerAction.Attack);

    /// <summary>
    /// 이번 프레임에 ESC 입력이 있었는지 여부
    /// </summary>
    public bool IsEscapePressed => WasPerformedThisFrame(PlayerAction.Escape);

    /// <summary>
    /// 이번 프레임에 상호작용 입력이 있었는지 여부
    /// </summary>
    public bool IsInteractPressed => WasPerformedThisFrame(PlayerAction.Interact);

    /// <summary>
    /// 이번 프레임에 인벤토리 입력이 있었는지 여부
    /// </summary>
    public bool IsInventoryPressed => WasPerformedThisFrame(PlayerAction.Inventory);

    /// <summary>
    /// 이번 프레임에 차지 입력이 완료되었는지 여부
    /// </summary>
    public bool IsChargeHolded => WasPerformedThisFrame(PlayerAction.Charge);

    /// <summary>
    /// 현재 기기가 마우스를 사용하는지 여부
    /// </summary>
    public bool IsCurrentDeviceMouse => PlayerInput.currentControlScheme == "PC";

    private Dictionary<PlayerAction, InputAction> actionMap = new();

    private void Awake()
    {
        player = GetComponent<PlayerController>();

        // 1) 바인딩 리스트 → 딕셔너리 변환
        actionMap = playerInputMap.actionBindings
            .Where(b => b != null && b.reference != null && b.reference.action != null)
            .ToDictionary
            (
                b => b.type,
                b => b.reference.action
            );

        // 2) 혹시 누락된 액션이 있으면 PlayerInput.asset 에서 찾아 채우기
        var inputAsset = playerInput.actions;
        foreach (PlayerAction action in Enum.GetValues(typeof(PlayerAction)))
        {
            if (!actionMap.ContainsKey(action))
            {
                if (inputAsset.FindAction(action.ToString()) is InputAction fallback)
                {
                    Debug.LogWarning($"[PlayerInputHandler] 누락된 바인딩 '{action}', InputActions에서 등록되었습니다.", this);
                    actionMap[action] = fallback;
                }
                else
                {
                    Debug.LogError($"[PlayerInputHandler] InputActions에 '{action}' 액션 자체가 없습니다!", this);
                }
            }
        }
    }

    private void OnEnable()
    {
        CursorManager.OnCursorStateChange += OnCursorStateChange;

        actionMap[PlayerAction.Run].started += OnRunStarted;
        actionMap[PlayerAction.Run].canceled += OnRunCanceled;
        actionMap[PlayerAction.Aim].started += OnAimStarted;
        actionMap[PlayerAction.Aim].canceled += OnAimCanceled;
        actionMap[PlayerAction.Quest].started += OnQuestBtnPressed;
        actionMap[PlayerAction.Inventory].started += OnInventoryBtnPressed;
    }

    private void OnDisable()
    {
        CursorManager.OnCursorStateChange -= OnCursorStateChange;

        actionMap[PlayerAction.Run].started -= OnRunStarted;
        actionMap[PlayerAction.Run].canceled -= OnRunCanceled;
        actionMap[PlayerAction.Aim].started -= OnAimStarted;
        actionMap[PlayerAction.Aim].canceled -= OnAimCanceled;
        actionMap[PlayerAction.Quest].started -= OnQuestBtnPressed;
        actionMap[PlayerAction.Inventory].started -= OnInventoryBtnPressed;
    }

    private void OnRunStarted(InputAction.CallbackContext context)
    {
        IsAiming = false;
        IsRunning = true;

        player.Animator.SetTrigger(PlayerAnimatorParams.RunTrigger);
        // player.Animator.SetBool(PlayerAnimatorParams.Run, true);
    }

    private void OnRunCanceled(InputAction.CallbackContext context)
    {
        IsRunning = false;

        // player.Animator.SetBool(PlayerAnimatorParams.Run, false);
    }

    private void OnAimStarted(InputAction.CallbackContext context)
    {
        IsRunning = false;
        IsAiming = true;
    }

    private void OnAimCanceled(InputAction.CallbackContext context)
    {
        IsAiming = false;
    }

    private void OnInventoryBtnPressed(InputAction.CallbackContext context)
    {
        if (player.InventoryUI != null)
        {
            bool isActive = player.InventoryUI.gameObject.activeSelf;
            if (isActive)
            {
                player.InventoryUI.OnHide();
            }
            else
            {
                player.InventoryUI.Initialize();
                player.InventoryUI.OnShow();
            }
        }
    }

    private void OnQuestBtnPressed(InputAction.CallbackContext context)
    {
        if (UIManager.Instance != null) // UIManager 인스턴스 확인
        {
            // UIManager를 통해 QuestLogUI 토글
            bool isActive = UIManager.Instance.IsUIActive<QuestLogUI>();
            if (isActive)
            {
                UIManager.Instance.HideUI<QuestLogUI>();
            }
            else
            {
                UIManager.Instance.ShowUI<QuestLogUI>();
            }
            // Debug.Log(...);
        }
        else
        {
            Debug.LogError("[PlayerInputHandler] UIManager 인스턴스를 찾을 수 없습니다!");
        }
    }

    private void OnCursorStateChange(CursorManager.State state)
    {
        if (lookActionReference == null)
        {
            // Debug.LogWarning("[PlayerInputHandler] LookActionReference가 등록되지 않았습니다!");
            return;
        }
        
        foreach (var ip in inputProviders)
        {
            ip.XYAxis = state switch
            {
                CursorManager.State.Show    => null,
                CursorManager.State.Hide    => lookActionReference,
                _                           => lookActionReference
            };
        }
    }

    private bool WasPerformedThisFrame(PlayerAction action)
    {
        return actionMap.TryGetValue(action, out var act) && (act?.WasPerformedThisFrame() ?? false);
    }
}
