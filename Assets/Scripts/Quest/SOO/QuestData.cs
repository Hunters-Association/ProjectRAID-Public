using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// ★ QuestType Enum: 퀘스트 종류 정의 (테이블 참조) ★
public enum QuestType
{
    Main = 0,
    Sub = 1,
    Repeatable = 2, // 반복 퀘스트
    Daily = 3       // 일일 퀘스트 등 추가 가능
}

// ★ AreaID Enum 또는 int 사용 (테이블 참조) ★
// Enum을 사용하면 Inspector에서 선택하기 편리합니다.
public enum QuestAreaID
{
    None = 0,
    TestScene = 1,
    Academy = 2,
    GreenForest = 3
    // 필요에 따라 지역 추가
}

[CreateAssetMenu(fileName = "New Quest", menuName = "Quest System/Quest")]
public class QuestData : ScriptableObject
{
    [Header("기본 정보")]
    [Tooltip("퀘스트의 고유 숫자 ID (중복 불가)")]
    public int questID; // ★ 타입 int로 변경 ★

    [Tooltip("UI에 표시될 퀘스트 이름")]
    public string questName; // QuestName

    [Tooltip("퀘스트 타입 (메인, 서브, 반복 등)")]
    public QuestType questType; // ★ 필드 추가 (QuestType) ★

    [Tooltip("퀘스트 로그 등에 표시될 상세 설명")]
    [TextArea(3, 5)]
    public string description; // QuestDesc

    [Tooltip("퀘스트 발생 또는 관련 지역 ID")]
    public QuestAreaID areaID; // ★ 필드 추가 (AreaID) ★

    [Tooltip("퀘스트 수락 가능 최소 레벨")]
    public int requiredLevel; // RequireLv

    [Tooltip("반복 가능한 퀘스트인지 여부")]
    public bool isRepeatable; // ★ 필드 추가 (IsRepeatable) ★

    [Tooltip("이 퀘스트를 받기 위해 먼저 완료해야 하는 다른 퀘스트 ID 목록")]
    public List<int> prerequisiteQuestIDs; // ★ 타입 List<int>로 변경 (ConditionQuestID) ★

    [Header("NPC 정보 (선택적)")]
    [Tooltip("퀘스트를 제공하는 NPC의 식별자 (문자열 또는 int)")]
    public int questGiverID; // 필요시 int로 변경 가능
    [Tooltip("퀘스트 완료 보고를 받는 NPC의 식별자 (문자열 또는 int)")]
    public int questCompleterID;

    [Header("퀘스트 타입 특정 설정")]
    [Tooltip("이 퀘스트가 토벌 퀘스트인지 여부입니다. 토벌 퀘스트일 경우 동료 선택 UI가 나올 수 있습니다.")]
    public bool isHuntQuest = false; // 토벌 퀘스트 여부

    [Tooltip("이 토벌 퀘스트에서 특별히 동행을 제안할 NPC ID 목록입니다. 비어있으면 조건 맞는 모든 NPC가 대상이 됩니다.")]
    public List<int> specificCompanionNpcIDs; // 특정 NPC만 동료로 제안할 경우 사용

    // 여러 개의 목표를 가질 수 있습니다.
    [Header("목표 정의 (Objective Definition SO 연결)")]
    [Tooltip("이 퀘스트를 완료하기 위한 목표 목록")]
    public List<QuestObjectiveDefinition> objectives;

    
    // 여러 종류의 보상을 조합할 수 있습니다.
    [Header("보상 정의 (Reward Definition SO 연결)")]
    [Tooltip("퀘스트 완료 시 지급될 보상 목록")]
    public List<QuestRewardDefinition> rewards;

    // ▼▼▼  테이블의 UnlockQuestID 반영 ▼▼▼
    [Header("완료 후 설정")]
    [Tooltip("이 퀘스트 완료 시 진행 가능하게 될 다른 퀘스트 ID 목록")]
    public List<int> unlockQuestIDs; // ★ 필드 추가 (UnlockQuestID) ★
}
