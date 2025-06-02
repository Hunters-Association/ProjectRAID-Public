using ProjectRaid.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NPCType
{
    Support,
    Hunter,
    Merchant,
    QuestGiver
}

[CreateAssetMenu(fileName = "NPC_Data_", menuName = "ProjectRaid/NPC Data")]
public class NPCData : ScriptableObject
{
    [Header("기본 정보")]
    public int npcID;
    public string npcName = "NPC 이름";
    public NPCType npcType = NPCType.Support;
    [TextArea(3, 5)] public string description = "NPC 설명";
    public string affiliation = "소속";

    [Header("능력치")]
    public int baseMaxHp = 100;
    public float baseFollowRangeMin = 3.0f;
    public float baseFollowRangeMax = 6.0f;
    public float baseMoveSpeed = 4.0f;
    public float baseRunSpeed = 5.0f; // 전투 시 또는 특정 상황 이동 속도

    [Header("전투 능력치 (추가)")]
    public float DetectionRange = 15f;

    [Header("퀘스트 정보")]
    [Tooltip("이 NPC가 플레이어에게 제공할 수 있는 퀘스트의 ID 목록입니다.")]
    public List<int> availableQuestIDs; // 제공 가능한 퀘스트 ID 리스트

    [Tooltip("플레이어가 이 NPC에게 완료 보고를 할 수 있는 퀘스트의 ID 목록입니다.")]
    public List<int> completableQuestIDs;

    [Header("특수 상태")] // 또는 적절한 헤더에 추가
    [Tooltip("NPC가 기절 상태에서 자동으로 부활하기까지 걸리는 시간 (초). 0 이하면 자동 부활 안 함.")]
    public float reviveTime = 30.0f;

    [Header("스킬 및 장비")]
    public List<string> defaultSkillIDs; // SkillData의 skillID 목록
    // public AssetReference exclusiveEquipmentRef; // Addressables 사용 시
    public ItemData exclusiveEquipment; // 직접 ItemData SO 참조 시 (ItemData 스크립트 필요)
    public string mountPointName = "MountPoint"; // NPC 모델 내 탑승 위치 Transform 이름

    [Header("외형 및 음성")]
    // public AssetReference animatorOverrideRef; // Addressables 사용 시
    public AnimatorOverrideController animatorOverrideController; // 직접 참조 시
    // public VoiceSetData voiceSet; // VoiceSetData SO 참조 (별도 정의 필요)
    public List<string> dialogueKeywords; // ["조심해!", "뒤에 있어!"]

    [Header("성격 및 배경")]
    [TextArea(2, 4)] public string personality = "꼼꼼하고 따뜻한 성격";
    [TextArea(2, 4)] public string appearanceDetails = "치료 가방을 들고 있음";
    [TextArea(2, 4)] public string backgroundStory = "회복에 특화된 아카데미 소속";
}
