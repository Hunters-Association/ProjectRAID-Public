using System;
using UnityEngine;

public class PlayerQuestDataManager : MonoBehaviour
{
    // 플레이어의 퀘스트 데이터 인스턴스
    public PlayerQuestData QuestData { get; private set; }
    public int TrackedQuestID { get; private set; } = 0;

    // 참조할 퀘스트 데이터베이스
    private QuestDatabase questDatabase; // Inspector에서 할당
    public QuestDatabase QuestDatabase => questDatabase;

    // --- 퀘스트 관련 이벤트 (이제 여기서 발행) ---
    public event Action<QuestData> OnQuestAccepted;
    public event Action<QuestData> OnQuestCompleted;
    public event Action<QuestStatus> OnQuestProgressUpdated;
    public event Action<QuestData> OnQuestAbandoned;
    public event Action<int> OnQuestUnlocked;
    public event Action<int> OnTrackedQuestChanged;
    public event System.Action<QuestData> OnQuestObjectivesMet;

    /// <summary> 플레이어 데이터 초기화 (새 게임 또는 로드) </summary>
    public void InitializePlayerData(QuestDatabase database)
    {
        questDatabase = database;

        // TODO: 여기서 실제 게임 데이터 로드 로직 호출
        // if (HasSaveData()) LoadGameData();
        // else CreateNewPlayerData();

        // 임시: 새 데이터 생성
        CreateNewPlayerData();
    }

    /// <summary> 새 플레이어 데이터 생성 </summary>
    private void CreateNewPlayerData()
    {
        QuestData = new PlayerQuestData();
        // Debug.Log("[PlayerQuestDataManager] New PlayerQuestData created.");
    }

    // --- 퀘스트 상호작용 메서드 (외부에서 호출) ---


    public bool AcceptQuest(QuestData questData)
    {
        if (QuestData == null) return false;
        bool success = QuestData.TryAcceptQuest(questData);
        if (success)
        {
            Debug.Log("이벤트 발행");
            OnQuestAccepted?.Invoke(questData);

            // 임시 코드 from JW
            SetTrackedQuest(questData.questID);
            UIManager.Instance.ShowUI<QuestTrackerUI>();
        } // 이벤트 발행
        return success;
    }

    public void NotifyObjectiveProgress(QuestStatus statusOfUpdatedObjective, QuestObjectiveDefinition objectiveDef, int amount) // 파라미터 이름 명확화
    {
        if (QuestData == null || statusOfUpdatedObjective == null || objectiveDef == null)
        {
            Debug.LogError("[PlayerQuestDataManager] NotifyObjectiveProgress: Null argument(s).");
            return;
        }

        // PlayerQuestData에 진행도 업데이트 요청 및 해당 특정 목표가 '방금' 완료되었는지 확인
        bool specificObjectiveWasJustCompleted = QuestData.NotifyObjectiveProgress(statusOfUpdatedObjective, objectiveDef, amount);

        // 진행도 업데이트 이벤트는 항상 발생 (UI 등에서 현재 상태를 보여주기 위함)
        OnQuestProgressUpdated?.Invoke(statusOfUpdatedObjective);

        // 특정 목표가 방금 완료되었거나, 또는 이전에 이미 다른 목표들이 완료되어 있었을 수 있으므로,
        // 항상 해당 퀘스트의 '모든' 목표가 달성되었는지 다시 확인합니다.
        if (QuestData.activeQuests.TryGetValue(statusOfUpdatedObjective.questID, out QuestStatus currentQuestFullStatus))
        {
            // QuestData (정의)를 가져옵니다. OnQuestObjectivesMet 이벤트에 전달하기 위함입니다.
            QuestData questDefinition = questDatabase.GetQuestByID(currentQuestFullStatus.questID);
            if (questDefinition == null)
            {
                Debug.LogError($"[PlayerQuestDataManager] NotifyObjectiveProgress: QuestDefinition not found for ID {currentQuestFullStatus.questID}");
                return;
            }

            // 해당 퀘스트의 모든 목표가 현재 달성된 상태인지 확인합니다.
            // 그리고 아직 "ObjectivesMet_AwaitingReport" 상태로 변경되지 않았는지 확인하여 중복 이벤트 발생 방지.
            if (currentQuestFullStatus.AreAllObjectivesComplete() && currentQuestFullStatus.ProgressState != QuestProgressState.ObjectivesMet_AwaitingReport)
            {
                // 퀘스트 상태를 "달성됨 (보고 대기)"로 변경
                QuestData.SetQuestProgressState(currentQuestFullStatus.questID, QuestProgressState.ObjectivesMet_AwaitingReport);
                // 또는 currentQuestFullStatus.ProgressState = QuestProgressState.ObjectivesMet_AwaitingReport; 직접 변경

                Debug.Log($"[PlayerQuestDataManager] 퀘스트 [{questDefinition.questName}] (ID: {questDefinition.questID}) 모든 목표 달성! OnQuestObjectivesMet 이벤트 발생.");
                OnQuestObjectivesMet?.Invoke(questDefinition); // QuestData (정의)를 이벤트로 전달
            }
            // else if (specificObjectiveWasJustCompleted) // 특정 목표만 완료되었고, 전체는 아닐 때의 로그 (선택적)
            // {
            //     Debug.Log($"[PlayerQuestDataManager] 퀘스트 [{questDefinition.questName}]의 특정 목표 [{objectiveDef.name}] 완료. 아직 모든 목표 달성 전.");
            // }
        }
        else
        {
            Debug.LogError($"[PlayerQuestDataManager] NotifyObjectiveProgress: QuestID {statusOfUpdatedObjective.questID} not found in active quests after progress update.");
        }
    }


    public bool CompleteQuest(int questID)
    {
        if (QuestData == null || questDatabase == null) return false;

        // 완료 시도 전에 해당 퀘스트 데이터 가져오기 (이벤트 전달 및 해금 처리용)
        QuestData completedQuestInfo = questDatabase.GetQuestByID(questID);
        if (completedQuestInfo == null)
        {
            Debug.LogError($"Cannot complete quest: QuestData not found for ID {questID}");
            return false;
        }

        bool success = QuestData.TryCompleteQuest(questID);
        if (success)
        {
            if (TrackedQuestID == questID) SetTrackedQuest(0);
            GrantRewards(completedQuestInfo); // 보상 지급
            UnlockQuests(completedQuestInfo); // 퀘스트 해금
            OnQuestCompleted?.Invoke(completedQuestInfo); // 완료 이벤트 발행
        }
        return success;
    }

    public void AbandonQuest(int questID)
    {
        if (QuestData == null || questDatabase == null) return;
        QuestData abandonedQuestData = questDatabase.GetQuestByID(questID); // 데이터 먼저 찾기
        if (TrackedQuestID == questID) SetTrackedQuest(0);
        QuestData.AbandonQuest(questID);
        if (abandonedQuestData != null) OnQuestAbandoned?.Invoke(abandonedQuestData); // 이벤트 발행
    }

    /// <summary> 퀘스트 보상 지급 </summary>
    private void GrantRewards(QuestData questData)
    {
        Debug.Log($"<b>[PlayerQuestDataManager]'{questData.questName}'</b> 퀘스트를 완료해 보상을 획득합니다!");
        if (questData.rewards != null)
        {
            foreach (var rewardDef in questData.rewards)
            {
                // rewardDef?.GrantReward(PlayerQuest.Instance); // PlayerQuest 대신 Player 인스턴스 필요
                // TODO: 보상 지급 로직 구현 (인벤토리, 스탯 시스템 등 호출)

                if (rewardDef is ItemRewardDefinition itemReward)
                {
                    Debug.Log($"<b>[PlayerQuestDataManager]</b> 획득한 보상: <b>'{itemReward.name}'</b> x{itemReward.amount}");
                    GameManager.Instance.Inventory.TryAdd(itemReward.itemID, itemReward.amount);
                }
            }
        }
    }

    /// <summary> 퀘스트 해금 처리 </summary>
    private void UnlockQuests(QuestData questData)
    {
        if (questData.unlockQuestIDs != null)
        {
            foreach (int unlockedQuestID in questData.unlockQuestIDs)
            {
                QuestData.UnlockQuest(unlockedQuestID); // PlayerQuestData의 해금 메서드 호출
                OnQuestUnlocked?.Invoke(unlockedQuestID); // 해금 이벤트 발행
            }
        }
    }
    public void SetTrackedQuest(int questID)
    {
        // 유효한 활성 퀘스트인지 확인 (선택적)
        if (QuestData != null && QuestData.activeQuests.ContainsKey(questID))
        {
            if (TrackedQuestID != questID) // 변경될 경우에만
            {
                TrackedQuestID = questID;
                Debug.Log($"New tracked quest set: {questID}");
                OnTrackedQuestChanged?.Invoke(TrackedQuestID); // 추적 변경 이벤트 발생
            }
        }
        else if (questID == 0)
        { // 추적 해제
            if (TrackedQuestID != 0)
            {
                TrackedQuestID = 0;
                Debug.Log("Quest tracking disabled.");
                OnTrackedQuestChanged?.Invoke(TrackedQuestID);
            }
        }
        else { Debug.LogWarning($"Cannot track quest {questID}. Not an active quest."); }
    }

    
    

    // --- 저장/로드 함수 ---
    public void SavePlayerData()
    {
        if (QuestData == null) return;
        QuestData.PrepareForSave(); // 리스너 해제 등 저장 준비
        // TODO: QuestData 객체를 직렬화하여 파일 또는 DB에 저장
        Debug.Log("Saving Player Data... (Implementation Needed)");
        // 예: string json = JsonUtility.ToJson(QuestData); File.WriteAllText(savePath, json);
    }

    public void LoadPlayerData()
    {
        if (questDatabase == null) { Debug.LogError("QuestDatabase needed for loading!"); return; }
        // TODO: 파일 또는 DB에서 데이터를 읽어와 PlayerQuestData 객체로 역직렬화
        Debug.Log("Loading Player Data... (Implementation Needed)");
        // 예: string json = File.ReadAllText(savePath); QuestData = JsonUtility.FromJson<PlayerQuestData>(json);
        // if (QuestData == null) QuestData = new PlayerQuestData(); // 로드 실패 시 새로 생성

        QuestData.RestoreAfterLoad(questDatabase); // 로드 후 참조 복원 및 리스너 활성화
    }
}
