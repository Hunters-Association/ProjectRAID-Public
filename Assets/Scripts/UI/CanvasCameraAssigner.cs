using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// World Space Canvas에 MainCamera를 자동 등록
/// </summary>
[RequireComponent(typeof(Canvas))]
public class CanvasCameraAssigner : MonoBehaviour
{
    private Canvas canvas;

    private void Start()
    {
        canvas = GetComponent<Canvas>();
        
        AssignCamera();
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    public void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AssignCamera();
    }

    public void AssignCamera()
    {
        if (canvas == null) return;

        if (canvas.renderMode is RenderMode.ScreenSpaceOverlay)
            canvas.renderMode = RenderMode.ScreenSpaceCamera;

        canvas.worldCamera = Camera.main;
    }
}
