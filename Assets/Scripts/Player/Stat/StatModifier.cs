using System.Collections.Generic;
using UnityEngine;

public enum StatType
{
    AttackPower,
    AttackSpeed,
    CriticalChance,
    CriticalDamage,
    MaxHealth,
    Defense,
    Stamina,
    MoveSpeed
}

[System.Serializable]
/// <summary>
/// 단일 스탯 변화(장비, 버프, 디버프 등) 표현
/// </summary>
public readonly struct StatModifier
{
    public readonly StatType Type;
    public readonly float Value;
    public readonly object Source; // 장비, 버프 등 구분용

    public StatModifier(StatType type, float value, object source)
    {
        Type = type;
        Value = value;
        Source = source;
    }
}

public class ModifierStack
{
    private readonly Dictionary<StatType, float> totals = new();
    private readonly List<StatModifier> list = new();

    public void Add(StatModifier mod)
    {
        list.Add(mod);
        totals[mod.Type] = this[mod.Type] + mod.Value;
    }

    public void RemoveBySource(object source)
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (ReferenceEquals(list[i].Source, source))
            {
                var m = list[i];
                totals[m.Type] = this[m.Type] - m.Value;
                list.RemoveAt(i);
            }
        }
    }

    public float this[StatType t] => totals.TryGetValue(t, out var v) ? v : 0f;
}
