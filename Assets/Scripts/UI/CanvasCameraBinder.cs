using System.Collections;
using UnityEngine;

/// <summary>
/// UIManager의 캔버스에 이 스크립트가 부착된 카메라를 자동 등록
/// </summary>
[RequireComponent(typeof(Camera))]
public class CanvasCameraBinder : MonoBehaviour
{
    public bool bindGameManagerCanvas;
    public bool bindSceneManagerCanvas;

    private void Awake()
    { 
        if (bindGameManagerCanvas) StartCoroutine(BindUIManagerCanvas());
        if (bindSceneManagerCanvas) StartCoroutine(BindGameManagerCanvas());
    }

    private IEnumerator BindUIManagerCanvas()
    {
        yield return new WaitUntil(() => UIManager.Instance != null);

        var canvas = UIManager.Instance.Canvas;
        var prevCam = canvas.worldCamera;

        if (canvas.renderMode is RenderMode.ScreenSpaceOverlay)
            canvas.renderMode = RenderMode.ScreenSpaceCamera;

        canvas.worldCamera = GetComponent<Camera>();
        prevCam = canvas.worldCamera;

        Debug.Log($"[CanvasCameraBinder] 카메라 바인드!! ({prevCam} -> {GetComponent<Camera>()})");
    }

    private IEnumerator BindGameManagerCanvas()
    {
        yield return new WaitUntil(() => GameManager.Instance != null);

        var canvas = GameManager.Instance.fader.GetComponent<Canvas>();

        if (canvas.renderMode is RenderMode.ScreenSpaceOverlay)
            canvas.renderMode = RenderMode.ScreenSpaceCamera;

        canvas.worldCamera = GetComponent<Camera>();
    }
}
