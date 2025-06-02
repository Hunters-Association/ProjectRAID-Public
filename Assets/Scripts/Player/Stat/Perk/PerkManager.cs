using System.Collections.Generic;
using UnityEngine;

public class PerkManager : MonoBehaviour
{
    [SerializeField] private PlayerStats stats;
    [SerializeField] private List<PerkData> allPerks;

    private readonly HashSet<PerkData> unlockedPerks = new();
    private int perkPoints = 0;

    public IReadOnlyCollection<PerkData> UnlockedPerks => unlockedPerks;

    public void AddPerkPoint() => perkPoints++;

    public bool CanUnlock(PerkData perk)
    {
        if (unlockedPerks.Contains(perk)) return false;
        if (stats.Level < perk.requiredLevel) return false;
        if (perkPoints < perk.cost) return false;

        foreach (var prerequisite in perk.prerequisites)
        {
            if (!unlockedPerks.Contains(prerequisite)) return false;
        }

        return true;
    }

    public bool TryUnlockPerk(PerkData perk)
    {
        if (!CanUnlock(perk)) return false;

        unlockedPerks.Add(perk);
        perkPoints -= perk.cost;
        ApplyPerk(perk);
        return true;
    }

    private void ApplyPerk(PerkData perk)
    {
        foreach (var mod in perk.statBonuses)
        {
            stats.ApplyModifier(mod.Type, mod.Value);
        }
    }
}
