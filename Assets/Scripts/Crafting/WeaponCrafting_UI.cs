using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using System;
using ProjectRaid.Data; // 네임스페이스 사용
using ProjectRaid.Core;
using System.Collections;
using Unity.VisualScripting;
using ProjectRaid.Runtime;

public class WeaponCrafting_UI : BaseUI, IWeaponCraftingView, IBlockingUI
{
    [Header("카테고리 탭")]
    // ★ 버튼 참조 유지, 리스너에서 WeaponClass 전달 ★
    [SerializeField]
    private Button rifleTabButton;
    [SerializeField]
    private Button swordTabButton;
    [SerializeField]
    private Button lanceTabButton;
    [SerializeField]
    private Button gauntletTabButton;

    // ...

    [Header("제작 목록")]
    [SerializeField] private ScrollRect craftableListScrollView;
    [SerializeField] private Transform craftableListContent;
    [SerializeField] private CraftableItemSlot_UI craftableItemSlotPrefab;

    [Header("상세 정보")]
    [SerializeField] private GameObject detailsPanel;
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private Image weaponIconImage;
    // [SerializeField] private Image weaponPreviewImage; // 필요 시
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Transform requiredMaterialsContent;
    [SerializeField] private RequiredMaterialSlot_UI requiredMaterialSlotPrefab;
    [SerializeField] private Transform statsContent;
    [SerializeField] private TextMeshProUGUI statTextPrefab; // 스탯 표시용
    [SerializeField] private Button craftButton;
    [SerializeField] private Button closeButton;

    [Header("팝업 및 메시지")]
    //[SerializeField] private ConfirmationPopup_UI confirmationPopup;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private float messageDuration = 2f;

    private WeaponCraftingPresenter _presenter;
    private readonly List<CraftableItemSlot_UI> _activeCraftableSlots = new();
    private readonly List<RequiredMaterialSlot_UI> _activeMaterialSlots = new();
    private readonly List<TextMeshProUGUI> _activeStatTexts = new(); // 스탯 텍스트 풀링/관리
    private Coroutine _messageCoroutine;

    private ICraftingSystem _craftingSystemRef;
    private InventorySystem _inventorySystemRef;

    // --- IWeaponCraftingView 이벤트 구현 ---
    public event Action<WeaponClass> OnCategorySelected; // WeaponClass 사용
    public event Action<RecipeData> OnRecipeSelected;
    public event Action OnCraftButtonPressed;
    public event Action OnConfirmCraft;
    public event Action OnCancelCraft;
    //public event Action OnCloseButtonPressed;

    public bool BlocksGameplay => true;

    public override void Initialize()
    {

        if (IsInitialized) return;
        // base.Initialize(); // 마지막에 IsInitialized = true 설정

        Debug.Log($"[{gameObject.name}] Initializing...");

        // --- 코드를 통해 시스템 참조 찾기 (ItemDatabase 싱글톤 경유) ---
        bool hasError = false;

        // 1. ItemDatabase 인스턴스 확인
        if (GameManager.Instance.Database == null)
        {
            Debug.LogError("WeaponCrafting_UI Error: ItemDatabase.Instance is null! 시스템 참조를 가져올 수 없습니다.", this);
            hasError = true;
        }
        else
        {            
            _craftingSystemRef = GameManager.Instance.Crafting; // GameManager 통해 인터페이스 참조 가져오기
            _inventorySystemRef = GameManager.Instance.Inventory;                       

            // 3. 찾기 결과 확인
            if (_craftingSystemRef == null)
            {
                Debug.LogError("WeaponCrafting_UI Error: ItemDatabase 오브젝트에서 CraftingSystem 컴포넌트를 찾을 수 없습니다!", GameManager.Instance.Database.gameObject);
                hasError = true;
            }
            if (_inventorySystemRef == null)
            {
                Debug.LogError("WeaponCrafting_UI Error: ItemDatabase 오브젝트에서 InventorySystem_Placeholder 컴포넌트를 찾을 수 없습니다!", GameManager.Instance.Database.gameObject);
                hasError = true;
            }
        }
        // --- 참조 찾기 끝 ---

        if (hasError) // 필수 참조 없으면 초기화 중단
        {
            gameObject.SetActive(false);
            enabled = false;
            return;
        }

        // --- Presenter 생성 (찾은 참조 사용) ---
        try
        {
            // ★ 찾은 인터페이스 참조를 Presenter에게 전달 ★
            _presenter = new WeaponCraftingPresenter(this, _craftingSystemRef, _inventorySystemRef);
        }
        catch (System.Exception e) // Presenter 생성 중 예외 발생 시
        {
            Debug.LogError($"WeaponCrafting_UI Error: Presenter 생성 실패! \n{e}", this);
            gameObject.SetActive(false);
            enabled = false;
            return;
        }


        // --- 버튼 리스너 연결 (Initialize에서 한 번만) ---
        rifleTabButton.onClick.RemoveAllListeners(); // 중복 방지
        rifleTabButton.onClick.AddListener(() => OnCategorySelected?.Invoke(WeaponClass.Rifle));
        swordTabButton.onClick.RemoveAllListeners();
        swordTabButton.onClick.AddListener(() => OnCategorySelected?.Invoke(WeaponClass.Sword));
        lanceTabButton.onClick.RemoveAllListeners();
        lanceTabButton.onClick.AddListener(() => OnCategorySelected?.Invoke(WeaponClass.Lance));
        gauntletTabButton.onClick.RemoveAllListeners();
        gauntletTabButton.onClick.AddListener(() => OnCategorySelected?.Invoke(WeaponClass.Gauntlet));

        craftButton.onClick.RemoveAllListeners();
        craftButton.onClick.AddListener(() => OnCraftButtonPressed?.Invoke());
        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(Close); // BaseUI.Close 호출
        // ★ 팝업 콜백 설정도 Initialize에서 하는 것이 안전할 수 있음 ★
        OnCraftButtonPressed += () => Debug.Log("<color=cyan>[View] OnCraftButtonPressed 이벤트 발생!</color>");


        if (messageText != null) messageText.gameObject.SetActive(false); // 메시지 초기 숨김

        // ★ 모든 초기화 완료 후 IsInitialized 플래그 설정 ★
        IsInitialized = true; // base.Initialize() 대신 직접 설정

        Debug.Log($"<color=lightblue>[View] Initialize 완료. isInitialized: {IsInitialized}</color>");
    }

    // ★ Start, OnEnable, OnDestroy 제거 -> OnShow, OnHide 로 통합 ★

    /// <summary>
    /// UI가 표시될 때 호출됩니다. (UIManager에 의해)
    /// </summary>
    public override void OnShow()
    {
        base.OnShow(); // 필수: GameObject 활성화, CanvasGroup 설정 등
        Debug.Log($"<color=green>[View] OnShow 호출됨. 데이터 준비 확인 및 Presenter 초기화 시도.</color>");

        // Presenter 초기화 (표시될 때마다 최신 상태 로드)
        _presenter?.InitializeView();
        // 이벤트 구독
        SubscribeToEvents();

        // 기타 UI 표시 관련 로직 (예: 커서)
        // Cursor.visible = true; Cursor.lockState = CursorLockMode.None;
        CursorManager.SetCursorState(true);
    }

    /// <summary>
    /// UI가 숨겨질 때 호출됩니다. (UIManager에 의해)
    /// </summary>
    public override void OnHide()
    {
        Debug.Log($"[{gameObject.name}] OnHide 실행.");
        // 이벤트 구독 해제
        UnsubscribeFromEvents();

        // --- 현재 사용되지 않음 ---
        // Presenter 리소스 해제
        // _presenter?.Dispose(); // 이벤트 구독 해제 등 

        // 코루틴 중지 (메시지 표시 중이었다면)
        if (_messageCoroutine != null)
        {
            StopCoroutine(_messageCoroutine);
            _messageCoroutine = null;
            if (messageText != null) messageText.gameObject.SetActive(false); // 메시지 강제 숨김
        }

        // 커서 상태 복원 등
        CursorManager.SetCursorState(false);

        base.OnHide(); // ★ 중요: 마지막에 호출 (GameObject 비활성화 등) ★
    }

    // ★ 이벤트 구독/해제 로직 (OnShow/OnHide에서 호출) ★
    private void SubscribeToEvents()
    {
        // Debug.Log($"[{gameObject.name}] Subscribing to events.");
        if (_inventorySystemRef != null)
            _inventorySystemRef.OnSlotChanged += UpdateUIBasedOnInventory;
    }
    private void UnsubscribeFromEvents()
    {
        // Debug.Log($"[{gameObject.name}] Unsubscribing from events.");
        if (_inventorySystemRef != null)
            _inventorySystemRef.OnSlotChanged -= UpdateUIBasedOnInventory;
    }

    // --- IWeaponCraftingView 메서드 구현 (Presenter가 호출 - 대부분 변경 없음) ---
    public void SetAvailableCategories(List<WeaponClass> categories) { /* ... */ }

    public void DisplayCraftableRecipes(List<RecipeViewModel> recipes)
    {
        
        // 슬롯 제거/생성 또는 풀링 로직
        foreach (var slot in _activeCraftableSlots) Destroy(slot.gameObject); // 간단 제거
        _activeCraftableSlots.Clear();
        if (craftableItemSlotPrefab == null) return;

        Debug.Log($"[View] 슬롯 생성 루프 시작. 생성할 슬롯 수: {recipes.Count}");
        foreach (var vm in recipes)
        {
            Debug.Log($"[View] 슬롯 생성 시도: Recipe='{vm?.Recipe?.name ?? "NULL"}', DisplayName='{vm?.DisplayName ?? "NULL"}'");
            CraftableItemSlot_UI slot = Instantiate(craftableItemSlotPrefab, craftableListContent);
            if (slot == null) { Debug.LogError($"[View] 슬롯 Instantiate 실패!"); continue; }
            slot.Setup(vm.Recipe, (selectedRecipe) => OnRecipeSelected?.Invoke(selectedRecipe));
            slot.UpdateCraftabilityVisuals(vm.CanCraft);
            _activeCraftableSlots.Add(slot);
        }
        Debug.Log($"[View] 슬롯 생성 루프 완료. 생성된 슬롯 수: {_activeCraftableSlots.Count}");
        // 레이아웃 강제 업데이트 및 스크롤 초기화
        LayoutRebuilder.ForceRebuildLayoutImmediate(craftableListContent as RectTransform);

        if (craftableListScrollView != null)
        {
            craftableListScrollView.verticalNormalizedPosition = 1f; // 맨 위로 스크롤
        }
    }

    public void DisplaySelectedRecipeDetails(RecipeDetailsViewModel details)
    {
        Debug.Log($"<color=cyan>[View] DisplaySelectedRecipeDetails 호출됨. Details null?: {details == null}</color>");

        if (detailsPanel == null) { Debug.LogError("[View] Details Panel 참조 없음!"); return; }

        if (details == null)
        {
            detailsPanel.SetActive(false);
            // Debug.Log("[View] 상세 정보 패널 숨김.");
            return;
        }

        detailsPanel.SetActive(true);
        // Debug.Log("[View] 상세 정보 패널 표시.");

        // --- 기본 정보 업데이트 ---
        weaponNameText.text = details.Name;
        weaponIconImage.sprite = details.Icon;
        weaponIconImage.enabled = details.Icon != null;
        descriptionText.text = details.Description;
        // Debug.Log($"[View] 기본 정보 설정 완료: Name={details.Name}, IconSet={details.Icon!=null}");

        // --- 재료 및 스탯 목록 업데이트 호출 ---
        PopulateRequiredMaterials(details.RequiredMaterials);
        UpdateStatsDisplay(details.Stats);
    }

    private void PopulateRequiredMaterials(List<MaterialViewModel> materials)
    {
        Debug.Log($"[View] PopulateRequiredMaterials 호출됨. 재료 수: {materials?.Count ?? 0}"); // ★ 호출 및 개수 확인 ★
        foreach (var slot in _activeMaterialSlots) Destroy(slot.gameObject);
        _activeMaterialSlots.Clear();
        if (requiredMaterialSlotPrefab == null || materials == null) return;

        foreach (var vm in materials)
        {
            // ★ 재료 슬롯 생성 로그 ★
            string matName = vm?.MaterialItem?.DisplayNameKey ?? "NULL";
            Debug.Log($"[View] 재료 슬롯 생성 시도: Mat={matName}, Req={vm?.RequiredCount}, Owned={vm?.OwnedCount}");

            RequiredMaterialSlot_UI slot = Instantiate(requiredMaterialSlotPrefab, requiredMaterialsContent);
            if (slot == null) { Debug.LogError("[View] 재료 슬롯 Instantiate 실패!"); continue; }

            slot.Setup(vm.MaterialItem?.ItemID ?? 0, vm.MaterialItem?.Icon, matName, vm.RequiredCount, vm.OwnedCount);
            _activeMaterialSlots.Add(slot);
        }
        Debug.Log($"[View] 재료 슬롯 생성 완료. 실제 생성 수: {_activeMaterialSlots.Count}"); // ★ 최종 개수 확인 ★
        LayoutRebuilder.ForceRebuildLayoutImmediate(requiredMaterialsContent as RectTransform);

    }

    private void UpdateStatsDisplay(List<StatViewModel> stats)
    {
        Debug.Log($"[View] UpdateStatsDisplay 호출됨. 스탯 수: {stats?.Count ?? 0}"); // ★ 호출 및 개수 확인 ★
        foreach (var text in _activeStatTexts) Destroy(text.gameObject);
        _activeStatTexts.Clear();
        if (statTextPrefab == null || stats == null) return;

        foreach (var vm in stats)
        {
            // ★ 스탯 텍스트 생성 로그 ★
            Debug.Log($"[View] 스탯 텍스트 생성 시도: Name={vm?.Name}, Value={vm?.ValueString}");

            TextMeshProUGUI statText = Instantiate(statTextPrefab, statsContent);
            if (statText == null) { Debug.LogError("[View] 스탯 텍스트 Instantiate 실패!"); continue; }

            statText.text = $"{vm.Name}: {vm.ValueString}";
            _activeStatTexts.Add(statText);
        }
        Debug.Log($"[View] 스탯 텍스트 생성 완료. 실제 생성 수: {_activeStatTexts.Count}"); // ★ 최종 개수 확인 ★
        LayoutRebuilder.ForceRebuildLayoutImmediate(statsContent as RectTransform);
    }

    public void UpdateCraftButtonInteractable(bool interactable) => craftButton.interactable = interactable;
        
    public void ShowConfirmationPopup(string message)
    {
        var popupInstance = UIManager.Instance?.ShowUI<ConfirmationPopup_UI>(true); // isPopup = true
        if (popupInstance != null)
        {
            // 팝업 인스턴스에 직접 데이터 설정 (SetupAndShow 대신)
            popupInstance.SetMessage(message); // ConfirmationPopup_UI에 SetMessage 메서드 추가 필요
            popupInstance.SetupActions( // ConfirmationPopup_UI에 SetupActions 메서드 추가 필요
                () => OnConfirmCraft?.Invoke(),
                () => OnCancelCraft?.Invoke()
            );
        }
        else { Debug.LogError("팝업 UI를 열 수 없습니다!"); }
    }

    public void HideConfirmationPopup()
    { UIManager.Instance?.HideUI<ConfirmationPopup_UI>(); }

    public void ShowMessage(string message) // duration은 멤버 변수 사용
    {
        if (messageText == null) return;
        if (_messageCoroutine != null) StopCoroutine(_messageCoroutine);
        _messageCoroutine = StartCoroutine(ShowMessageCoroutine(message));
    }
    private IEnumerator ShowMessageCoroutine(string message)
    {
        messageText.text = message;
        messageText.gameObject.SetActive(true);
        yield return new WaitForSeconds(messageDuration);
        messageText.gameObject.SetActive(false);
        _messageCoroutine = null;
    }

    public void HighlightSelectedSlot(RecipeData recipeToSelect)
    {
        foreach (var slot in _activeCraftableSlots)
        {
            slot.SetSelected(slot.AssociatedRecipe == recipeToSelect);
        }
    }

    

    //private void ClearSelectedWeaponInfo() { if (detailsPanel != null) detailsPanel.SetActive(false); }

    // 인벤토리 변경 시 호출될 메서드
    private void UpdateUIBasedOnInventory(ItemType type, int idx, ItemStack stack)
    {
        // UI가 활성화 상태일 때만 갱신 수행
        if (gameObject.activeInHierarchy && _presenter != null)
        {
            Debug.Log("인벤토리 변경 감지됨. WeaponCrafting_UI 갱신 시작.");

            // ★ Presenter에게 현재 카테고리 정보로 갱신 요청 ★
            // HandleCategorySelected는 레시피 목록과 상세 정보를 모두 다시 로드함
            // _currentCategory 변수는 Presenter 내부에 있으므로,
            // Presenter에 현재 카테고리를 다시 로드하는 메서드를 만들거나,
            // View가 현재 카테고리를 기억하고 있다가 전달해야 함.
            // 여기서는 View가 기억하고 있다고 가정 (더 간단)
            _presenter.RefreshCurrentCategory(); // ★ Presenter에 이 메서드 추가 필요 ★

            // 또는, 현재 선택된 레시피만 갱신 (덜 완전할 수 있음)
            // if (_presenter != null && _presenter.HasSelectedRecipe()) // Presenter에 확인 메서드 추가 가정
            // {
            //     _presenter.RefreshSelectedRecipeView(); // Presenter에 이 메서드 추가 가정
            // }
        }
        // else Debug.Log("인벤토리 변경 감지됨 (UI 비활성 상태).");
    }
}
