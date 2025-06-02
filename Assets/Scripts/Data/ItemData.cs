using UnityEngine;
using ProjectRaid.Core;
using ProjectRaid.EditorTools;

namespace ProjectRaid.Data
{
    [CreateAssetMenu(fileName = "Item", menuName = "Data/Item")]
    public class ItemData : ScriptableObject, IHasID
    {
        [FoldoutGroup("아이템 데이터", ExtendedColor.White)]
        [SerializeField] private int itemID;

        public ItemType ItemType;
        public EquipmentData Equipment;

        [Space(20f)]
        public string DisplayNameKey;
        public string DescriptionKey;

        [Space(20f)]
        public int Price;
        public float Weight;

        public Sprite Icon;
        public GameObject Prefab;

        [FoldoutGroup("아이템 데이터/스택", ExtendedColor.White)]
        public bool Stackable;
        public int MaxStack;

        [FoldoutGroup("아이템 데이터/제작", ExtendedColor.White)]
        public bool Craftable;
        public RecipeData Recipe;

        public int ItemID => itemID;
        public int ID => itemID;

#if UNITY_EDITOR
        #region EDITOR SETTER
        public void Editor_SetID(int id) => itemID = id;
        #endregion
#endif
    }
}
