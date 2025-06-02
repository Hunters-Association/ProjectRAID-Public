using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private PlayerStatData baseData;

    public int Level { get; private set; } = 1;
    public int CurrentExp { get; private set; } = 0;
    public int ExpToNextLevel => 100 + (Level - 1) * 50;

    public float MoveSpeed { get; private set; }
    public float RunSpeed { get; private set; }

    public float MaxHealth { get; private set; }
    public float CurrentHealth { get; private set; }
    public float Defense { get; private set; }

    public float AttackPower { get; private set; }
    public float AttackSpeed { get; private set; }
    public float CriticalChance { get; private set; }
    public float CriticalDamage { get; private set; }

    public float KnockbackResistance { get; private set; }
    public float InvincibleDuration { get; private set; }

    public event Action OnDie;

    /// <summary>
    /// SO 데이터 기반으로 초기화
    /// </summary>
    public void ApplyBaseStats()
    {
        if (baseData == null)
        {
            Debug.LogWarning("[PlayerStats] PlayerStatData가 등록되지 않았습니다.");
            return;
        }

        // AttackPower = baseData.attackPower;
        // AttackSpeed = baseData.attackSpeed;
        // CriticalChance = baseData.criticalChance / 100f;
        // CriticalDamage = baseData.criticalDamage;

        // MaxHealth = baseData.maxHealth;
        // CurrentHealth = MaxHealth;
        // Defense = baseData.defense;

        MoveSpeed = baseData.baseMoveSpeed;
        RunSpeed = baseData.baseRunSpeed;

        KnockbackResistance = baseData.knockbackResistance;
        InvincibleDuration = baseData.invincibleDuration;
    }

    public void ApplyModifier(StatType stat, float value)
    {
        switch (stat)
        {
            case StatType.MaxHealth:
                MaxHealth += Mathf.RoundToInt(value);
                break;
            case StatType.Defense:
                Defense += value;
                break;
            case StatType.AttackPower:
                AttackPower += value;
                break;
            case StatType.CriticalChance:
                CriticalChance += value;
                break;
            case StatType.CriticalDamage:
                CriticalDamage += value;
                break;
        }

        Debug.Log($"[PlayerStats] {stat} 증가: {value} → 현재 값: {GetStatValue(stat)}");
    }

    public float GetStatValue(StatType stat)
    {
        return stat switch
        {
            StatType.MaxHealth => MaxHealth,
            StatType.AttackPower => AttackPower,
            StatType.Defense => Defense,
            StatType.CriticalChance => CriticalChance,
            StatType.CriticalDamage => CriticalDamage,
            _ => 0f
        };
    }

    #region LEVEL
    public void GainExp(int amount)
    {
        CurrentExp += amount;
        while (CurrentExp >= ExpToNextLevel)
        {
            CurrentExp -= ExpToNextLevel;
            LevelUp();
        }
    }

    private void LevelUp()
    {
        Level++;
        MaxHealth += 10;
        AttackPower += 1.5f;
        Defense += 1f;
        Debug.Log($"레벨업! 현재 레벨: {Level}");
    }
    #endregion

    #region HEALTH
    public void TakeDamage(float amount)
    {
        // 방어력 적용, 체력 감소, UI 반영 등
        float finalDamage = Mathf.Max(0, amount - Defense);
        CurrentHealth = Mathf.Max(0, CurrentHealth - (int)finalDamage);

        if (CurrentHealth <= 0)
        {
            OnDie?.Invoke();
        }
    }

    public void Heal(int amount)
    {
        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
    }
    #endregion
}
