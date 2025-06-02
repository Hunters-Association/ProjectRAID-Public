using System.Collections.Generic;
using UnityEngine;

public enum QuestProgressState
{
    NotStarted,                 // 아직 시작 안 함
    InProgress,                 // 진행 중
    ObjectivesMet_AwaitingReport, // 목표 달성, 완료 보고 대기 중
    Completed,                  // 완료 (보상 수령 완료)
    Failed                      // 실패 (선택적)
}
/// <summary>
/// 플레이어가 진행 중인 개별 퀘스트의 상태와 진행도를 저장하는 클래스.
/// </summary>
[System.Serializable] // 저장 시스템에서 직렬화 가능하도록
public class QuestStatus
{
    [Tooltip("참조하는 QuestData의 고유 ID")]
    public int questID; // 저장/로드 시 이 ID를 사용해 QuestData 참조 복원

    [Tooltip("퀘스트 완료(보상 수령) 여부")]
    public bool isCompleted;

    [Tooltip("목표별 현재 진행도 (QuestData.objectives 순서와 일치)")]
    public List<int> objectiveProgress;

    public QuestProgressState ProgressState { get; set; }
    // --- 런타임 전용 필드 (저장되지 않음) ---
    [System.NonSerialized] public QuestData questData; // 로드 후 할당될 실제 QuestData 참조
    [System.NonSerialized] private bool listenersActive = false; // 이벤트 리스너 활성화 상태

    /// <summary> 기본 생성자 (로드 시 필요할 수 있음) </summary>
    public QuestStatus() { }

    /// <summary> 새로운 퀘스트를 시작할 때 호출되는 생성자 </summary>
    public QuestStatus(QuestData data)
    {
        questID = data.questID;
        questData = data;
        isCompleted = false;
        ProgressState = QuestProgressState.InProgress; // 퀘스트 수락 시 InProgress 상태로 시작
        objectiveProgress = new List<int>();
        if (data.objectives != null) // objectives null 체크 추가
        {
            for (int i = 0; i < data.objectives.Count; i++)
            {
                objectiveProgress.Add(0);
            }
        }
    }

    /// <summary> 이 퀘스트의 목표들에 대한 이벤트 리스너를 활성화합니다. </summary>
    public void ActivateListeners()
    {
        if (!listenersActive && questData != null)
        {
            // Debug.Log($"Activating listeners for Quest: {questData.questName}");
            for (int i = 0; i < questData.objectives.Count; i++)
            {
                // 각 목표 정의 SO의 SetupListener 호출
                questData.objectives[i]?.SetupListener(this);
            }
            listenersActive = true;
        }
    }

    /// <summary> 이 퀘스트의 목표들에 대한 이벤트 리스너를 비활성화합니다. </summary>
    public void DeactivateListeners()
    {
        if (listenersActive && questData != null)
        {
            // Debug.Log($"Deactivating listeners for Quest: {questData.questName}");
            for (int i = 0; i < questData.objectives.Count; i++)
            {
                // 각 목표 정의 SO의 RemoveListener 호출
                questData.objectives[i]?.RemoveListener(this);
            }
            listenersActive = false;
        }
    }

    /// <summary> 특정 목표의 진행도를 업데이트합니다. </summary>
    /// <returns> 진행도 변경 여부 </returns>
    public bool UpdateProgress(int objectiveIndex, int amount)
    {
        if (questData == null || objectiveIndex < 0 || objectiveIndex >= objectiveProgress.Count || IsObjectiveComplete(objectiveIndex))
            return false; // 유효하지 않거나 이미 완료된 목표

        int oldValue = objectiveProgress[objectiveIndex];
        int required = questData.objectives[objectiveIndex].requiredCount;
        // 현재 값에 amount를 더하고, 0과 requiredCount 사이로 제한
        objectiveProgress[objectiveIndex] = Mathf.Clamp(oldValue + amount, 0, required);

        bool isValueChanged = objectiveProgress[objectiveIndex] != oldValue;
        if (isValueChanged && questID == 1) AnalyticsManager.SendFunnelStep(objectiveProgress[objectiveIndex] + 4);

        // 값이 변경되었는지 확인
            return isValueChanged;
    }

    /// <summary> 특정 목표의 완료 여부를 확인합니다. </summary>
    public bool IsObjectiveComplete(int objectiveIndex)
    {
        if (questData == null || objectiveIndex < 0 || objectiveIndex >= objectiveProgress.Count) return false;
        return objectiveProgress[objectiveIndex] >= questData.objectives[objectiveIndex].requiredCount;
    }

    /// <summary> 모든 목표가 완료되었는지 확인합니다. </summary>
    public bool AreAllObjectivesComplete()
    {
        if (questData == null) return false;
        for (int i = 0; i < questData.objectives.Count; i++)
        {
            if (!IsObjectiveComplete(i)) return false; // 하나라도 완료되지 않았으면 false
        }
        return true; // 모든 목표가 완료됨
    }

    /// <summary> 로드 후 QuestData 참조를 복원하는 함수 </summary>
    public void RestoreQuestDataReference(QuestDatabase db)
    {
        if (db != null)
        {
            questData = db.GetQuestByID(questID);
            if (questData == null)
            {
                Debug.LogError($"Failed to restore QuestData reference for loaded quest ID: {questID}");
            }
        }
    }
}
