using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using ProjectRaid.Data;
using ProjectRaid.Core;
using ProjectRaid.Runtime;

public class CraftingSystem : MonoBehaviour, ICraftingSystem
{
    private InventorySystem inventory;
    private ItemDatabase itemDatabase;

    private bool isInitialized = false;

    public void InitializeSystem(InventorySystem inven, ItemDatabase database)
    {
        if (isInitialized) return;
        inventory = inven;
        itemDatabase = database;

        if (inventory == null) { /* 오류 처리 */ enabled = false; return; }
        if (!itemDatabase.IsInitialized) // ItemDatabase 초기화 확인
        {
            Debug.LogError("[CraftingSystem] ItemDatabase가 초기화되지 않았습니다!");
            // 필요 시 여기서 ItemDatabase 초기화를 기다리는 로직 추가 가능
            enabled = false; return;
        }
        
        isInitialized = true;
        // Debug.Log("[CraftingSystem] 초기화 성공");
    }

    public bool TryCraftItem(RecipeData recipe)
    {
        if (!isInitialized || recipe == null || recipe.ResultItem == null || !CanCraft(recipe)) return false;

        // ★★★★ 실제 인벤토리 연동 ★★★★
        if (inventory.TryRemove(recipe.RequiredMaterials))
        {
            inventory.TryAdd(recipe.ResultItem, recipe.ResultCount); // ItemData 객체 전달
            Debug.Log($"<color=green>'{recipe.ResultItem.DisplayNameKey}' 제작 성공!</color>");
            return true;
        }
        // ★★★★★★★★★★★★★★★★★★★★
        return false;
    }

    public bool CanCraft(RecipeData recipe)
    {
        if (!isInitialized || recipe == null || recipe.RequiredMaterials == null || inventory == null) return false;
        return inventory.CanRemove(recipe.RequiredMaterials);
    }

    // WeaponClass 사용
    public List<RecipeData> GetRecipesByWeaponClass(WeaponClass type)
    {
        Debug.Log($"[CraftingSystem] GetRecipesByWeaponClass({type}) 호출. RecipeDatabase 참조 유효: {itemDatabase != null}");
        if (!itemDatabase.IsInitialized)
        {
            Debug.LogWarning($"[CraftingSystem] RecipeDatabase가 아직 초기화되지 않았습니다!");
        }

        var itemList = itemDatabase.GetItemListByWeaponClass(type)
            .Where(x => x.Craftable == true && x.Recipe != null);
        var recipeList = new List<RecipeData>();

        foreach (var item in itemList)
        {
            recipeList.Add(item.Recipe);
        }

        return recipeList;
    }
    public List<RecipeData> GetRecipesByArmorClass(ArmorClass type)
    {
        Debug.Log($"[CraftingSystem] GetRecipesByArmorClass({type}) 호출.");
        if (!itemDatabase.IsInitialized) // ItemDatabase 참조 (GameManager 통해 주입됨)
        {
            Debug.LogWarning($"[CraftingSystem] ItemDatabase가 아직 초기화되지 않았습니다!");
            return new List<RecipeData>();
        }

        // ItemDatabase에서 ArmorClass에 해당하는 ItemData 목록을 가져옴
        var itemList = itemDatabase.GetItemListByArmorClass(type);
            // ★★★ 디버깅 로그 추가 ★★★
    Debug.Log($"[CraftingSystem] GetItemListByArmorClass({type}) 결과 아이템 개수: {itemList?.Count ?? -1}");
        if (itemList != null)
        {
            foreach (var item in itemList)
            {
                Debug.Log($"  - 아이템: {item?.DisplayNameKey ?? "N/A"}, 제작가능: {item?.Craftable}, 레시피: {item?.Recipe?.name ?? "N/A"}");
            }
        }
        var filteredItemList = itemList?.Where(item => item.Craftable && item.Recipe != null);
        Debug.Log($"[CraftingSystem] 필터링 후 아이템 개수: {filteredItemList?.Count() ?? -1}");

        var recipeList = new List<RecipeData>();
        if (filteredItemList != null) // Null 체크 추가
        {
            // ★★★★★★★★★★★★★★★★★★★★★★★★
            //      여기서 filteredItemList를 사용해야 합니다!
            // ★★★★★★★★★★★★★★★★★★★★★★★★
            foreach (var item in filteredItemList) // itemList -> filteredItemList 로 변경
            {
                if (item.Recipe != null) // 한 번 더 Recipe null 체크 (안전 장치)
                {
                    recipeList.Add(item.Recipe);
                }
                else
                {
                    // 이 경우는 filteredItemList 조건에 의해 발생하지 않아야 하지만, 방어 코드로 추가
                    Debug.LogWarning($"[CraftingSystem] 필터링된 아이템 '{item.DisplayNameKey}'의 Recipe가 null입니다. 스킵.");
                }
            }
        }
        // 결과 레시피 개수 로그 추가
        Debug.Log($"[CraftingSystem] GetRecipesByArmorClass({type}) 최종 반환 레시피 개수: {recipeList.Count}");
        return recipeList;
    }

    public bool IsRecipeDatabaseReady()
    {
        // itemDatabase 참조가 있고, 해당 데이터베이스가 초기화되었는지 확인
        return itemDatabase != null && itemDatabase.IsInitialized;
    }

    // ItemDatabase 싱글톤 사용
    public ItemData GetItemData(int id) => itemDatabase.GetItem(id);
}
