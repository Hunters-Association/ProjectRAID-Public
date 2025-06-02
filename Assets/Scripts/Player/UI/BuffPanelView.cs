using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuffPanelView : MonoBehaviour
{
    [SerializeField] private Image iconPrefab;
    [SerializeField] private bool showTimer = true;

    private readonly Dictionary<BuffInstance, Image> map = new();

    public void AddIcon(BuffInstance inst)
    {
        if (iconPrefab == null) return;

        var img = Instantiate(iconPrefab, transform);
        img.sprite = inst.Data.icon;
        img.color = inst.Data.iconTint;
        map.Add(inst, img);
        if (showTimer) StartCoroutine(Flash(img, inst.Data.duration));
    }
    
    public void RemoveIcon(BuffInstance inst)
    {
        if (iconPrefab == null) return;

        if (map.TryGetValue(inst, out var img))
        {
            Destroy(img.gameObject);
            map.Remove(inst);
        }
    }

    private IEnumerator Flash(Image img, float sec)
    {
        float elapsed = 0f;
        while (elapsed < sec)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // 마지막 1초 동안 깜빡임
        float flash = 0f;
        while (flash < 1f)
        {
            flash += Time.deltaTime * 4f;
            img.enabled = !img.enabled;
            yield return new WaitForSeconds(0.25f);
        }
        img.enabled = true;
    }
}
