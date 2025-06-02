using UnityEngine;
using ProjectRaid.Core;
using ProjectRaid.Data;

public static class DamageCalculator
{
    public static (float finalDamage, bool isCritical) CalculateWeaponDamage(WeaponData weapon, float motionValue)
    {
        float raw = weapon.BaseStats.Attack;
        float random = Random.Range(0.9f, 1.1f);
        float baseDamage;

        if (weapon.Class is WeaponClass.Sword)
        {
            float sharpnessMultiplier = SharpnessHelper.GetMultiplier(weapon.Sharpness);
            baseDamage = raw * random * motionValue * sharpnessMultiplier;
        }
        else
        {
            baseDamage = raw * random * motionValue;
        }

        float critChance = weapon.BaseStats.CriticalChance;
        float critDamage = weapon.BaseStats.CriticalDamage;
        bool isCritical = Random.Range(0f, 100f) <= critChance;

        float finalDamage = baseDamage;

        if (isCritical) finalDamage *= 1f + (critDamage / 100f);

        return (finalDamage, isCritical);
    }
}