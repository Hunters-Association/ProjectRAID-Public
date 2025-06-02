using System;
using System.Collections.Generic;
using ProjectRaid.Core;
using ProjectRaid.Data;
public interface IInventorySystem
{
    event Action<ItemType, int, ItemStack> OnSlotChanged;
    event Action OnInventoryChanged;
    int GetItemCount(int itemID);
    bool ConsumeMaterials(List<MaterialRequirement> requiredMaterials);
    void AddItem(ItemData itemData, int count = 1);
    bool HasMaterials(List<MaterialRequirement> requiredMaterials);
}
