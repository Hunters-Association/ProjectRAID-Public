using System.Collections.Generic;
using UnityEngine;
using ProjectRaid.EditorTools;

[CreateAssetMenu(fileName = "Perk", menuName = "Data/Player/Perk")]
public class PerkData : ScriptableObject
{
    [FoldoutGroup("기본 정보", ExtendedColor.Plum)]
    public string perkId;
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;

    [FoldoutGroup("코스트 및 조건", ExtendedColor.Plum)]
    public int requiredLevel = 1;
    public int cost = 1;
    public List<PerkData> prerequisites;

    [FoldoutGroup("스탯 보너스", ExtendedColor.Plum)]
    public StatModifier[] statBonuses;
}
