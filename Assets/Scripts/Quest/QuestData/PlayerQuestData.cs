using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어의 퀘스트 관련 상태(진행중, 완료)를 관리하는 컴포넌트.
/// 플레이어 GameObject에 부착됩니다.
/// </summary>
[System.Serializable] // 저장/로드를 위해 직렬화 가능하도록 설정
public class PlayerQuestData
{
    // 현재 진행 중인 퀘스트 목록 (QuestID(int) -> QuestStatus)
    public Dictionary<int, QuestStatus> activeQuests = new Dictionary<int, QuestStatus>();
    // 완료한 퀘스트 ID(int) 목록 (보상까지 받은 퀘스트)
    public HashSet<int> completedQuests = new HashSet<int>();
    // ★ 해금되었지만 아직 수락하지 않은 퀘스트 ID 목록 (선택적) ★
    public HashSet<int> unlockedQuests = new HashSet<int>();


    // --- 이벤트 발행 주체 변경 ---
    // 이벤트는 PlayerDataManager가 발행하도록 변경하고,
    // 이 클래스는 필요시 PlayerDataManager의 메서드를 호출하여 이벤트 발생을 요청합니다.
    // public static event Action<QuestData> OnQuestAccepted;
    // ...


    // --- 메서드들 (PlayerDataManager에서 호출됨) ---

    /// <summary> 퀘스트 수락 시도 </summary>
    public bool TryAcceptQuest(QuestData questData)
    {
        if (questData == null || activeQuests.ContainsKey(questData.questID) || completedQuests.Contains(questData.questID)) return false;
        if (!CheckPrerequisites(questData)) return false;
        // TODO: 레벨 등 다른 조건 체크

        QuestStatus newQuestStatus = new QuestStatus(questData);
        activeQuests.Add(questData.questID, newQuestStatus);
        newQuestStatus.ActivateListeners(); // 리스너 활성화는 여전히 필요
        // PlayerDataManager.Instance.RaiseQuestAcceptedEvent(questData); // 매니저에게 이벤트 발생 요청
        Debug.Log($"PlayerQuestData: Accepted Quest {questData.questID}");
        // 해금 목록에서 제거 (수락했으므로)
        unlockedQuests.Remove(questData.questID);
        return true;
    }

    /// <summary> 퀘스트 진행도 업데이트 (목표 SO 등에서 호출될 수 있음 - PlayerDataManager 경유 권장) </summary>
    public bool NotifyObjectiveProgress(QuestStatus status, QuestObjectiveDefinition objectiveDef, int amount)
    {
        if (status == null || objectiveDef == null || status.isCompleted || !activeQuests.ContainsKey(status.questID) || !activeQuests.ContainsValue(status)) // activeQuests에 questID로도 체크
        {
            // Debug.LogWarning($"[PlayerQuestData] NotifyObjectiveProgress: Invalid state or quest not active for QuestID {status?.questID}");
            return false;
        }

        // QuestStatus 내에서 해당 objectiveDef를 찾아 인덱스를 가져오는 로직이 필요합니다.
        // QuestStatus.questData.objectives 리스트와 objectiveDef를 비교해야 합니다.
        int objectiveIndex = -1;
        if (status.questData != null && status.questData.objectives != null)
        {
            for (int i = 0; i < status.questData.objectives.Count; i++)
            {
                // QuestObjectiveDefinition 끼리 비교 (참조 또는 고유 ID로)
                if (status.questData.objectives[i] == objectiveDef) // 참조 비교. 만약 ID 필드가 있다면 ID로 비교하는 것이 더 안전.
                {
                    objectiveIndex = i;
                    break;
                }
            }
        }

        if (objectiveIndex != -1)
        {
            bool progressChanged = status.UpdateProgress(objectiveIndex, amount); // QuestStatus의 UpdateProgress 호출
            if (progressChanged)
            {
                Debug.Log($"[PlayerQuestData] Progress Updated for Quest {status.questID}, Objective Index {objectiveIndex}. New Value: {status.objectiveProgress[objectiveIndex]}");
                // 이 특정 목표가 '방금' 완료되었는지 확인하여 반환
                if (status.IsObjectiveComplete(objectiveIndex))
                {
                    // 여기서 모든 목표 완료 여부를 확인하지 않고, 단일 목표 완료 여부만 반환
                    return true;
                }
            }
        }
        else
        {
            Debug.LogWarning($"[PlayerQuestData] ObjectiveDefinition not found in QuestStatus for QuestID {status.questID}. Objective: {objectiveDef.name}");
        }
        return false;
    }


    /// <summary> 퀘스트 완료 시도 </summary>
    public bool TryCompleteQuest(int questID)
    {
        if (activeQuests.TryGetValue(questID, out QuestStatus status))
        {
            if (status.AreAllObjectivesComplete())
            {
                status.isCompleted = true;
                status.DeactivateListeners();
                activeQuests.Remove(questID);

                if (!status.questData.isRepeatable)
                {
                    completedQuests.Add(questID);
                }

                // ★ 해금 퀘스트 처리 -> PlayerDataManager가 담당 ★
                // if (status.questData.unlockQuestIDs != null) { ... }

                // 보상 지급 로직 -> PlayerDataManager가 담당
                // GrantRewards(status.questData);

                // PlayerDataManager.Instance.RaiseQuestCompletedEvent(status.questData); // 매니저에게 이벤트 발생 요청
                Debug.Log($"PlayerQuestData: Completed Quest {questID}");
                return true;
            }
        }
        return false;
    }

    /// <summary> 퀘스트 포기 </summary>
    public void AbandonQuest(int questID)
    {
        if (activeQuests.TryGetValue(questID, out QuestStatus status))
        {
            status.DeactivateListeners();
            QuestData abandonedQuestData = status.questData;
            activeQuests.Remove(questID);
            // PlayerDataManager.Instance.RaiseQuestAbandonedEvent(abandonedQuestData); // 매니저에게 이벤트 발생 요청
            Debug.Log($"PlayerQuestData: Abandoned Quest {questID}");
        }
    }

    /// <summary> 선행 조건 확인 </summary>
    private bool CheckPrerequisites(QuestData questData)
    {
        if (questData.prerequisiteQuestIDs == null) return true;
        foreach (int preReqId in questData.prerequisiteQuestIDs)
        {
            if (!completedQuests.Contains(preReqId)) return false;
        }
        // ★ 해금 여부 확인 추가 (선택적) ★
        // if (!unlockedQuests.Contains(questData.questID) && !IsQuestInitiallyAvailable(questData)) return false; // 처음부터 가능한 퀘스트가 아니면 해금 목록 확인
        return true;
    }

    /// <summary> 특정 퀘스트가 선행/해금 조건 없이 처음부터 가능한지 확인 (예: 메인 퀘스트 1) </summary>
    private bool IsQuestInitiallyAvailable(QuestData questData)
    {
        // 예: 퀘스트 ID가 10000 미만이거나, 타입이 Main 이거나 등등 게임 규칙에 따라 정의
        return questData.questID < 10000;
    }


    /// <summary> 퀘스트 해금 처리 (외부에서 호출) </summary>
    public void UnlockQuest(int questID)
    {
        if (!activeQuests.ContainsKey(questID) && !completedQuests.Contains(questID))
        {
            unlockedQuests.Add(questID);
            Debug.Log($"PlayerQuestData: Quest {questID} unlocked.");
            // PlayerDataManager.Instance.RaiseQuestUnlockedEvent(questID); // 이벤트 발생 요청
        }
    }

    /// <summary> 저장/로드를 위한 준비 </summary>
    public void PrepareForSave()
    {
        // 활성 퀘스트 리스너 해제 (저장 전)
        foreach (var status in activeQuests.Values) status.DeactivateListeners();
    }
    public void RestoreAfterLoad(QuestDatabase db)
    {
        // 로드 후 QuestData 참조 복원 및 리스너 활성화
        foreach (var status in activeQuests.Values)
        {
            status.RestoreQuestDataReference(db);
            status.ActivateListeners();
        }
    }
    public bool AreAllObjectivesMetForQuest(int questID)
    {
        if (activeQuests.TryGetValue(questID, out QuestStatus status))
        {
            return status.AreAllObjectivesComplete();
        }
        return false;
    }

    // 퀘스트 상태 변경 함수 (PlayerQuestDataManager에서 사용)
    public void SetQuestProgressState(int questID, QuestProgressState newState)
    {
        if (activeQuests.TryGetValue(questID, out QuestStatus status))
        {
            status.ProgressState = newState; // QuestStatus에 ProgressState 프로퍼티 추가 필요
            Debug.Log($"[PlayerQuestData] Quest {questID} ProgressState set to {newState}");
        }
    }
}
