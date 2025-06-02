using UnityEngine;

[CreateAssetMenu(fileName = "KillObjective", menuName = "Quest System/Objectives/Kill Objective")]
public class KillMonsterObjectiveDefinition : QuestObjectiveDefinition
{
    [Tooltip("처치해야 할 몬스터의 고유 ID")]
    public int targetMonsterID;

    [Tooltip("몬스터 처치 시 발행되는 이벤트 (GameEventString 타입 SO 연결)")]
    public GameEventInt monsterKilledEvent; // Inspector에서 연결!

    // 이벤트 리스너가 참조할 QuestStatus (리스너 해제를 위해 저장)
    private QuestStatus listenerTargetStatus;

    /// <summary>
    /// MonsterKilledEvent 구독 설정
    /// </summary>
    public override void SetupListener(QuestStatus questStatus)
    {
        listenerTargetStatus = questStatus;
        monsterKilledEvent?.RegisterListener(OnMonsterKilled);
        // Debug.Log($"Listener Setup: Quest '{questStatus?.questData?.questName}', Objective: Kill {targetMonsterID}");
    }

    /// <summary>
    /// MonsterKilledEvent 구독 해제
    /// </summary>
    public override void RemoveListener(QuestStatus questStatus)
    {
        monsterKilledEvent?.UnregisterListener(OnMonsterKilled);
        // Debug.Log($"Listener Removed: Quest '{questStatus?.questData?.questName}', Objective: Kill {targetMonsterID}");
        listenerTargetStatus = null; // 참조 해제
    }

    /// <summary>
    /// MonsterKilledEvent가 발생했을 때 호출될 콜백 메서드
    /// </summary>
    private void OnMonsterKilled(int killedMonsterID)
    {
        // 리스너가 활성 상태이고, 전달된 ID가 목표 ID와 같으면 PlayerQuest에 알림
        if (listenerTargetStatus != null && killedMonsterID == targetMonsterID)
        {
            // Debug.Log($"Event Rx: Kill {killedMonsterID} for Quest '{listenerTargetStatus?.questData?.questName}'. Notifying PlayerQuest.");
            QuestManager.Instance.PlayerQuestDataManager.NotifyObjectiveProgress(listenerTargetStatus, this, 1); // 진행도 1 증가 알림
        }
    }

    /// <summary>
    /// 진행도가 목표치 이상인지 확인하여 완료 여부 반환
    /// </summary>
    public override bool IsComplete(QuestStatus questStatus, int objectiveIndex)
    {
        // 이 목표는 dataManager 참조가 실제로는 필요 없음
        if (objectiveIndex < 0 || objectiveIndex >= questStatus.objectiveProgress.Count) return false;
        return questStatus.objectiveProgress[objectiveIndex] >= requiredCount;
    }

    /// <summary>
    /// 진행 상황 텍스트 생성
    /// </summary>
    public override string GetProgressDescription(QuestStatus questStatus, int objectiveIndex)
    {
        if (objectiveIndex < 0 || objectiveIndex >= questStatus.objectiveProgress.Count) return "Error";
        // TODO: targetMonsterID로 실제 몬스터 이름 찾아오기 
        string targetName = $"몬스터 [{targetMonsterID}]";
        return $"{description} ({targetName}) ({questStatus.objectiveProgress[objectiveIndex]}/{requiredCount})";
    }
}
