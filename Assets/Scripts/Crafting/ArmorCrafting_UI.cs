using ProjectRaid.Core;
using ProjectRaid.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArmorCrafting_UI : BaseUI, IArmorCraftingView, IBlockingUI
{
    [Header("카테고리 탭 (방어구)")]
    [SerializeField] private Button helmetTabButton;
    [SerializeField] private Button topTabButton;
    [SerializeField] private Button bottomTabButton; 
    [SerializeField] private Button glovesTabButton; 
    [SerializeField] private Button shoesTabButton;  
    // 필요한 만큼 다른 방어구 카테고리 버튼 추가

    [Header("제작 목록")]
    [SerializeField] private ScrollRect craftableListScrollView;
    [SerializeField] private Transform craftableListContent;
    [SerializeField] private CraftableItemSlot_UI craftableItemSlotPrefab; // 동일한 슬롯 UI 프리팹 사용 가능

    [Header("상세 정보")]
    [SerializeField] private GameObject detailsPanel;
    [SerializeField] private TextMeshProUGUI armorNameText; // 이름 변경 (weaponNameText -> armorNameText)
    [SerializeField] private Image armorIconImage;    // 이름 변경 (weaponIconImage -> armorIconImage)
    // [SerializeField] private Image armorPreviewImage; // 필요 시
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Transform requiredMaterialsContent;
    [SerializeField] private RequiredMaterialSlot_UI requiredMaterialSlotPrefab; // 동일한 재료 슬롯 UI 프리팹 사용 가능
    [SerializeField] private Transform statsContent;
    [SerializeField] private TextMeshProUGUI statTextPrefab; // 동일한 스탯 텍스트 프리팹 사용 가능
    [SerializeField] private Button craftButton;
    [SerializeField] private Button closeButton;

    [Header("팝업 및 메시지")]
    [SerializeField] private TextMeshProUGUI messageText; // WeaponCrafting_UI와 동일
    [SerializeField] private float messageDuration = 2f;  // WeaponCrafting_UI와 동일

    private ArmorCraftingPresenter _presenter; // Presenter 타입 변경
    private readonly List<CraftableItemSlot_UI> _activeCraftableSlots = new List<CraftableItemSlot_UI>();
    private readonly List<RequiredMaterialSlot_UI> _activeMaterialSlots = new List<RequiredMaterialSlot_UI>();
    private readonly List<TextMeshProUGUI> _activeStatTexts = new List<TextMeshProUGUI>();
    private Coroutine _messageCoroutine;

    // 시스템 참조 (Initialize에서 GameManager를 통해 할당)
    private ICraftingSystem _craftingSystemRef;
    private InventorySystem _inventorySystemRef; // 실제 InventorySystem 타입

    // --- IArmorCraftingView 이벤트 구현 ---
    public event Action<ArmorClass> OnCategorySelected;
    public event Action<RecipeData> OnRecipeSelected;   
    public event Action OnCraftButtonPressed;
    public event Action OnConfirmCraft;
    public event Action OnCancelCraft;

    public bool BlocksGameplay => true; // UI가 게임 플레이를 막는지 여부
        

    public override void Initialize()
    {
        if (IsInitialized) return;
        // base.Initialize(); // BaseUI의 Initialize는 IsInitialized 플래그만 설정하므로, 여기서 직접 관리

        Debug.Log($"[{gameObject.name}] Initializing ArmorCrafting_UI...");

        bool hasError = false;

        // GameManager를 통해 시스템 참조 가져오기
        if (GameManager.Instance == null)
        {
            Debug.LogError($"[{gameObject.name}] GameManager.Instance is null! Cannot get system references.", this);
            hasError = true;
        }
        else
        {
            _craftingSystemRef = GameManager.Instance.Crafting;
            _inventorySystemRef = GameManager.Instance.Inventory; // 실제 InventorySystem 참조

            if (_craftingSystemRef == null)
            {
                Debug.LogError($"[{gameObject.name}] CraftingSystem reference not found via GameManager!", this);
                hasError = true;
            }
            if (_inventorySystemRef == null)
            {
                Debug.LogError($"[{gameObject.name}] InventorySystem reference not found via GameManager!", this);
                hasError = true;
            }
        }

        if (hasError)
        {
            gameObject.SetActive(false); // 오류 시 UI 비활성화
            enabled = false; // 스크립트 비활성화
            return;
        }

        try
        {
            _presenter = new ArmorCraftingPresenter(this, _craftingSystemRef, _inventorySystemRef);
        }
        catch (Exception e)
        {
            Debug.LogError($"[{gameObject.name}] Error creating ArmorCraftingPresenter: {e.Message}\n{e.StackTrace}", this);
            gameObject.SetActive(false);
            enabled = false;
            return;
        }

        // 버튼 리스너 연결
        helmetTabButton?.onClick.RemoveAllListeners();
        helmetTabButton?.onClick.AddListener(() => OnCategorySelected?.Invoke(ArmorClass.Helmet));
        topTabButton?.onClick.RemoveAllListeners();
        topTabButton?.onClick.AddListener(() => OnCategorySelected?.Invoke(ArmorClass.Top));
        bottomTabButton?.onClick.RemoveAllListeners();
        bottomTabButton?.onClick.AddListener(() => OnCategorySelected?.Invoke(ArmorClass.Bottom));
        glovesTabButton?.onClick.RemoveAllListeners();
        glovesTabButton?.onClick.AddListener(() => OnCategorySelected?.Invoke(ArmorClass.Gloves));
        shoesTabButton?.onClick.RemoveAllListeners();
        shoesTabButton?.onClick.AddListener(() => OnCategorySelected?.Invoke(ArmorClass.Shoes));

        craftButton?.onClick.RemoveAllListeners();
        craftButton?.onClick.AddListener(() => OnCraftButtonPressed?.Invoke());
        closeButton?.onClick.RemoveAllListeners();
        closeButton?.onClick.AddListener(Close); // BaseUI의 Close 메서드 호출

        if (messageText != null) messageText.gameObject.SetActive(false);

        IsInitialized = true; // 초기화 완료 플래그 설정
        Debug.Log($"<color=lightblue>[{gameObject.name}] Initialize_ArmorCrafting_UI 완료. isInitialized: {IsInitialized}</color>");
    }

    public override void OnShow()
    {
        base.OnShow(); // BaseUI의 OnShow 호출 (GameObject 활성화, CanvasGroup 설정 등)
        Debug.Log($"<color=green>[{gameObject.name}] OnShow 호출됨 (ArmorCrafting_UI).</color>");

        if (!IsInitialized) Initialize(); // 아직 초기화되지 않았다면 초기화 시도

        _presenter?.InitializeView(ArmorClass.Helmet); // 기본으로 헬멧 카테고리 표시
        SubscribeToEvents();
        CursorManager.SetCursorState(true); // UI 표시 시 커서 보이기
    }

    public override void OnHide()
    {
        Debug.Log($"[{gameObject.name}] OnHide 실행 (ArmorCrafting_UI).");
        UnsubscribeFromEvents();

        // _presenter?.Dispose(); // Presenter에 Dispose가 있다면 호출

        if (_messageCoroutine != null)
        {
            StopCoroutine(_messageCoroutine);
            _messageCoroutine = null;
            if (messageText != null) messageText.gameObject.SetActive(false);
        }
        CursorManager.SetCursorState(false); // UI 숨길 때 커서 숨기기/잠금
        base.OnHide(); // BaseUI의 OnHide 호출 (GameObject 비활성화 등)
    }

    private void SubscribeToEvents()
    {
        if (_inventorySystemRef != null)
        {
            // InventorySystem의 이벤트 시그니처에 맞게 수정 필요
            // 예: _inventorySystemRef.OnSlotChanged += UpdateUIBasedOnInventory;
            _inventorySystemRef.OnSlotChanged += UpdateUIBasedOnInventory; // WeaponCrafting_UI의 이벤트 핸들러와 동일하게 단순 버전 사용 가정
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (_inventorySystemRef != null)
        {
            //_inventorySystemRef.OnSlotChanged -= UpdateUIBasedOnInventory;
            _inventorySystemRef.OnSlotChanged -= UpdateUIBasedOnInventory;
        }
    }

    // --- IArmorCraftingView 메서드 구현 ---
    private void UpdateUIBasedOnInventory(ItemType type, int slotIndex, ItemStack stack)
    {
        if (gameObject.activeInHierarchy && _presenter != null)
        {
            Debug.Log($"[{gameObject.name}] 인벤토리 슬롯 변경 감지 (Type: {type}, Index: {slotIndex}). UI 갱신.");
            _presenter.RefreshCurrentCategoryView(); // Presenter에게 현재 카테고리 UI 갱신 요청
        }
    }

    public void SetAvailableCategories(List<ArmorClass> categories)
    {
        // 이 메서드는 현재 Presenter에서 호출되지 않지만, 필요하다면 구현
        // 예: 특정 조건에 따라 카테고리 탭 버튼을 동적으로 활성화/비활성화
        Debug.LogWarning($"[{gameObject.name}] SetAvailableCategories는 아직 구현되지 않았습니다.");
    }

    public void DisplayCraftableRecipes(List<RecipeViewModel> recipes)
    {
        // 기존 슬롯들 정리
        foreach (var slot in _activeCraftableSlots)
        {
            if (slot != null) Destroy(slot.gameObject);
        }
        _activeCraftableSlots.Clear();

        if (craftableItemSlotPrefab == null || craftableListContent == null || recipes == null)
        {
            Debug.LogError($"[{gameObject.name}] DisplayCraftableRecipes 실패: 필수 참조 또는 recipes 리스트가 null입니다.");
            return;
        }

        Debug.Log($"[{gameObject.name}] DisplayCraftableRecipes 호출. 레시피 수: {recipes.Count}");
        foreach (var vm in recipes)
        {
            if (vm == null || vm.Recipe == null)
            {
                Debug.LogWarning($"[{gameObject.name}] ViewModel 또는 ViewModel.Recipe가 null입니다. 스킵.");
                continue;
            }
            CraftableItemSlot_UI slot = Instantiate(craftableItemSlotPrefab, craftableListContent);
            slot.Setup(vm.Recipe, (selectedRecipe) => OnRecipeSelected?.Invoke(selectedRecipe));
            slot.UpdateCraftabilityVisuals(vm.CanCraft);
            _activeCraftableSlots.Add(slot);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(craftableListContent as RectTransform);
        if (craftableListScrollView != null)
        {
            craftableListScrollView.verticalNormalizedPosition = 1f; // 맨 위로 스크롤
        }
    }

    public void DisplaySelectedRecipeDetails(RecipeDetailsViewModel details)
    {
        if (detailsPanel == null || armorNameText == null || armorIconImage == null || descriptionText == null ||
            requiredMaterialsContent == null || requiredMaterialSlotPrefab == null || statsContent == null || statTextPrefab == null)
        {
            Debug.LogError($"[{gameObject.name}] DisplaySelectedRecipeDetails 실패: 상세 정보 UI 요소 중 일부가 할당되지 않았습니다.");
            if (detailsPanel != null) detailsPanel.SetActive(false);
            return;
        }

        if (details == null)
        {
            detailsPanel.SetActive(false);
            return;
        }

        detailsPanel.SetActive(true);
        armorNameText.text = details.Name;
        armorIconImage.sprite = details.Icon;
        armorIconImage.enabled = details.Icon != null;
        descriptionText.text = details.Description;

        PopulateRequiredMaterials(details.RequiredMaterials);
        UpdateStatsDisplay(details.Stats);
    }

    private void PopulateRequiredMaterials(List<MaterialViewModel> materials)
    {
        foreach (var slot in _activeMaterialSlots)
        {
            if (slot != null) Destroy(slot.gameObject);
        }
        _activeMaterialSlots.Clear();

        if (materials == null) return;

        foreach (var vm in materials)
        {
            if (vm == null) continue;
            RequiredMaterialSlot_UI slot = Instantiate(requiredMaterialSlotPrefab, requiredMaterialsContent);
            // MaterialViewModel의 MaterialItem이 ItemData를 직접 참조한다고 가정
            slot.Setup(vm.MaterialItem?.ItemID ?? 0, vm.MaterialItem?.Icon, vm.MaterialItem?.DisplayNameKey ?? "재료 이름 없음", vm.RequiredCount, vm.OwnedCount);
            _activeMaterialSlots.Add(slot);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(requiredMaterialsContent as RectTransform);
    }

    private void UpdateStatsDisplay(List<StatViewModel> stats)
    {
        foreach (var textGO in _activeStatTexts)
        {
            if (textGO != null) Destroy(textGO.gameObject);
        }
        _activeStatTexts.Clear();

        if (stats == null) return;

        foreach (var vm in stats)
        {
            if (vm == null) continue;
            TextMeshProUGUI statText = Instantiate(statTextPrefab, statsContent);
            statText.text = $"{vm.Name}: {vm.ValueString}";
            _activeStatTexts.Add(statText);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(statsContent as RectTransform);
    }

    public void UpdateCraftButtonInteractable(bool interactable)
    {
        if (craftButton != null) craftButton.interactable = interactable;
    }

    public void ShowConfirmationPopup(string message)
    {
        var popupInstance = UIManager.Instance?.ShowUI<ConfirmationPopup_UI>(true); // isPopup = true
        if (popupInstance != null)
        {
            popupInstance.SetMessage(message);
            popupInstance.SetupActions(
                () => OnConfirmCraft?.Invoke(),
                () => OnCancelCraft?.Invoke()
            );
        }
        else
        {
            Debug.LogError($"[{gameObject.name}] ConfirmationPopup_UI를 열 수 없습니다! UIManager 또는 프리팹 설정을 확인하세요.");
            // 팝업 없이 바로 제작 시도하거나, 사용자에게 오류 메시지 표시 가능
            // OnConfirmCraft?.Invoke(); // 임시: 팝업 실패 시 바로 확인으로 간주 (테스트용)
        }
    }

    public void HideConfirmationPopup()
    {
        UIManager.Instance?.HideUI<ConfirmationPopup_UI>();
    }

    public void ShowMessage(string message)
    {
        if (messageText == null) return;
        if (_messageCoroutine != null) StopCoroutine(_messageCoroutine);
        messageText.text = message;
        messageText.gameObject.SetActive(true);
        _messageCoroutine = StartCoroutine(ShowMessageCoroutineInternal());
    }

    private IEnumerator ShowMessageCoroutineInternal()
    {
        yield return new WaitForSeconds(messageDuration);
        if (messageText != null) messageText.gameObject.SetActive(false);
        _messageCoroutine = null;
    }

    public void HighlightSelectedSlot(RecipeData recipeToSelect) // 파라미터 타입 RecipeData로 변경
    {
        foreach (var slot in _activeCraftableSlots)
        {
            if (slot != null)
            {
                // CraftableItemSlot_UI의 AssociatedRecipe 타입이 RecipeData여야 함
                slot.SetSelected(slot.AssociatedRecipe == recipeToSelect);
            }
        }
    }

    // 인벤토리 변경 시 호출될 메서드 (WeaponCrafting_UI와 동일한 단순 버전 가정)
    // 실제 InventorySystem.OnSlotChanged 이벤트 시그니처에 맞춰 파라미터 추가 필요
    private void UpdateUIBasedOnInventory_Simple()
    {
        if (gameObject.activeInHierarchy && _presenter != null)
        {
            Debug.Log($"[{gameObject.name}] 인벤토리 변경 감지. UI 갱신.");
            _presenter.RefreshCurrentCategoryView();
        }
    }
}
