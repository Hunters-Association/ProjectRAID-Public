using UnityEngine;
using ProjectRaid.Core;
using ProjectRaid.EditorTools;

namespace ProjectRaid.Data
{
    public abstract class EquipmentData : ScriptableObject, IHasID
    {
        [FoldoutGroup("장비 데이터", ExtendedColor.White)]
        [SerializeField] protected int equipmentID;
        [SerializeField] protected int requireLevel;
        [SerializeField] protected EquipmentSlot slot;
        [SerializeField] protected StatBlock baseStats;

        public int EquipmentID => equipmentID;
        public int ID => equipmentID;
        public int RequireLevel => requireLevel;
        public EquipmentSlot Slot => slot;
        public StatBlock BaseStats => baseStats;

#if UNITY_EDITOR
        #region EDITOR SETTER
        public void Editor_SetID(int id) => equipmentID = id;
        public void Editor_SetReqLevel(int level) => requireLevel = level;
        public void Editor_SetSlot(EquipmentSlot slot) => this.slot = slot;
        public void Editor_SetStats(StatBlock stats) => baseStats = stats;
        #endregion
#endif
    }
}
