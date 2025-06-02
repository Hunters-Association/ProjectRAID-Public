using System.Collections.Generic;
using ProjectRaid.Core;
using ProjectRaid.Data;

namespace ProjectRaid.Extensions
{
    public static class WeaponExtensions
    {
        private static readonly HashSet<WeaponClass> AimingCategories = new()
        {
            WeaponClass.Rifle,
        };

        private static readonly HashSet<WeaponClass> BlockingCategories = new()
        {
            // WeaponClass.Sword,
            WeaponClass.Lance
        };

        private static readonly HashSet<WeaponClass> ShieldCategories = new()
        {
            WeaponClass.Lance
        };

        private static readonly HashSet<WeaponClass> ChargeAttackCategories = new()
        {
            WeaponClass.Sword
        };

        public static int GetMaxCombo(this WeaponData weapon) => weapon.MaxCombo;

        public static bool CanAim(this WeaponData weapon) =>
            AimingCategories.Contains(weapon.Class);

        public static bool CanBlock(this WeaponData weapon) =>
            BlockingCategories.Contains(weapon.Class);

        public static bool CanUseShield(this WeaponData weapon) =>
            ShieldCategories.Contains(weapon.Class);

        public static bool CanChargeAttack(this WeaponData weapon) =>
            ChargeAttackCategories.Contains(weapon.Class);
    }
}
