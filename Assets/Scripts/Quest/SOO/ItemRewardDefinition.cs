using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemReward", menuName = "Quest System/Rewards/Item Reward")]
public class ItemRewardDefinition : QuestRewardDefinition
{
    [Tooltip("지급할 아이템의 고유 ID")]
    public int itemID;
    // public ItemData itemData; // Item SO 직접 연결 방식도 가능

    [Tooltip("지급할 아이템의 수량")]
    public int amount = 1;

    /// <summary>
    /// 인벤토리 시스템을 통해 아이템을 지급합니다.
    /// </summary>
    public override void GrantReward(PlayerQuestDataManager playerQuest)
    {
        Debug.Log($"Granting Item Reward: {itemID} x {amount} to {playerQuest.gameObject.name}");
        // TODO: 실제 인벤토리 시스템 연동
        // InventoryManager.Instance.AddItem(itemID, amount);
    }

    /// <summary>
    /// 보상 설명 텍스트 생성
    /// </summary>
    public override string GetDescription()
    {
        // TODO: itemID로 실제 아이템 이름 찾아오기 (선택적)
        string itemName = $"아이템 [{itemID}]";
        return $"{itemName} x{amount}";
    }
}
