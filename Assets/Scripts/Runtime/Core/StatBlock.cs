using UnityEngine;

[System.Serializable]
public struct StatBlock
{
    public float Attack;            // 기본 공격력 (raw)
    public float AttackSpeed;       // 초당 공격 횟수, 혹은 애니메이션‑노멀라이즈 값

    [Range(0f, 100f)]
    public float CriticalChance;    // 크리티컬 발동 확률(%) – 예: 5 = +5%
    public float CriticalDamage;    // 크리티컬 추가 배율(%) – 예: 50 = +50%

    public float HP;                // 체력
    public float Defense;           // 방어력
    
    public float Stamina;           // 스태미나 혹은 집중력
    public float MoveSpeed;         // 이동 속도 배율 (1 = 기본)
}