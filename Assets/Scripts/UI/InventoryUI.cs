using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using ProjectRaid.Core;
using ProjectRaid.Runtime;
using ProjectRaid.EditorTools;
using DG.Tweening;

/// <summary>
/// BaseUI 시스템을 따르는 인벤토리 메인 패널
/// </summary>
public class InventoryUI : BaseUI, IBlockingUI
{
    private const int SLOTS_PER_CATEGORY = 50; // 10×5
    
    // ===========================================================================
    #region Inspector
    [FoldoutGroup("Prefabs & Layout", ExtendedColor.White)]
    [SerializeField] private InventorySlotUI slotPrefab;
    [SerializeField] private Transform equipmentGrid;
    [SerializeField] private Transform consumableGrid;
    [SerializeField] private Transform miscGrid;
    [SerializeField] private Transform cosmeticGrid;

    [FoldoutGroup("Category Buttons", ExtendedColor.Silver)]
    [SerializeField] private Button equipmentBtn;
    [SerializeField] private Button consumableBtn;
    [SerializeField] private Button miscBtn;
    [SerializeField] private Button cosmeticBtn;
    #endregion

    private readonly Dictionary<ItemType, InventorySlotUI[]> slotUIs = new();
    private ItemType currentCategory = ItemType.Equipment;

    private InventorySystem Inventory => GameManager.Instance.Inventory;
    private ItemDatabase ItemDB => GameManager.Instance.Database;

    public bool BlocksGameplay => true;

    // ===========================================================================
    #region BaseUI overrides
    public override void Initialize()
    {
        base.Initialize();
        DOTween.Init();
        CreateSlots();
        BindButtons();
    }

    public override void OnShow()
    {
        base.OnShow();
        ShowCategory(ItemType.Equipment);
        SubscribeEvents(true);
    }

    public override void OnHide()
    {
        base.OnHide();
        SubscribeEvents(false);
    }
    #endregion

    // ===========================================================================
    #region Initialization helpers
    private void CreateSlots()
    {
        slotUIs[ItemType.Equipment]     = SpawnSlots(equipmentGrid);
        slotUIs[ItemType.Consumable]    = SpawnSlots(consumableGrid);
        slotUIs[ItemType.Misc]          = SpawnSlots(miscGrid);
        slotUIs[ItemType.Cosmetic]      = SpawnSlots(cosmeticGrid);
    }

    private InventorySlotUI[] SpawnSlots(Transform parent)
    {
        ClearChildren(parent);

        var arr = new InventorySlotUI[SLOTS_PER_CATEGORY];
        for (int i = 0; i < SLOTS_PER_CATEGORY; i++)
        {
            var ui = Instantiate(slotPrefab, parent);
            ui.Index = i;
            arr[i] = ui;
        }
        return arr;
    }

    private void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; --i)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(parent.GetChild(i).gameObject);
            else
#endif
                Destroy(parent.GetChild(i).gameObject);
        }
    }

    private void BindButtons()
    {
        if (equipmentBtn    != null) equipmentBtn.onClick.AddListener(()    => ShowCategory(ItemType.Equipment));
        if (consumableBtn   != null) consumableBtn.onClick.AddListener(()   => ShowCategory(ItemType.Consumable));
        if (miscBtn         != null) miscBtn.onClick.AddListener(()         => ShowCategory(ItemType.Misc));
        if (consumableBtn   != null) cosmeticBtn.onClick.AddListener(()     => ShowCategory(ItemType.Cosmetic));
    }
    #endregion

    // ===========================================================================
    #region Event Wiring
    private void SubscribeEvents(bool sub)
    {
        if (Inventory == null) return;
        if (sub) Inventory.OnSlotChanged += HandleSlotChanged;
        else Inventory.OnSlotChanged -= HandleSlotChanged;
    }
    #endregion

    // ===========================================================================
    #region Category & Refresh
    private void ShowCategory(ItemType cat)
    {
        currentCategory = cat;
        equipmentGrid.gameObject.SetActive(cat  == ItemType.Equipment);
        consumableGrid.gameObject.SetActive(cat == ItemType.Consumable);
        miscGrid.gameObject.SetActive(cat       == ItemType.Misc);
        cosmeticGrid.gameObject.SetActive(cat   == ItemType.Cosmetic);
        ForceRefresh(cat);
    }

    private void ForceRefresh(ItemType cat)
    {
        if (!slotUIs.TryGetValue(cat, out var arr)) return;
        for (int i = 0; i < SLOTS_PER_CATEGORY; i++)
        {
            var stack = Inventory.GetItemStack(cat, i);
            arr[i].Refresh(stack, ItemDB);
        }
    }

    private void HandleSlotChanged(ItemType type, int idx, ItemStack stack)
    {
        if (!slotUIs.TryGetValue(type, out var arr)) return;
        arr[idx].Refresh(stack, ItemDB);
    }
    #endregion
}
