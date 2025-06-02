using System.Collections.Generic;
using UnityEngine;
using ProjectRaid.Core;
using ProjectRaid.EditorTools;

namespace ProjectRaid.Data
{
    [System.Flags]
    public enum WeaponBattleFeature
    {
        None            = 0,
        Cuttable        = 1 << 0,   // 절단 가능 (ex. 검 또는 랜스로 꼬리 공격)
        Destructible    = 1 << 1,   // 파괴 가능 (ex. 총을 제외한 무기로 뿔 공격)
        Stunnable       = 1 << 2    // 기절 가능 (ex. 건틀릿으로 머리 지속 공격)
    }

    public enum WeaponSFX
    {
        Attack,
        Charge,
        Reload,
        Explode
    }

    [System.Serializable]
    public class WeaponSoundClip
    {
        public AudioClip Clip;
        [Range(0f, 1f)] public float Volume = 1f;
    }

    [System.Serializable]
    public class WeaponSoundEntry
    {
        public WeaponSFX Type;
        public List<WeaponSoundClip> Clips;
    }

    [System.Serializable]
    public class WeaponOffset
    {
        public Vector3 position;
        public Vector3 rotation;
    }

    [CreateAssetMenu(fileName = "Weapon", menuName = "Data/Equipment/Weapon")]
    public class WeaponData : EquipmentData
    {
        #region WEAPON DATA
        [FoldoutGroup("무기 데이터", ExtendedColor.White)]
        public WeaponClass Class;

        [Space(20f)]
        public int MaxCombo;
        public bool SupportsCharge;
        public WeaponBattleFeature BattleFeatures;

        public float CutValue;
        public float DestructionValue;
        [Range(0f, 1f)] public float StunChance;

        public bool IsCuttable => BattleFeatures.HasFlag(WeaponBattleFeature.Cuttable);
        public bool IsDestructible => BattleFeatures.HasFlag(WeaponBattleFeature.Destructible);
        public bool IsStunnable => BattleFeatures.HasFlag(WeaponBattleFeature.Stunnable);

        [FoldoutGroup("Rifle-Only", ExtendedColor.White)]
        [ShowIf(nameof(Class), WeaponClass.Rifle)] public float BulletRange;
        [ShowIf(nameof(Class), WeaponClass.Rifle)] public int MagazineSize;
        [ShowIf(nameof(Class), WeaponClass.Rifle)] public float ReloadTime;

        [FoldoutGroup("Sword-Only", ExtendedColor.White)]
        [ShowIf(nameof(Class), WeaponClass.Sword)] public SharpnessLevel Sharpness;

        [FoldoutGroup("SFX", ExtendedColor.Gold)]
        public List<WeaponSoundEntry> WeaponSounds;

        [FoldoutGroup("Offset", ExtendedColor.DodgerBlue)]
        public WeaponOffset handOffset;
        public WeaponOffset backOffset;
        #endregion

        #region PUBLIC API
        public int GetAnimatorWeaponIndex() => (int)Class + 1;
        public WeaponSoundClip GetRandomSFX(WeaponSFX type)
        {
            var entry = WeaponSounds?.Find(e => e.Type == type);
            if (entry != null && entry.Clips != null && entry.Clips.Count > 0)
                return entry.Clips[Random.Range(0, entry.Clips.Count)];

            return null;
        }
        #endregion
    }
}
