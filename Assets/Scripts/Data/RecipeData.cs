using System.Collections.Generic;
using UnityEngine;
using ProjectRaid.Core;
using ProjectRaid.EditorTools;

namespace ProjectRaid.Data
{
    [System.Serializable]
    public struct MaterialRequirement
    {
        public int ItemID;
        public int Quantity;

        public MaterialRequirement(int id, int qty)
        {
            ItemID = id;
            Quantity = qty;
        }

        public override readonly string ToString() => $"{ItemID}:{Quantity}";
    }

    [CreateAssetMenu(fileName = "Recipe", menuName = "Data/Recipe")]
    public class RecipeData : ScriptableObject, IHasID
    {
        [FoldoutGroup("레시피 데이터", ExtendedColor.White)]
        [SerializeField] private int recipeID;

        public ItemData ResultItem;
        [Min(1)] public int ResultCount = 1;

        [Header("제작 조건 (필수)")]
        public List<MaterialRequirement> RequiredMaterials = new();
        public int RequireGold;

        [Header("제작 조건 (선택)")]
        public bool RequiresSpecificStation = false;
        [ShowIf(nameof(RequiresSpecificStation), true)]
        public string RequiredStationTag = "WeaponWorkbench";

        [Header("정렬 순서 (선택)")]
        public int SortOrder = 0;

        public int RecipeID => recipeID;
        public int ID => recipeID;

        public bool CanBeCraftedAt(GameObject station)
        {
            if (!RequiresSpecificStation) return true;
            return station != null && station.CompareTag(RequiredStationTag);
        }

#if UNITY_EDITOR
        #region EDITOR SETTER
        public void Editor_SetID(int id) => recipeID = id;
        #endregion
#endif
    }
}
