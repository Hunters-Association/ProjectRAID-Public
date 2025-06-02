using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MeleeAttack_Data", menuName = "Monster/Attack Data/Melee Attack")]
public class MeleeAttackData : AttackData
{
    [Header("근접 공격 특정 속성")]
    [Tooltip("이 근접 공격의 기본 데미지")]
    public int damage = 10;

    // 추후 추가 가능 필드:
    // public DamageType damageType = DamageType.Physical; // 데미지 유형 (Enum 정의 필요)
    // public float knockbackForce = 5f; // 넉백 힘
    // public float cleaveRadius = 0f; // 범위 공격 반경 (0이면 단일 타겟)
    // public string hitSoundName = "MeleeHit"; // 피격 사운드 이름
}
