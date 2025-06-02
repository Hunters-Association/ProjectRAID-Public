using UnityEngine;

[CreateAssetMenu(fileName = "ReachLocationObjective", menuName = "Quest System/Objectives/Reach Location Objective")]
public class ReachLocationObjectiveDefinition : QuestObjectiveDefinition
{
    [Tooltip("도달해야 할 지역의 고유 숫자 ID")]
    public int locationID; // ★★★ 타입 int로 변경 ★★★

    [Tooltip("지역 도달 시 발행되는 이벤트 (GameEventInt 타입 SO 연결)")]
    public GameEventInt locationReachedEvent; // ★★★ 타입 GameEventInt로 변경 ★★★

    private QuestStatus listenerTargetStatus;

    // 이벤트 리스너 설정
    public override void SetupListener(QuestStatus questStatus)
    {
        listenerTargetStatus = questStatus;
        // ★★★ int 받는 리스너 등록 ★★★
        locationReachedEvent?.RegisterListener(OnLocationReached);
        // Debug.Log($"Listener Setup: Quest '{questStatus?.questData?.questName}', Objective: Reach Location ID {locationID}");
    }

    // 이벤트 리스너 해제
    public override void RemoveListener(QuestStatus questStatus)
    {
        // ★★★ int 받는 리스너 해제 ★★★
        locationReachedEvent?.UnregisterListener(OnLocationReached);
        // Debug.Log($"Listener Removed: Quest '{questStatus?.questData?.questName}', Objective: Reach Location ID {locationID}");
        listenerTargetStatus = null;
    }

    // 이벤트 콜백
    // ★★★ 파라미터 타입 int로 변경 ★★★
    private void OnLocationReached(int reachedLocationID)
    {
        // ★★★ 숫자 ID 비교 ★★★
        if (listenerTargetStatus != null && reachedLocationID == locationID)
        {
            // Debug.Log($"Event Rx: Reached Location ID {reachedLocationID} for Quest '{listenerTargetStatus?.questData?.questName}'. Notifying PlayerQuest.");
            // 도달 시 진행도 1 증가 (완료 처리)
            QuestManager.Instance.PlayerQuestDataManager.NotifyObjectiveProgress(listenerTargetStatus, this, 1);
        }
    }

    // 완료 체크 (1번 도달하면 완료)
    public override bool IsComplete(QuestStatus questStatus, int objectiveIndex)
    {
        if (objectiveIndex < 0 || objectiveIndex >= questStatus.objectiveProgress.Count) return false;
        // 진행도가 1 이상이면 완료로 간주
        return questStatus.objectiveProgress[objectiveIndex] >= 1;
    }

    // 진행 상황 텍스트
    public override string GetProgressDescription(QuestStatus questStatus, int objectiveIndex)
    {
        if (objectiveIndex < 0 || objectiveIndex >= questStatus.objectiveProgress.Count) return "Error";
        // TODO: locationID(int)로 실제 지역 이름 찾아오기 (선택적)
        string locationName = $"지역 ID [{locationID}]"; // ★ int ID 표시 ★
        string statusText = questStatus.objectiveProgress[objectiveIndex] >= 1 ? "(도착 완료)" : "(미도착)";
        return $"{description} ({locationName}) {statusText}";
    }
}
