using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using ProjectRaid.EditorTools;
using DG.Tweening;

public class Bootstrapper : MonoBehaviour
{
    [FoldoutGroup("매니저", ExtendedColor.White)]
    [SerializeField] private MonoBehaviour[] singletonManagers;
    [SerializeField] private int nextSceneID = 1;
    [SerializeField] private LoadingUI loadingUI;

    [FoldoutGroup("테스트", ExtendedColor.DodgerBlue)]
    [SerializeField] private bool testScene = false;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (testScene) StartCoroutine(InitTestScene());
    }

    private void Start()
    {
        if (!testScene) StartCoroutine(InitializeAndLoad());
    }

    private IEnumerator InitializeAndLoad()
    {
        // 1) 싱글턴 인스턴스 강제 생성
        foreach (var mb in singletonManagers)
        {
            var type = mb.GetType();
            var instanceProp = type.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            instanceProp?.GetValue(null, null);
        }

        // 2) IInitializable 구현 매니저 초기화
        foreach (var mb in singletonManagers)
        {
            if (mb is IInitializable init)
            {
                yield return StartCoroutine(init.Initialize());
            }
        }

        // 3) 로딩 씬 비동기 로드
        var op = SceneManager.LoadSceneAsync(nextSceneID, LoadSceneMode.Single);
        op.allowSceneActivation = false;

        // 프로그레스 바가 있다면 업데이트
        while (op.progress < 0.9f)
        {
            if (loadingUI != null) loadingUI.SetProgress(op.progress);
            yield return null;
        }

        Debug.Log("<color=#1e90ff><b>[Bootstrapper]</b> 로딩 완료</color>");

        AnalyticsManager.SendFunnelStep(1);

        yield return new WaitUntil(() =>
            (
                Keyboard.current != null &&
                Keyboard.current.anyKey.wasPressedThisFrame) ||
            (
                Mouse.current != null && (
                Mouse.current.leftButton.wasPressedThisFrame ||
                Mouse.current.rightButton.wasPressedThisFrame ||
                Mouse.current.middleButton.wasPressedThisFrame ||
                Mouse.current.forwardButton.wasPressedThisFrame ||
                Mouse.current.backButton.wasPressedThisFrame
            ))
        );

        // 잠깐 대기 (페이드아웃 등)
        yield return new WaitForSecondsRealtime(0.25f);

        op.allowSceneActivation = true;
        yield return op;

        // 4) 부트스트랩 오브젝트 정리
        Destroy(gameObject);
    }

    private IEnumerator InitTestScene()
    {
        // 1) 싱글턴 인스턴스 강제 생성
        foreach (var mb in singletonManagers)
        {
            var type = mb.GetType();
            var instanceProp = type.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            instanceProp?.GetValue(null, null);
        }

        // 2) IInitializable 구현 매니저 초기화
        foreach (var mb in singletonManagers)
        {
            if (mb is IInitializable init)
            {
                yield return StartCoroutine(init.Initialize());
            }
        }
        
        Debug.Log("<color=#1e90ff><b>[Bootstrapper]</b> 로딩 완료</color>");

        yield return new WaitForSecondsRealtime(0.25f);

        Destroy(gameObject);
    }
}
