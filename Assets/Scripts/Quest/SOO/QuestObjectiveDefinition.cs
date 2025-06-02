using UnityEngine;





public abstract class QuestObjectiveDefinition : ScriptableObject
{
    [Tooltip("UI에 표시될 목표 설명 (예: 고블린 처치)")]
    [TextArea] public string description;

    [Tooltip("목표 달성에 필요한 횟수 또는 수량")]
    public int requiredCount = 1;

    /// <summary>
    /// 이 목표가 활성화될 때 필요한 이벤트 리스너 등을 설정합니다.
    /// </summary>
    /// <param name="questStatus">이 목표가 속한 퀘스트의 상태 정보</param>
    public abstract void SetupListener(QuestStatus questStatus);

    /// <summary>
    /// 이 목표가 비활성화될 때 설정했던 리스너 등을 제거합니다.
    /// </summary>
    /// <param name="questStatus">이 목표가 속한 퀘스트의 상태 정보</param>
    public abstract void RemoveListener(QuestStatus questStatus);

    /// <summary>
    /// 현재 이 목표가 완료되었는지 확인합니다.
    /// </summary>
    /// <param name="playerQuest">플레이어의 퀘스트 정보 컴포넌트</param>
    /// <param name="questStatus">이 목표가 속한 퀘스트의 상태 정보</param>
    /// <param name="objectiveIndex">QuestData 내 objectives 리스트에서의 이 목표의 인덱스</param>
    /// <returns>완료 여부</returns>
    public abstract bool IsComplete(QuestStatus questStatus, int objectiveIndex);

    /// <summary>
    /// UI에 표시될 현재 진행 상황 텍스트를 반환합니다. (예: 고블린 처치 (3/5))
    /// </summary>
    /// <param name="questStatus">이 목표가 속한 퀘스트의 상태 정보</param>
    /// <param name="objectiveIndex">QuestData 내 objectives 리스트에서의 이 목표의 인덱스</param>
    /// <returns>진행 상황 설명 문자열</returns>
    public abstract string GetProgressDescription(QuestStatus questStatus, int objectiveIndex);
}
