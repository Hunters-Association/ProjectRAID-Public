using System.Collections.Generic;
using UnityEngine;

public enum MonsterBehaviorType
{
    Passive = 0,     // 공격 안 함 (온순함) 
    Territorial = 1, // 선공 가능 (영역 몬스터)
    Aggressive = 2   // 선공 (난폭함)
}

// 몬스터 서식지 정의 (기획서 기준)
public enum MonsterAreaType
{
    TestScene = 0,
    Academy = 1,
    GreenForest = 2 // A forest of green trees
    // 필요에 따라 추가
}

[CreateAssetMenu(fileName = "Monster_Data", menuName = "Monster/Data/Generic Monster Data")]
public class MonsterData : ScriptableObject
{
    [Header("기본 정보")]
    public int monsterID;
    public string monsterName;
    public MonsterBehaviorType monsterType;
    [TextArea] public string monsterDescription;
    public MonsterAreaType monsterArea;

    [Header("퀘스트 시스템 연동")]
    public GameEventInt monsterKilledEvent; // Quest 시스템 등에서 사용할 이벤트

    [Header("기본 능력치")]
    public int maxHp = 10;
    public float moveSpeed = 5f;
    [Tooltip("플레이어를 감지하는 기본 범위")]
    public float detectionRange = 20f;
    [Tooltip("Idle 상태에서 배회하는 반경")]
    public float WanderRadius = 5f;
    [Tooltip("공격 대상과의 거리가 이 값 이하일 때 원거리 공격 대신 다른 행동을 고려할 수 있는 거리 (AI 로직에서 활용)")]
    public float engagementDistance = 3.0f; // 이름 변경: stopProjectileRange -> engagementDistance (좀 더 일반적인 용도)
    [Min(0)] public float behaviorRadius = 30f;

    [Header("사용 가능한 공격/능력 패턴")]
    [Tooltip("이 몬스터가 사용할 수 있는 공격 및 능력 데이터 에셋 목록입니다. (MeleeAttackData, ProjectileAttackData, AbilityAttackData 등)")]
    public List<AttackData> availableAttacks;
    

    [Header("드랍 정보 (갈무리 시)")]
    public List<DropItemInfo> dropTable;

    [Header("도망 설정 (Passive 타입 등)")]
    public float fleeReactivationTime = 60f;
    public string fleeReactivationPointTag = "RespawnPoint";
    public string fleeTargetTag = "FleePoint";

    [Header("온순화 설정 (저체력 도망 후)")]
    public float pacifiedHealthRegenRate = 1.0f;

    [Header("Idle 중 배회 시 아이템 드랍")]
    public float wanderDropChance = 5.0f;
    public GameObject wanderItemPrefab;
    public float wanderDropCheckInterval = 2.0f;

    
    [Header("땅파기/나오기 설정 (Morven 등)")]
    // 땅파기/나오기 관련 필드는 유지합니다. 이는 AttackData로 표현하기 애매한 '능력' 또는 '상태 전환'에 가깝기 때문입니다.
    // (추후 AbilityAttackData와 연동하여 리팩토링할 수도 있습니다)
    public float burrowDuration = 1.2f;
    public float emergeDuration = 1.2f;
    public float minTimeBurrowed = 3.0f;
    public float maxTimeBurrowed = 8.0f;
    public float emergeNearPlayerDistance = 5.0f;
    [Tooltip("스폰 지점으로부터 이 반경 내에 있을 때만 땅 파기를 시도합니다. 0 이하면 제한 없음.")]
    [Min(0)] public float burrowAllowedSpawnRadius = 25f;


    [Header("Idle 패턴 확률 (%) - 필요시 사용")]
    // Idle 패턴 선택 로직은 그대로 유지될 수 있습니다.
    public int idlePattern1_Chance = 50;
    public int idlePattern2_Chance = 40;
    public int idlePattern3_Chance = 10;

   
}
