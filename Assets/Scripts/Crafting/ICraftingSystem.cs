using System.Collections.Generic;
using UnityEngine;
using ProjectRaid.Data;
using ProjectRaid.Core; // WeaponClass 사용 위해

public interface ICraftingSystem
{
    bool TryCraftItem(RecipeData recipe);
    bool CanCraft(RecipeData recipe);
    List<RecipeData> GetRecipesByWeaponClass(WeaponClass type); // WeaponClass 사용
    List<RecipeData> GetRecipesByArmorClass(ArmorClass type);
    ItemData GetItemData(int id);
    bool IsRecipeDatabaseReady();
    // void SetCurrentCraftingStation(GameObject station);
}
