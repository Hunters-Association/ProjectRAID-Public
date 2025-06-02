using System.Collections.Generic;
using UnityEngine;

public class BaseInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] protected InteractableData interactableData;
    [SerializeField] protected uint defaultRenderingLayerMask;
    [SerializeField] protected uint outlineRenderingLayerMask;
    [SerializeField] protected List<Renderer> renderers;

    private void Start()
    {
        foreach (var renderer in renderers)
            renderer.renderingLayerMask = defaultRenderingLayerMask;
    }

    public virtual void Interact(PlayerController player) { }

    public virtual void ShowHighlight()
    {
        foreach (var renderer in renderers)
            renderer.renderingLayerMask = outlineRenderingLayerMask;
    }

    public virtual void HideHighlight()
    {
        foreach (var renderer in renderers)
            renderer.renderingLayerMask = defaultRenderingLayerMask;
    }

    public virtual InteractableData GetInteractableData() => interactableData;
}
