using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectRaid.Core;
using ProjectRaid.Data;

/// <summary>
/// 슬롯 하나가 보유한 스택.
/// itemId == 0이면 빈 슬롯으로 간주
/// </summary>
[Serializable]
public struct ItemStack
{
    public int itemID;
    public int quantity;
}

/// <summary>
/// 플레이어 인벤토리 컴포넌트
/// • 분류(장비/소비/기타/치장)별 10×5 슬롯(총 50) 관리
/// • 아이템 획득/소비/버리기/이동 등 슬롯 갱신 로직 담당
/// • 제작/갈무리 등 외부 시스템에서 수량·여유 공간 조회 가능하도록 public API 제공
/// • UI 쪽에서는 슬롯 변경 이벤트(OnSlotChanged) 구독하여 내용 갱신
/// </summary>
public class InventorySystem : MonoBehaviour
{
    private const int COLUMNS = 10;
    private const int ROWS = 5;
    private const int SLOTS_PER_CATEGORY = COLUMNS * ROWS;

    // 카테고리별 슬롯 배열
    private readonly Dictionary<ItemType, ItemStack[]> slots = new();

    /// <summary>
    /// 슬롯 변경(추가/제거/스택수 변동)시 호출
    /// </summary>
    public event Action<ItemType, int, ItemStack> OnSlotChanged;

    #region LIFECYCLE
    private void Awake()
    {
        // 카테고리별 슬롯 초기화
        foreach (ItemType cat in Enum.GetValues(typeof(ItemType)))
        {
            slots[cat] = new ItemStack[SLOTS_PER_CATEGORY];
        }
    }
    #endregion

    #region PUBLIC-API (조회)
    /// <summary>
    /// 특정 카테고리 인벤토리가 가득 찼는지 확인
    /// </summary>
    public bool IsCategoryFull(ItemType cat)
    {
        ItemStack[] arr = slots[cat];
        for (int i = 0; i < SLOTS_PER_CATEGORY; ++i)
        {
            if (arr[i].itemID == 0) return false; // 빈 칸이 존재
        }
        return true;
    }

    /// <summary>아이템 ID로 현재 총 보유 개수 반환</summary>
    public int GetItemCount(int itemId)
    {
        int count = 0;
        foreach (var arr in slots.Values)
        {
            foreach (var stack in arr)
            {
                if (stack.itemID == itemId) count += stack.quantity;
            }
        }
        return count;
    }

    /// <summary>
    /// 해당 아이템을 추가할 여유 공간이 충분한지 확인
    /// </summary>
    public bool CanAdd(ItemData data, int amount)
    {
        ItemType type = data.ItemType;
        int toAdd = amount;
        ItemStack[] arr = slots[type];
        int maxStack = data.MaxStack;

        // 1) 기존 스택 채워넣기
        for (int i = 0; i < SLOTS_PER_CATEGORY && toAdd > 0; ++i)
        {
            if (arr[i].itemID == data.ItemID && arr[i].quantity < maxStack)
            {
                int space = maxStack - arr[i].quantity;
                int used = Mathf.Min(space, toAdd);
                toAdd -= used;
            }
        }
        // 2) 빈 슬롯 확보
        int emptySlots = 0;
        for (int i = 0; i < SLOTS_PER_CATEGORY && toAdd > 0; ++i)
        {
            if (arr[i].itemID == 0)
            {
                int used = Mathf.Min(maxStack, toAdd);
                toAdd -= used;
                emptySlots++;
            }
        }
        
        return toAdd == 0; // 모두 수용 가능하면 true
    }

    /// <summary>
    /// 해당 아이템을 소비할 개수가 충분한지 확인
    /// </summary>
    public bool CanRemove(int itemID, int amount)
    {
        int available = GetItemCount(itemID);
        return available >= amount;
    }

    public bool CanRemove(List<MaterialRequirement> requirements)
    {
        Dictionary<int, int> totalRequirements = new();

        // 1. 같은 ItemID끼리 수량 합치기
        foreach (var req in requirements)
        {
            if (totalRequirements.ContainsKey(req.ItemID))
                totalRequirements[req.ItemID] += req.Quantity;
            else
                totalRequirements[req.ItemID] = req.Quantity;
        }

        // 2. 제거 가능한지 먼저 확인
        foreach (var kv in totalRequirements)
        {
            if (!CanRemove(kv.Key, kv.Value)) return false;
        }

        return true;
    }
    #endregion

    #region PUBLIC-API (조작)
    /// <summary>
    /// 인벤토리에 아이템 추가 (ID) / 
    /// 성공 여부 반환(공간 부족 시 false)
    /// </summary>
    public bool TryAdd(int id, int amount = 1)
    {
        if (!GameManager.Instance.Database.TryGetItem(id, out var data)) return false;
        return TryAdd(data, amount);
    }

    /// <summary>
    /// 인벤토리에 아이템 추가 (ItemData) / 
    /// 성공 여부 반환(공간 부족 시 false)
    /// </summary>
    public bool TryAdd(ItemData data, int amount = 1)
    {
        if (!CanAdd(data, amount)) return false;

        ItemType type = data.ItemType;
        ItemStack[] arr = slots[type];
        int maxStack = data.MaxStack;
        int remaining = amount;

        // 1) 같은 스택 채우기
        for (int i = 0; i < SLOTS_PER_CATEGORY && remaining > 0; ++i)
        {
            if (arr[i].itemID == data.ItemID && arr[i].quantity < maxStack)
            {
                int space = maxStack - arr[i].quantity;
                int add = Mathf.Min(space, remaining);
                arr[i].quantity += add;
                remaining -= add;
                OnSlotChanged?.Invoke(type, i, arr[i]);
            }
        }
        // 2) 빈 슬롯에 새 스택 생성
        for (int i = 0; i < SLOTS_PER_CATEGORY && remaining > 0; ++i)
        {
            if (arr[i].itemID == 0)
            {
                int add = Mathf.Min(maxStack, remaining);
                arr[i].itemID = data.ItemID;
                arr[i].quantity = add;
                remaining -= add;
                OnSlotChanged?.Invoke(type, i, arr[i]);
            }
        }
        return true;
    }

    public bool TryRemove(List<MaterialRequirement> requirements)
    {
        if (!CanRemove(requirements)) return false;

        foreach (var req in requirements)
        {
            TryRemove(req.ItemID, req.Quantity);
        }

        return true;
    }

    public bool TryRemove(ItemData data, int amount = 1)
    {
        return TryRemove(data.ID, amount);
    }

    /// <summary>
    /// 인벤토리에서 특정 아이템을 주어진 수량만큼 제거. 부족 시 false
    /// </summary>
    public bool TryRemove(int itemId, int amount = 1)
    {
        int available = GetItemCount(itemId);
        if (available < amount) return false;

        int remaining = amount;
        foreach (var kv in slots)
        {
            ItemType type = kv.Key;
            ItemStack[] arr = kv.Value;
            for (int i = 0; i < SLOTS_PER_CATEGORY && remaining > 0; ++i)
            {
                if (arr[i].itemID != itemId) continue;
                int remove = Mathf.Min(arr[i].quantity, remaining);
                arr[i].quantity -= remove;
                remaining -= remove;

                // 스택이 비면 슬롯 초기화
                if (arr[i].quantity == 0)
                {
                    arr[i].itemID = 0;
                }
                OnSlotChanged?.Invoke(type, i, arr[i]);
            }
        }
        return true;
    }

    /// <summary>
    /// 슬롯 내용을 직접 비우기(버리기/창고보관 용도)
    /// </summary>
    public void ClearSlot(ItemType type, int index)
    {
        if (index < 0 || index >= SLOTS_PER_CATEGORY) return;
        slots[type][index] = default;
        OnSlotChanged?.Invoke(type, index, default);
    }
    #endregion

    #region HELPER
    /// <summary>
    /// 갈무리 전 체크: 소비·기타 카테고리 각각 1칸 이상 빈칸이 있는지 반환
    /// </summary>
    public bool HasHarvestSpace() => !IsCategoryFull(ItemType.Consumable) && !IsCategoryFull(ItemType.Misc);

    /// <summary>
    /// 특정 인덱스의 아이템 스택 정보 반환
    /// </summary>
    public ItemStack GetItemStack(ItemType type, int index) => slots[type][index];
    #endregion
}