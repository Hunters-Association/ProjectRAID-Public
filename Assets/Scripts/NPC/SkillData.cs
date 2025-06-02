using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] // Inspector에서 이 클래스의 리스트를 편집할 수 있도록 합니다.
public class AnimatorParameterSetter
{
    public enum ParamType { Bool, Int, Float, Trigger }
    public ParamType parameterType = ParamType.Trigger; // 기본값을 Trigger로 설정 (선택적)

    [Tooltip("Animator Controller에 정의된 파라미터의 이름")]
    public string parameterName;

    [Tooltip("Parameter Type이 Bool일 때 설정할 값")]
    public bool boolValue;

    [Tooltip("Parameter Type이 Int일 때 설정할 값")]
    public int intValue;

    [Tooltip("Parameter Type이 Float일 때 설정할 값")]
    public float floatValue;

    // Trigger 타입은 별도의 값이 필요 없으므로, parameterName만 사용합니다.
}
public enum SkillType { Heal, Buff, Debuff, Attack, Utility }
public enum SkillTargetType { Self, AllySingle, AllyArea, EnemySingle, EnemyArea, Point }

[CreateAssetMenu(fileName = "Skill_Data_", menuName = "ProjectRaid/Skill Data")]
public class SkillData : ScriptableObject
{
    [Header("기본 정보")]
    public string skillID = "SK_NewSkill";
    public string skillName = "새로운 스킬";
    [TextArea] public string description = "스킬 설명";
    public Sprite icon; // UI용 아이콘

    [Header("타입 및 대상")]
    public SkillType skillType = SkillType.Utility;
    public SkillTargetType targetType = SkillTargetType.Self;

    [Header("시전 및 효과")]
    public float cooldown = 10.0f;
    public float castTime = 0.5f; // 0이면 즉시 발동
    public float range = 5.0f;    // 스킬 사용 가능 거리 (타겟 지정 스킬용)
    public float effectRadius = 3.0f; // 범위 효과 반경

    [Header("공격 효과 (SkillType.Attack 일 때)")]
    // public bool isAttackEffect = false; // SkillType.Attack으로 구분하므로 이 플래그는 선택적
    [Tooltip("스킬의 기본 공격 데미지")]
    [Min(0)] public int attackDamageAmount = 0;

    [Tooltip("이 스킬이 투사체를 사용하는지 여부 (선택적, 투사체 로직은 ExecuteSkillEffectInternal에서 분기)")]
    public bool usesProjectile = false;

    [Header("애니메이션 및 효과")]
    [Tooltip("이 스킬 사용 시 Animator에 설정할 파라미터 목록입니다. 리스트의 순서대로 적용됩니다.")]
    public List<AnimatorParameterSetter> animatorParametersToSet = new();
    public GameObject vfxPrefab;    
    public AudioClip sfxClip;
    public string skillEffectEventName = "TriggerSkillEffect"; //애니메이션 이벤트에서 호출될 함수 이름
    // --- 스킬 효과 데이터 (하위 클래스로 분리하거나, 여기에 직접 정의) ---
    // 예시: 치료 효과
    [Header("치료 효과 (SkillType.Heal 일 때)")]
    public bool isHealEffect = false;
    [Min(0)] public int healAmount = 0; // 고정 회복량
    [Range(0f, 1f)] public float healPercent = 0.3f; // 최대 체력의 % 회복 (0.3 = 30%)
    public float healOverTimeDuration = 0f; // 지속 회복 시간 (0이면 즉시 회복)
    public int healTicks = 0; // 지속 회복 틱 수 (duration / ticks 간격으로 회복)

    //  버프 효과
    [Header("버프 효과 (SkillType.Buff 일 때)")]
    public bool isBuffEffect = false;
    // public StatModifierData buffStatModifier; // 적용할 스탯 변경 (StatModifierData SO 참조 - 별도 정의 필요)
    public float buffDuration = 10f;

    //  도발 효과
    [Header("도발 효과 (SkillType.Utility 또는 Debuff 일 때)")]
    public bool isTauntEffect = false;
    public float tauntDuration = 5f;
    public float tauntRadius = 10f; // 주변 몬스터를 도발할 반경
}
