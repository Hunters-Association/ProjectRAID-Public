using UnityEngine;

/// <summary>
/// 모든 퀘스트 보상 정의의 기반이 되는 추상 ScriptableObject 클래스.
/// </summary>
public abstract class QuestRewardDefinition : ScriptableObject
{
    /// <summary>
    /// 플레이어에게 이 보상을 지급합니다.
    /// </summary>
    /// <param name="playerQuest">보상을 받을 플레이어의 PlayerQuest 컴포넌트</param>
    public abstract void GrantReward(PlayerQuestDataManager playerQuest);

    /// <summary>
    /// UI 등에 표시될 보상 설명을 반환합니다.
    /// </summary>
    /// <returns>보상 설명 문자열</returns>
    public abstract string GetDescription();
}
