using UnityEngine;
using ProjectRaid.Extensions;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))] // 여전히 CanvasGroup은 유용할 수 있음 (interactable, blocksRaycasts 제어)
public abstract class BaseUI : MonoBehaviour
{
    protected RectTransform rectTransform; // UI 위치/크기 제어용
    protected CanvasGroup canvasGroup;     // 상호작용, Raycast 제어용

    public float TransitionDuration { get; protected set; } = 0.25f;
    public bool IsInitialized { get; protected set; } = false;

    /// <summary>
    /// 기본적인 컴포넌트 참조를 설정합니다.
    /// </summary>
    protected virtual void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
    }

    /// <summary>
    /// UI 초기 설정을 수행합니다. (자식 클래스에서 재정의 가능)
    /// </summary>
    public virtual void Initialize()
    {
        if (IsInitialized) return;
        IsInitialized = true;
    }

    /// <summary>
    /// UI를 화면에 즉시 표시합니다.
    /// GameObject를 활성화하고 상호작용 가능하게 설정합니다.
    /// 자식 클래스에서 재정의하여 추가 로직을 수행할 수 있습니다.
    /// </summary>
    public virtual void OnShow()
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        // 즉시 상호작용 가능하게 설정
        if (canvasGroup != null)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            canvasGroup
                .DOFade(1f, TransitionDuration)
                .SetEase(Ease.OutQuad);
        }

        transform.SetAsLastSibling();

        // 커서 활성화 & 고정 해제
        CursorManager.SetCursorState(CursorManager.State.Show);
    }

    /// <summary>
    /// UI를 화면에서 즉시 숨깁니다.
    /// GameObject를 비활성화합니다.
    /// 자식 클래스에서 재정의하여 추가 로직을 수행할 수 있습니다.
    /// </summary>
    public virtual void OnHide()
    {
        // 커서 비활성화 & 위치 고정 (중앙)
        CursorManager.SetCursorState(CursorManager.State.Hide);

        // 상호작용 불가능하게 설정 (비활성화 전)
        if (canvasGroup != null)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            canvasGroup
                .DOFade(0f, TransitionDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => gameObject.SetActive(false));
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public virtual void Close()
    {
        UIManager.Instance.HideUIByType(GetType());
    }
}
