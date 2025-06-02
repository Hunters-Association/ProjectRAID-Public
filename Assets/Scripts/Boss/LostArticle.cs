using ProjectRaid.Data;
using UnityEngine;

public class LostArticle : MonoBehaviour, IInteractable
{
    public ItemData itemData;

    public void Interact(PlayerController player)
    {
        // 아이템 데이터를 플레이어에게 넘겨주자
        Debug.Log($"{itemData.name}을 획득하였습니다");
        GameManager.Instance.Inventory.TryAdd(itemData);

        Destroy(gameObject);
    }

    public void HideHighlight() { }
    public void ShowHighlight() { }


    [SerializeField] private InteractableData data;

    public InteractableData GetInteractableData()
    {
        if (data == null) Debug.LogWarning("[LostArticle] 상호작용 데이터가 등록되지 않았습니다.");
        return data;
    }
}
