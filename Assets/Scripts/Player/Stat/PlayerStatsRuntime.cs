using System;
using UnityEngine;

/// <summary>
/// 플레이어의 런타임 스탯 관리
/// </summary>
[Serializable]
public class PlayerStatsRuntime
{
    // 기본 스탯 (곡선에서 계산된 값)
    public int Level;
    public int Exp;

    public float AttackPower;
    public float AttackSpeed;
    public float CritChance;
    public float CritDamage;

    public float MaxHealth;
    public float CurrentHealth;
    public float Defense;

    public float MaxStamina;
    public float CurrentStamina;
    public float MoveSpeed;
    public float RunSpeed;


    // public event Action OnDie;
    public event Action<int> OnLevelChanged;
    public event Action<float, float> OnHealthChanged;  // (current, max)
    public event Action<float, float> OnStaminaChanged; // (current, max)

    public void InitHealth(float max)
    {
        MaxHealth = CurrentHealth = max;
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    public void InitStamina(float max)
    {
        MaxStamina = CurrentStamina = max;
        OnStaminaChanged?.Invoke(CurrentStamina, MaxStamina);
    }

    public void SetHealth(float value)
    {
        CurrentHealth = Mathf.Clamp(value, 0, MaxHealth);
        OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
    }

    public void SetStamina(float value)
    {
        CurrentStamina = Mathf.Clamp(value, 0, MaxStamina);
        OnStaminaChanged?.Invoke(CurrentStamina, MaxStamina);
    }

    public void LevelUp()
    {
        OnLevelChanged?.Invoke(++Level);
    }
}
