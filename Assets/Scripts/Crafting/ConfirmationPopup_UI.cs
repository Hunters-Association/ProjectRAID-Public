using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 간단한 예/아니오 확인 팝업 UI를 제어하는 클래스.
/// 해당 UI 프리팹의 루트 게임 오브젝트에 부착해야 합니다.
/// </summary>
public class ConfirmationPopup_UI : BaseUI, IBlockingUI // BaseUI 상속 고려 가능
{
    [Header("UI 요소 참조 (인스펙터 연결)")]
    [SerializeField] private TextMeshProUGUI messageText; // 메시지를 표시할 TextMeshProUGUI
    [SerializeField] private Button yesButton;           // "예" 버튼
    [SerializeField] private Button noButton;            // "아니오" 버튼
    private Action _onConfirm;
    private Action _onCancel;

    public bool BlocksGameplay => true;

    // 버튼 클릭 시 실행될 콜백 함수들을 저장할 변수    

    //void Awake()
    //{
    //    // 버튼이 null이 아닐 경우, 클릭 리스너 연결
    //    // Start 대신 Awake에서 연결하면 비활성화 상태에서도 리스너가 미리 설정됨
    //    yesButton?.onClick.AddListener(OnYesButtonClicked);
    //    noButton?.onClick.AddListener(OnNoButtonClicked);

    //    // 시작 시에는 팝업을 숨겨둠
    //    gameObject.SetActive(false);
    //}
    public override void Initialize()
    {
        // 이미 초기화되었으면 중복 실행 방지
        if (IsInitialized) return;
        base.Initialize(); // 부모 Initialize 호출 (IsInitialized = true 설정 등)

        Debug.Log($"[{gameObject.name}] Initializing Confirmation Popup...");

        // 버튼 리스너 연결 (여기서 한 번만)
        yesButton.onClick.RemoveAllListeners(); // 중복 방지
        yesButton.onClick.AddListener(OnYesButtonClicked);
        noButton.onClick.RemoveAllListeners();
        noButton.onClick.AddListener(OnNoButtonClicked);

        // 필요한 다른 초기화 로직 추가 가능
    }
    
    public void SetMessage(string msg)
    {
        if (messageText != null) messageText.text = msg;
        else Debug.LogError("ConfirmationPopup_UI: Message Text 미할당!", this);
    }
    public void SetupActions(Action onConfirm, Action onCancel)
    {
        _onConfirm = onConfirm;
        _onCancel = onCancel;
    }
    public override void OnShow()
    {
        base.OnShow(); // 기본 활성화, CanvasGroup 설정, 커서 보이기 등
        // Debug.Log($"[{gameObject.name}] Confirmation Popup OnShow.");
        // 필요 시 추가 로직 (예: 애니메이션 시작)
        CursorManager.SetCursorState(true);
    }

    /// <summary>
    /// 팝업을 숨깁니다.
    /// </summary>
    public override void OnHide()
    {
        // Debug.Log($"[{gameObject.name}] Confirmation Popup OnHide.");
        // 콜백 참조 정리 (메모리 누수 방지)
        _onConfirm = null;
        _onCancel = null;

        CursorManager.SetCursorState(false);
        base.OnHide(); // 중요: 기본 비활성화 등 - 맨 마지막 호출 권장
    }

    /// <summary>
    /// "예" 버튼 클릭 시 호출될 내부 메서드.
    /// </summary>
    private void OnYesButtonClicked()
    {
        _onConfirm?.Invoke();
        Close();
    }

    /// <summary>
    /// "아니오" 버튼 클릭 시 호출될 내부 메서드.
    /// </summary>
    private void OnNoButtonClicked()
    {
        _onCancel?.Invoke();
        Close();
    }

    
}
