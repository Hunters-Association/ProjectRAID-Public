using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class BaseScanable : MonoBehaviour
{
    [SerializeField] private uint defaultRenderingLayerMask;
    [SerializeField] private uint outlineRenderingLayerMask;
    [SerializeField] private List<Renderer> renderers;

    private void Awake()
    {
        Init();
    }

    public virtual void Init()
    {
        SetDefaultLayerMask();
    }

    public virtual void ActiveOutLine()
    {
        SetOutLineLayerMask();
    }

    public virtual void CancleOutLine()
    {
        SetDefaultLayerMask();
    }

    public void SetDefaultLayerMask()
    {
        for (int i = 0; i < renderers.Count; i++)
        {
            renderers[i].renderingLayerMask = defaultRenderingLayerMask;
        }
    }

    public void SetOutLineLayerMask()
    {
        for (int i = 0; i < renderers.Count; i++)
        {
            renderers[i].renderingLayerMask = outlineRenderingLayerMask;
        }
    }
}
