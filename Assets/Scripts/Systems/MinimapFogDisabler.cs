using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Play 모드에서만 동작
/// beginCameraRendering 직전에 Fog 변경, endCameraRendering·PlayMode 종료 시 원복
/// </summary>
[DisallowMultipleComponent]
public class MinimapFogDisabler : MonoBehaviour
{
    [SerializeField] Camera targetCamera;
    [SerializeField] bool disableFog = true;
    [SerializeField] float densityMultiplier = 0.5f;

    bool cached;
    bool origEnabled;
    float origDensity;
    Color origColor;

    void Awake()
    {
        if (!targetCamera) targetCamera = GetComponent<Camera>();
    }

    private void OnEnable()
    {
        if (!Application.isPlaying) return;

        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering += OnEndCameraRendering;

#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += OnPlaymodeChange;
#endif
    }

    private void OnDisable()
    {
        if (!Application.isPlaying) return;

        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
        Restore();

#if UNITY_EDITOR
        EditorApplication.playModeStateChanged -= OnPlaymodeChange;
#endif
    }

#if UNITY_EDITOR
    void OnPlaymodeChange(PlayModeStateChange s)
    {
        if (s == PlayModeStateChange.ExitingPlayMode) Restore();
    }
#endif

    private void OnBeginCameraRendering(ScriptableRenderContext _, Camera cam)
    {
        if (cam != targetCamera) return;

        if (!cached)
        {
            origEnabled = RenderSettings.fog;
            origDensity = RenderSettings.fogDensity;
            origColor   = RenderSettings.fogColor;
            cached      = true;
        }

        if (disableFog)
        {
            RenderSettings.fog = false;
        }
        else
        {
            RenderSettings.fog = true;
            RenderSettings.fogDensity = origDensity * densityMultiplier;
        }
    }

    private void OnEndCameraRendering(ScriptableRenderContext _, Camera cam)
    {
        if (cam == targetCamera) Restore();
    }

    void Restore()
    {
        if (!cached) return;
        RenderSettings.fog          = origEnabled;
        RenderSettings.fogDensity   = origDensity;
        RenderSettings.fogColor     = origColor;
        cached = false;
    }
}
