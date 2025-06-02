using System.Collections;
using UnityEngine;

public class BlipAutoRegister : MonoBehaviour
{
    [SerializeField] private BlipType blipType = BlipType.Objective;
    private MinimapSystem minimap;

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => GameManager.Instance.MinimapSystem != null);

        minimap = GameManager.Instance.MinimapSystem;
        if (minimap != null)
        {
            var settings = minimap.GetBlipSettings(blipType);
            if (settings != null) minimap.RegisterBlip(transform, settings);
        }
    }

    private void OnDestroy()
    {
        if (minimap != null) minimap.UnregisterBlip(transform);
    }
}
