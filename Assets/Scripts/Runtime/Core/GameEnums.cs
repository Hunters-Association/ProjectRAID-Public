namespace ProjectRaid.Core
{
    public enum ItemType { Equipment, Consumable, Material, Misc, Cosmetic }
    public enum EquipmentSlot { Weapon, Helmet, Top, Bottom, Gloves, Shoes }

    // 세분화 enum
    public enum WeaponClass { Rifle, Sword, Lance, Gauntlet }
    public enum ArmorClass { Helmet, Top, Bottom, Gloves, Shoes }

    [System.Flags]
    public enum WeaponBattleFeature
    {
        None            = 0,
        Cuttable        = 1 << 0,
        Destructible    = 1 << 1,
        Stunnable       = 1 << 2
    }
}