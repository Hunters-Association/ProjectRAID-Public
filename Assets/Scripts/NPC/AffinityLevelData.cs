using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StatModifierData
{
    public enum StatType { Health, Attack, Defense } //  타입
    public StatType statToModify;
    public float value;
    public bool isPercentage; // true면 백분율 증가, false면 고정값 증가
}

[CreateAssetMenu(fileName = "AffinityLevel_", menuName = "ProjectRaid/Affinity Level Data")]
public class AffinityLevelData : ScriptableObject
{
    [Header("레벨 정보")]
    public int level; // 호감도 레벨 
    [Min(0)] public int requiredAffinity; // 이 레벨 달성에 필요한 누적 호감도

    [Header("해금 요소")]
    public int unlockQuestID; // 해금되는 퀘스트의 ID (0이면 없음)
    public List<string> unlockSkillIDs; // 해금되는 스킬의 ID 목록
    public List<StatModifierData> playerStatBonuses; // 플레이어에게 제공되는 스탯 보너스
    public List<StatModifierData> npcStatBonuses;    // NPC 자신에게 적용되는 스탯 보너스
    [TextArea(2, 5)] public string newDialogueSnippet = "새로운 대화 내용..."; // 이 레벨 도달 시 추가되는 대화 

    [Header("전투 참여")]
    public bool canJoinCombat = false; // 이 레벨부터 전투에 참여 가능한지
    // public CombatBehaviorData combatBehaviorOverride; // 이 레벨부터 변경될 전투 행동 패턴 SO (선택적)
}
