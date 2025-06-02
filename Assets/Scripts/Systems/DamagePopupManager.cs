using System.Collections.Generic;
using UnityEngine;
using ProjectRaid.EditorTools;

public class DamagePopupManager : MonoBehaviour
{
    [FoldoutGroup("Pool", ExtendedColor.Cyan)]
    [SerializeField] private DamagePopup popupPrefab;
    [SerializeField] private Transform parent;
    [SerializeField] private int poolSize = 20;

    private readonly Queue<DamagePopup> pool = new();

    private void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            var popup = Instantiate(popupPrefab, parent != null ? parent : transform);
            popup.gameObject.SetActive(false);
            pool.Enqueue(popup);
        }
    }

    public void ShowPopup(float damage, bool isCritical, Vector3 position)
    {
        DamagePopup popup = pool.Count > 0 ? pool.Dequeue() : Instantiate(popupPrefab, transform);
        popup.gameObject.SetActive(true);
        popup.Show(damage, isCritical, position);
    }

    public void ReturnToPool(DamagePopup popup)
    {
        popup.gameObject.SetActive(false);
        pool.Enqueue(popup);
    }
}
