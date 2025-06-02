using System.Collections.Generic;
using UnityEngine;
using ProjectRaid.Core;
using ProjectRaid.EditorTools;

namespace ProjectRaid.Data
{
    [CreateAssetMenu(fileName = "Armor", menuName = "Data/Equipment/Armor")]
    public class ArmorData : EquipmentData
    {
        #region ARMOR DATA
        [FoldoutGroup("방어구 데이터", ExtendedColor.White)]
        public ArmorClass Class;
        public int SetBonusKey;
        public StatBlock SetBonusStats;
        #endregion
    }
}