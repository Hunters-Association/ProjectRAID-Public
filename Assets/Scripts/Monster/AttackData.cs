using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AttackData : ScriptableObject
{
    [Header("공통 공격 속성")]
    [Tooltip("에디터 및 로그 식별용 공격 이름")]
    public string attackName = "기본 공격";

    [Tooltip("공격 완료 후 다시 사용하기까지 걸리는 시간 (초)")]
    [Min(0)] public float cooldown = 2.0f;

    [Tooltip("공격 행동의 총 지속 시간 (애니메이션 + 회복/후딜레이). 상태 타이밍에 사용됩니다.")]
    [Min(0)] public float attackDuration = 1.0f;

    [Tooltip("이 공격을 수행하기 위한 최소 거리")]
    [Min(0)] public float minRange = 0f;

    [Tooltip("이 공격을 시작할 수 있는 최대 거리")]
    [Min(0)] public float maxRange = 1.5f;

    [Tooltip("공격 애니메이션 시작 후 데미지/효과가 실제로 적용되기까지 걸리는 시간 (초). 애니메이션과 동기화하는 데 도움됩니다.")]
    [Min(0)] public float damageApplicationDelay = 0.5f;

    [Tooltip("(선택 사항) 이 공격에 재생할 애니메이션 트리거 이름")]
    public string animationTriggerName = ""; // 예: "MeleeAttack", "ShootProjectile"

    [Header("AI 결정 요소 (선택 사항)")]
    [Tooltip("여러 공격이 가능할 때 이 공격을 선택할 상대적 확률/가중치 (높을수록 가능성 높음)")]
    [Min(0)] public int decisionWeight = 100;

    // 추후 필요한 조건 추가 가능:
    // public float requiredHealthPercent = 0f; // 이 공격을 사용하기 위한 최소 HP 비율
    // public TargetStatus requiredTargetStatus = TargetStatus.None; // 타겟이 특정 상태일 때만 사용 등
}
