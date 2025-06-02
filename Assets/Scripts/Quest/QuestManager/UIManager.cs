using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using ProjectRaid.EditorTools;
using DG.Tweening;

public class UIManager : MonoSingleton<UIManager>, IInitializable
{
    [FoldoutGroup("UI Config", ExtendedColor.Silver)]
    [SerializeField] private UIConfig uIConfig;
    [SerializeField] private bool debugMode = false;

    [FoldoutGroup("Root", ExtendedColor.White)]
    [SerializeField] private Canvas uICanvas;
    [SerializeField] private Transform uIRoot;
    [SerializeField] private Transform popupRoot;

    public Canvas Canvas => uICanvas;

    private readonly Dictionary<Type, BaseUI> activeUIs = new();
    private readonly Dictionary<Type, Queue<BaseUI>> uIPool = new();

    public IEnumerator Initialize()
    {
        Debug.Log("<color=#c0c0c0><b>[UIManager]</b> 초기화 시작</color>");

        if (uIConfig != null)
            uIConfig.Initialize(debugMode);
        else
            Debug.LogError("[UIManager] UIConfig가 등록되지 않았습니다!");

        SceneManager.sceneUnloaded += OnSceneUnloaded;
        SceneManager.sceneLoaded += OnSceneLoaded;

        Debug.Log("<color=#c0c0c0><b>[UIManager]</b> 초기화 성공!</color>");

        yield break;
    }
    
    private void OnDestroy()
    {
        // 이벤트 구독 해제
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// 씬이 언로드될 때 호출되어 모든 활성 UI를 정리합니다.
    /// </summary>
    private void OnSceneUnloaded(Scene current)
    {
        ClearAllUI();
    }

    /// <summary>
    /// 씬이 로드될 때 호출되어 UI 루트 Transform 참조를 다시 설정합니다.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // if (uICanvas != null && uICanvas.renderMode is RenderMode.ScreenSpaceCamera)
        //     uICanvas.worldCamera = Camera.main;
    }

    /// <summary>
    /// 지정된 타입의 UI를 화면에 표시합니다.
    /// 이미 활성화된 경우 해당 인스턴스를 반환합니다.
    /// 풀에 비활성화된 인스턴스가 있으면 재사용하고, 없으면 새로 생성합니다.
    /// </summary>
    /// <typeparam name="T">표시할 UI의 타입 (BaseUI 상속)</typeparam>
    /// <param name="isPopup">팝업 레이어에 표시할지 여부</param>
    /// <returns>표시된 UI 컴포넌트의 인스턴스 또는 null</returns>
    public T ShowUI<T>(bool isPopup = false) where T : BaseUI
    {
        // 0) 이미 떠 있으면 즉시 반환
        if (activeUIs.TryGetValue(typeof(T), out var existing))
        {
            existing.transform.SetAsLastSibling();
            existing.OnShow();
            return (T)existing;
        }

        // 1) 프리팹 가져오기
        var prefab = uIConfig.GetPrefab(typeof(T));
        Transform parent = isPopup ? popupRoot : uIRoot;
        if (!parent)
        {
            Debug.LogError($"[UIManager] UI root가 등록되지 않았습니다!");
            return null;
        }

        // 2) 프리팹 생성 (큐에 있으면 Dequeue)
        BaseUI ui;
        if (uIPool.TryGetValue(typeof(T), out var q) && q.Count > 0)
            ui = q.Dequeue();
        else
            ui = Instantiate(prefab, parent);

        // 3) 초기화
        ui.Initialize();
        ui.OnShow();

        // 4) 활성 UI 목록에 추가
        activeUIs[typeof(T)] = ui;
        return (T)ui;
    }

    /// <summary>
    /// 지정된 타입의 UI를 화면에서 숨깁니다. (풀링 사용 시 풀에 반환)
    /// </summary>
    /// <typeparam name="T">숨길 UI의 타입 (BaseUI 상속)</typeparam>
    /// <param name="returnToPool">숨긴 후 풀에 반환할지 여부</param>
    public void HideUI<T>(bool returnToPool = true) where T : BaseUI
    {
        Type type = typeof(T);
        
        if (activeUIs.TryGetValue(type, out var uI))
        {
            if (uI != null) // 유효한 인스턴스인지 확인
            {
                // 활성 목록에서 제거
                uI.OnHide();
                activeUIs.Remove(type);

                DOVirtual.DelayedCall(uI.TransitionDuration, () =>
                {
                    if (returnToPool)
                    {
                        if (uI.gameObject != null)
                        {
                            // 해당 타입의 큐가 없으면 새로 생성
                            if (!uIPool.ContainsKey(type))
                                uIPool[type] = new Queue<BaseUI>();

                            uIPool[type].Enqueue(uI);
                        }
                    }
                    else
                    {
                        if (uI.gameObject != null)
                        {
                            Destroy(uI.gameObject);
                            Debug.Log($"[UIManager] '{type}' Destroyed (not pooling).");
                        }
                    }
                });
            }
            else
            {
                // 참조는 있지만 오브젝트가 null이면 목록에서 제거
                activeUIs.Remove(type);
            }
        }
    }

    public void HideUIByType(Type type, bool returnToPool = true)
    {
        if (activeUIs.TryGetValue(type, out var uI))
        {
            if (uI != null) // 유효한 인스턴스인지 확인
            {
                uI.OnHide(); // 비활성화 및 관련 로직 실행 (BaseUI 구현에 따라 SetActive(false) 포함)
                activeUIs.Remove(type); // 활성 목록에서 제거

                if (returnToPool && uI.gameObject.activeSelf == false)
                {
                    if (!uIPool.ContainsKey(type)) uIPool[type] = new Queue<BaseUI>();
                    uIPool[type].Enqueue(uI);
                }
                else if (!returnToPool)
                {
                    Destroy(uI.gameObject);
                }

                CheckAndSetCursorState();
            }
            else { activeUIs.Remove(type); }
        }
    }

    /// <summary>
    /// 지정된 타입의 UI를 완전히 제거합니다. (풀링 안 함)
    /// </summary>
    public void RemoveUI<T>() where T : BaseUI
    {
        HideUI<T>(false); // 풀에 반환하지 않고 숨김
    }

    /// <summary>
    /// 현재 활성화된 모든 UI를 정리합니다. (씬 전환 시 호출됨)
    /// </summary>
    public void ClearAllUI()
    {
        var keys = activeUIs.Keys.ToList();
        foreach (var key in keys)
        {
            if (activeUIs.TryGetValue(key, out var ui) && ui != null)
            {
                if (!uIConfig.IsPersistent(ui))
                {
                    Destroy(ui.gameObject);
                    activeUIs.Remove(key);
                }
            }
        }

        foreach (var kv in uIPool)
        {
            var queue = kv.Value;
            int originalCount = queue.Count;
            for (int i = 0; i < originalCount; i++)
            {
                var pooledUI = queue.Dequeue();
                if (pooledUI == null) continue;

                if (uIConfig.IsPersistent(pooledUI))
                    queue.Enqueue(pooledUI);
                else
                    Destroy(pooledUI.gameObject);
            }
        }

        CheckAndSetCursorState();
    }

    /// <summary>
    /// 특정 타입의 UI가 현재 활성화(표시) 상태인지 확인합니다.
    /// </summary>
    public bool IsUIActive<T>() where T : BaseUI
    {
        return activeUIs.TryGetValue(typeof(T), out var ui) && ui != null && ui.gameObject.activeSelf;
    }

    /// <summary>
    /// 현재 활성화된 특정 타입 UI의 컴포넌트 인스턴스를 반환합니다. 없으면 null.
    /// </summary>
    public T GetActiveUIInstance<T>() where T : BaseUI
    {
        activeUIs.TryGetValue(typeof(T), out var ui);
        return ui as T;
    }

    private void CheckAndSetCursorState()
    {
        bool hasBlockingUI = IsBlockingUIActive();
        CursorManager.SetCursorState(hasBlockingUI);
    }

    public bool IsBlockingUIActive()
    {
        return activeUIs.Values.Any(ui =>
            ui != null &&
            ui.isActiveAndEnabled &&
            ui is IBlockingUI blocking &&
            blocking.BlocksGameplay);
    }
}

