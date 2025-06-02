using System.Collections;
using UnityEngine;

public class QuestManager : MonoSingleton<QuestManager>, IInitializable
{
    [Tooltip("퀘스트 데이터베이스 ScriptableObject 참조")]
    [SerializeField] private QuestDatabase QuestDatabase; // Inspector에서 할당!
    [SerializeField] private PlayerQuestDataManager playerQuestDataManager;

    public QuestDatabase Database => QuestDatabase;
    public PlayerQuestDataManager PlayerQuestDataManager => playerQuestDataManager;

    public IEnumerator Initialize()
    {
        // 데이터베이스 초기화 (여기서 하거나, 별도의 초기화 매니저에서 호출)
        if (QuestDatabase != null)
        {
            // Debug.Log("Initializing Quest Database from QuestManager Awake..."); // ★ 확인용 로그 추가 ★
            QuestDatabase.InitializeDatabase();
        }
        else
        {
            Debug.LogError("[QuestManager] QuestDatabase가 등록되지 않았습니다!");
        }

        if (playerQuestDataManager != null)
        {
            playerQuestDataManager.InitializePlayerData(QuestDatabase);
        }
        else
        {
            Debug.LogError("[QuestManager] PlayerQuestDataManager가 등록되지 않았습니다!");
        }

        yield break;
    }

    // --- 다른 시스템(UI, NPC 등)에서 호출할 함수들 ---

    /// <summary>
    /// 플레이어가 특정 퀘스트를 수락하도록 요청합니다.
    /// </summary>
    /// <returns>수락 성공 여부</returns>
    public bool AcceptQuest(int questID)
    {
        // QuestData를 먼저 찾음
        QuestData data = QuestDatabase.GetQuestByID(questID);
        if (data == null)
        {           
            Debug.LogError($"[QuestManager] AcceptQuest 실패: QuestData not found for ID {questID}");
            return false;
        }
        AnalyticsManager.GetQuest(questID, Time.time);
        switch (questID)
        {
            case 1:
                AnalyticsManager.SendFunnelStep(4); // 람쥐 썬더 수주
                break;

            case 2:
                AnalyticsManager.SendFunnelStep(11); // 라비에스 수주
                break;

            case 3:
                AnalyticsManager.SendFunnelStep(22); // 발카리온 수주
                break;

            case 4:
                AnalyticsManager.SendFunnelStep(27); // MVP 종료
                break;

            default:
                break;
        }
        // PlayerDataManager의 AcceptQuest 호출 (QuestData 전달)
        return playerQuestDataManager.AcceptQuest(data);
    }

    /// <summary>
    /// 플레이어가 특정 퀘스트를 완료하도록 요청합니다.
    /// </summary>
    /// <returns>완료 성공 여부</returns>
    public bool CompleteQuest(int questID)
    {
        AnalyticsManager.EndQuest(questID, Time.time);
        return playerQuestDataManager.CompleteQuest(questID);
    }

    /// <summary>
    /// 플레이어가 특정 퀘스트를 포기하도록 요청합니다.
    /// </summary>
    public void AbandonQuest(int questID)
    {
        playerQuestDataManager.AbandonQuest(questID);
    }

    /// <summary>
    /// 특정 퀘스트가 현재 플레이어가 수락 가능한 상태인지 확인합니다.
    /// </summary>
    public bool IsQuestAvailable(int questID)
    {
        QuestData data = QuestDatabase.GetQuestByID(questID);
        // PlayerDataManager 인스턴스 및 QuestData 객체 확인
        if (data == null || playerQuestDataManager.QuestData == null) return false;

        // ★★★ PlayerDataManager의 QuestData 참조 ★★★
        var playerQuestData = playerQuestDataManager.QuestData;

        // 이미 진행 중이거나 완료했는지 확인
        if (playerQuestData.activeQuests.ContainsKey(questID) || playerQuestData.completedQuests.Contains(questID))
            return false;

        // 선행 퀘스트 조건 확인
        if (data.prerequisiteQuestIDs != null) // null 체크 추가
        {
            foreach (int preReqId in data.prerequisiteQuestIDs)
            {
                if (!playerQuestData.completedQuests.Contains(preReqId)) return false;
            }
        }

        // TODO: PlayerDataManager를 통해 플레이어 레벨 가져와 확인
        // if (PlayerDataManager.Instance.GetPlayerLevel() < data.requiredLevel) return false;

        // TODO: PlayerDataManager를 통해 퀘스트 해금 상태 확인 (선택적)
        // if (!playerQuestData.unlockedQuests.Contains(questID) && !IsQuestInitiallyAvailable(data)) return false;

        return true; // 모든 조건을 만족하면 수락 가능
    }

    /// <summary>
    /// 특정 퀘스트의 모든 목표가 완료되어 완료 보고가 가능한 상태인지 확인합니다.
    /// </summary>
    public bool IsQuestReadyToComplete(int questID)
    {
        // PlayerDataManager 인스턴스 및 QuestData 객체 확인
        if (playerQuestDataManager.QuestData == null) return false;

        // ★★★ PlayerDataManager의 QuestData 참조 ★★★
        var playerQuestData = playerQuestDataManager.QuestData;

        // 활성 퀘스트 목록에서 찾아 모든 목표 완료 여부 확인
        return playerQuestData.activeQuests.TryGetValue(questID, out var status) && status.AreAllObjectivesComplete();
    }
}
