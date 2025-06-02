using UnityEngine;

[CreateAssetMenu(fileName = "DeliverItemObjective", menuName = "Quest System/Objectives/Deliver Item Objective")]
public class DeliverItemObjectiveDefinition : QuestObjectiveDefinition
{
    [Tooltip("전달해야 할 아이템의 고유 숫자 ID")]
    public int requiredItemID; 

    // requiredCount는 QuestObjectiveDefinition의 것을 사용 (전달할 아이템 개수)

    [Tooltip("아이템을 전달받을 NPC의 식별자 (string 또는 int)")]
    public int targetNPCID; // QuestData의 Giver/Completer ID와 같은 형식 사용 권장

    /// <summary>
    /// 아이템 전달 목표는 특정 게임 이벤트 리스닝 대신,
    /// NPC 상호작용 시점에 직접 확인하는 방식이 더 적합할 수 있음
    /// 따라서 이 함수들은 비워두거나, 별도의 커스텀 이벤트 시스템과 연동할 수 있음
    /// </summary>
    public override void SetupListener(QuestStatus questStatus)
    {
        // Debug.Log($"DeliverItemObjective '{this.name}' for quest '{questStatus?.questData?.questName}' does not require active event listening.");
    }

    public override void RemoveListener(QuestStatus questStatus)
    {
        // 특별히 해제할 리스너 없음
    }

    /// <summary>
    /// 이 목표의 완료 여부를 확인합니다. (단순 진행도 체크)
    /// 실제 아이템 전달 가능 여부는 NPC 상호작용 시 체크합니다.
    /// </summary>
    public override bool IsComplete(QuestStatus questStatus, int objectiveIndex)
    {
        // 이 목표는 dataManager 참조가 실제로는 필요 없음 (진행도만 체크)
        if (objectiveIndex < 0 || objectiveIndex >= questStatus.objectiveProgress.Count) return false;
        return questStatus.objectiveProgress[objectiveIndex] >= 1;
    }

    /// <summary>
    /// 진행 상황 텍스트를 반환합니다.
    /// </summary>
    public override string GetProgressDescription(QuestStatus questStatus, int objectiveIndex)
    {
        if (objectiveIndex < 0 || objectiveIndex >= questStatus.objectiveProgress.Count) return "Error";

        // TODO: requiredItemID로 실제 아이템 이름 찾아오기
        string itemName = $"아이템 ID [{requiredItemID}]";
        // TODO: targetNPCID로 실제 NPC 이름 찾아오기
        string npcName = $"NPC [{targetNPCID}]";

        int currentProgress = questStatus.objectiveProgress[objectiveIndex]; // 0 또는 1
        string statusText = currentProgress >= 1 ? "(전달 완료)" : "(미완료)";

        // requiredCount는 전달할 아이템 '개수'를 의미하도록 유지
        return $"{description} ({itemName} {requiredCount}개) 에게 ({npcName}) {statusText}";
    }

    /// <summary>
    /// ★★★ 이 목표를 완료 처리하는 로직 (NPC가 호출) ★★★
    /// 플레이어가 아이템을 가지고 있는지 확인하고, 있다면 소모 후 진행도를 업데이트
    /// </summary>
    /// <returns>아이템 전달 및 목표 업데이트 성공 여부</returns>
    public bool TryCompleteObjective(QuestStatus status)
    {
        if (QuestManager.Instance.PlayerQuestDataManager == null || status == null || status.questData == null) return false;

        int objectiveIndex = status.questData.objectives.IndexOf(this);
        // ★ IsComplete 호출 시 playerQuest 인자 제거 ★
        if (objectiveIndex < 0 || IsComplete(status, objectiveIndex))
        {
            return false; // 이미 완료 또는 유효하지 않음
        }

        // TODO: ★★★ 실제 인벤토리 시스템 연동 필요 ★★★
        // 인벤토리에서 requiredItemID 아이템을 requiredCount만큼 가지고 있는지 확인합니다.
        // bool hasEnoughItems = InventoryManager.Instance.HasItem(requiredItemID, requiredCount);
        bool hasEnoughItems = true; // << 임시로 항상 true로 설정 (테스트용)

        if (hasEnoughItems)
        {
            // TODO: ★★★ 실제 인벤토리 시스템 연동 필요 ★★★
            // 인벤토리에서 requiredItemID 아이템을 requiredCount만큼 제거합니다.
            // bool consumed = InventoryManager.Instance.RemoveItem(requiredItemID, requiredCount);
            bool consumed = true; // << 임시로 항상 성공으로 설정

            if (consumed)
            {
                // PlayerQuest를 통해 진행도 업데이트 (1 증가시켜 완료 상태로 만듦)
                QuestManager.Instance.PlayerQuestDataManager.NotifyObjectiveProgress(status, this, 1);
                Debug.Log($"Quest '{status.questData.questName}': Delivered item '{requiredItemID}' x {requiredCount}. Objective complete.");
                return true; // 성공
            }
            else
            {
                Debug.LogWarning($"Quest '{status.questData.questName}': Failed to consume item '{requiredItemID}' from inventory.");
                return false; // 아이템 소모 실패
            }
        }
        else
        {
            Debug.Log($"Quest '{status.questData.questName}': Player does not have enough item '{requiredItemID}' ({requiredCount} needed).");
            // TODO: UI에 아이템 부족 메시지 표시 등
            return false; // 아이템 부족
        }
    }

    
}
