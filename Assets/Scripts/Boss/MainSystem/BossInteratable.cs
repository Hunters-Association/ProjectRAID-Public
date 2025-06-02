using UnityEngine;

public enum BossInteractableType
{
    Body,       // 보스 몸통
    Cut,        // 절단된 부위
}

public class BossInteratable : MonoBehaviour, IInteractable
{
    public BossInteractableType type;

    // 몬스터의 드롭 테이블
    public BossDropTable dropTable;

    // 최대 갈무리 가능 횟수
    private int maxCaptureCount;

    private void OnEnable()
    {
        SetCaptureCount();
    }

    private void SetCaptureCount()
    {
        maxCaptureCount = type switch
        {
            BossInteractableType.Body => 3,
            BossInteractableType.Cut => 1,
            _ => 3,
        };
    }

    public void Interact(PlayerController player)
    {
        // 갈무리 횟수가 다 되었다면 못하게 막음
        if (maxCaptureCount <= 0) return;

        int dropItemID = -1;

        if (dropTable != null)
            dropItemID = dropTable.GetDropItemID(dropTable.GetTable(type));

        if (dropItemID == -1) Debug.LogAssertion($"{gameObject.name}에 드롭 될 아이템이 정해지지 않았습니다.");

        if (dropItemID != -1)
        {
            // 아이템 데이터베이스에서 아이템 불러오기
            if (!GameManager.Instance.Inventory.TryAdd(dropItemID)) return;
                
            maxCaptureCount--;

            if (maxCaptureCount == 0)
            {
                if (type == BossInteractableType.Body)
                    gameObject.SetActive(false);
                else
                    Destroy(gameObject);
            }
        }
    }

    public void ShowHighlight() { }
    public void HideHighlight() { }


    [SerializeField] private InteractableData data;

    public InteractableData GetInteractableData()
    {
        if (data == null) Debug.LogWarning("[BossInteractable] 상호작용 데이터가 등록되지 않았습니다.");
        return data;
    }
}
