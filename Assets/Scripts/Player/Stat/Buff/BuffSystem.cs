using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectRaid.EditorTools;

/// <summary>
/// 버프의 실시간 존재를 표현하는 구조체(식별용)
/// </summary>
public readonly struct BuffInstance
{
    public readonly PlayerBuffData Data;
    public BuffInstance(PlayerBuffData d) => Data = d;
}

/// <summary>
/// 런타임 버프 적용/제거를 관리하는 컴포넌트
/// </summary>
public class BuffSystem : MonoBehaviour
{
    [FoldoutGroup("Stat", ExtendedColor.Silver)]
    [SerializeField] private StatController stats;

    private readonly List<BuffInstance> active = new();

    public event Action<BuffInstance> OnBuffAdded;
    public event Action<BuffInstance> OnBuffRemoved;

    public void AddBuff(PlayerBuffData data)
    {
        var inst = new BuffInstance(data);
        active.Add(inst);
        stats.AddModifier(new StatModifier(data.targetStat, data.value, inst));
        OnBuffAdded?.Invoke(inst);
        StartCoroutine(RemoveAfter(inst, data.duration));
    }

    private IEnumerator RemoveAfter(BuffInstance inst, float sec)
    {
        yield return new WaitForSeconds(sec);
        active.Remove(inst);
        stats.RemoveModifiersFrom(inst);
        OnBuffRemoved?.Invoke(inst);
    }
}
