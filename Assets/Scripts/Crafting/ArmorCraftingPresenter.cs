using ProjectRaid.Core; // ArmorClass, ItemType Enum 등
using ProjectRaid.Data; // ItemData, RecipeData, ArmorData 등
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ArmorCraftingPresenter
{
    private readonly IArmorCraftingView _view; // 방어구 제작 View 인터페이스
    private readonly ICraftingSystem _craftingSystem;
    private readonly InventorySystem _inventorySystem; // 실제 InventorySystem 사용

    private RecipeData _selectedRecipe;
    private ArmorClass _currentCategory; // 현재 선택된 방어구 카테고리

    public ArmorCraftingPresenter(IArmorCraftingView view, ICraftingSystem craftingSystem, InventorySystem inventorySystem)
    {
        this._view = view;
        this._craftingSystem = craftingSystem;
        this._inventorySystem = inventorySystem;

        if (view == null || craftingSystem == null || inventorySystem == null) { /* 오류 처리 */ return; }

        SubscribeEvents();
    }

    private void SubscribeEvents()
    {

        _view.OnCategorySelected += HandleCategorySelected;
        _view.OnRecipeSelected += HandleRecipeSelected;
        _view.OnCraftButtonPressed += HandleCraftButtonPressed;
        _view.OnConfirmCraft += HandleConfirmCraft;
        _view.OnCancelCraft += HandleCancelCraft;
        if (_inventorySystem != null)
        {
            _inventorySystem.OnSlotChanged += HandleInventorySlotChanged; // OnSlotChanged 사용
        }

    }

    // 현재 WeaponCraftingPresenter에서 Dispose가 사용되지 않으므로 일단 생략. 필요시 추가.
    // public void Dispose() { /* 이벤트 구독 해제 */ }

    public void InitializeView(ArmorClass initialCategory = ArmorClass.Helmet)
    {
        Debug.Log($"<color=yellow>[ArmorCraftingPresenter] InitializeView 호출됨. 초기 카테고리: {initialCategory}</color>");

        bool itemDbReady = GameManager.Instance.Database != null && GameManager.Instance.Database.IsInitialized;
        bool recipeDbReady = _craftingSystem.IsRecipeDatabaseReady();

        Debug.Log($"[ArmorCraftingPresenter] 데이터 준비 상태 확인: ItemDB={itemDbReady}, RecipeDB={recipeDbReady}");

        if (!itemDbReady || !recipeDbReady)
        {
            Debug.LogWarning("[ArmorCraftingPresenter] 데이터베이스가 아직 준비되지 않았습니다. 로딩을 기다립니다...");
            // _view.ShowLoadingIndicator(); // View에 로딩 표시 요청 (필요시 IArmorCraftingView에 추가)
            return;
        }
        //초기 카테고리 로드
        HandleCategorySelected(initialCategory);
    }

    private void HandleCategorySelected(ArmorClass category) // 파라미터 타입 ArmorClass로 변경
    {
        _currentCategory = category;
        Debug.Log($"[ArmorCraftingPresenter] HandleCategorySelected({category})");

        bool itemDbReady = GameManager.Instance.Database != null && GameManager.Instance.Database.IsInitialized;
        bool recipeDbReady = _craftingSystem.IsRecipeDatabaseReady();

        if (!itemDbReady || !recipeDbReady)
        {
            Debug.LogWarning($"[ArmorCraftingPresenter] HandleCategorySelected({category}): 데이터베이스 미준비. 실행 건너뜀.");
            _view.DisplayCraftableRecipes(new List<RecipeViewModel>());
            HandleRecipeSelected(null);
            return;
        }

        List<RecipeData> recipes = _craftingSystem.GetRecipesByArmorClass(category); // 방어구 레시피 가져오기
        if (recipes == null)
        {
            Debug.LogError($"[ArmorCraftingPresenter] GetRecipesByArmorClass({category}) 결과가 null입니다!");
        }
        else
        {
            Debug.Log($"[ArmorCraftingPresenter] GetRecipesByArmorClass({category}) 결과 레시피 개수: {recipes.Count}");
            foreach (var r in recipes)
            {
                Debug.Log($" - 레시피: {r?.name}, 결과물: {r?.ResultItem?.DisplayNameKey ?? "N/A"}");
            }
        }

        List<RecipeViewModel> viewModels = recipes.Select(r =>
        {
            if (r == null || r.ResultItem == null)
            {
                Debug.LogWarning("[ArmorCraftingPresenter] Null RecipeData or ResultItem found in recipes list.");
                return new RecipeViewModel { Recipe = r, DisplayName = "INVALID RECIPE", Icon = null, CanCraft = false };
            }
            string recipeName = r.name ?? "Recipe NULL";
            string resultItemNameKey = r.ResultItem.DisplayNameKey ?? "Result Item NULL";
            string finalDisplayName = GetItemName(r.ResultItem);
            Sprite icon = r.ResultItem.Icon;
            bool canCraft = _craftingSystem.CanCraft(r);
            Debug.Log($"[ArmorCraftingPresenter VM 생성] Recipe: {recipeName}, ResultKey: {resultItemNameKey}, FinalName: {finalDisplayName}, IconNull?: {icon == null}, CanCraft: {canCraft}");

            return new RecipeViewModel
            {
                Recipe = r,
                DisplayName = finalDisplayName,
                Icon = icon,
                CanCraft = canCraft
            };
        }).ToList();
        if (viewModels == null)
        {
            Debug.LogError("[ArmorCraftingPresenter] 생성된 viewModels 리스트가 null입니다!");
        }
        else
        {
            Debug.Log($"[ArmorCraftingPresenter] 생성된 viewModels 개수: {viewModels.Count}");
            for (int i = 0; i < viewModels.Count; i++)
            {
                var vm = viewModels[i];
                if (vm == null)
                {
                    Debug.LogError($"[ArmorCraftingPresenter] viewModels[{i}] is null!");
                }
                else
                {
                    Debug.Log($"  - ViewModel[{i}]: Recipe='{vm.Recipe?.name ?? "N/A"}', DisplayName='{vm.DisplayName ?? "N/A"}'");
                }
            }
        }

        _view.DisplayCraftableRecipes(viewModels);
        if (viewModels.Any(vm => vm.Recipe != null)) // 유효한 레시피가 하나라도 있으면
        {
            // 첫 번째 유효한 레시피 선택 또는 기본 선택 로직
            RecipeData firstValidRecipe = viewModels.FirstOrDefault(vm => vm.Recipe != null)?.Recipe;
            HandleRecipeSelected(firstValidRecipe);
        }
        else
        {
            HandleRecipeSelected(null);
        }
    }

    private void HandleRecipeSelected(RecipeData recipe) // 파라미터 타입 RecipeData로 변경
    {
        string recipeName = recipe?.name ?? "NULL";
        Debug.Log($"<color=yellow>[ArmorCraftingPresenter] HandleRecipeSelected 호출됨. 선택된 Recipe: {recipeName}</color>");

        _selectedRecipe = recipe;
        if (_selectedRecipe != null && _selectedRecipe.ResultItem != null)
        {
            RecipeDetailsViewModel details = CreateRecipeDetailsViewModel(_selectedRecipe);
            Debug.Log($"[ArmorCraftingPresenter] _view.DisplaySelectedRecipeDetails 호출 시도. Details null?: {details == null}");
            _view.DisplaySelectedRecipeDetails(details);
            _view.UpdateCraftButtonInteractable(_craftingSystem.CanCraft(_selectedRecipe));
            _view.HighlightSelectedSlot(_selectedRecipe);
        }
        else
        {
            Debug.Log("[ArmorCraftingPresenter] 선택된 레시피 유효하지 않음. 상세 정보 클리어.");
            _view.DisplaySelectedRecipeDetails(null);
            _view.UpdateCraftButtonInteractable(false);
            _view.HighlightSelectedSlot(null);
        }
    }

    private RecipeDetailsViewModel CreateRecipeDetailsViewModel(RecipeData recipe) // 파라미터 타입 RecipeData로 변경
    {
        RecipeDetailsViewModel details = new RecipeDetailsViewModel { Stats = new List<StatViewModel>() };
        if (recipe == null || recipe.ResultItem == null) return null;

        details.Name = GetItemName(recipe.ResultItem);
        details.Description = GetItemDescription(recipe.ResultItem);
        details.Icon = recipe.ResultItem.Icon;

        details.RequiredMaterials = recipe.RequiredMaterials?.Select(req =>
        {
            int owned = _inventorySystem.GetItemCount(req.ItemID);
            // ★ 재료 ViewModel 생성 로그 ★
            // Debug.Log($"  [Presenter VM 재료] Mat: {ing.materialItem?.DisplayNameKey ?? "NULL"}, Req: {ing.count}, Owned: {owned}");
            return new MaterialViewModel
            {
                MaterialItem = GameManager.Instance.Database.GetItem(req.ItemID),
                RequiredCount = req.Quantity,
                OwnedCount = owned
            };
        }).ToList() ?? new List<MaterialViewModel>();

        // 스탯 ViewModel 생성 (방어구 데이터 사용)
        if (recipe.ResultItem.Equipment is ArmorData armorEquipment) // ArmorData로 캐스팅
        {
            StatBlock stats = armorEquipment.BaseStats;
            if (stats.Attack > 0) details.Stats.Add(new StatViewModel { Name = "공격력", ValueString = stats.Attack.ToString("F0") });
            if (stats.AttackSpeed > 0) details.Stats.Add(new StatViewModel { Name = "공격 속도", ValueString = stats.AttackSpeed.ToString("F1") }); // 방어구도 공속이 있을 수 있음 (세트 효과 등)
            if (stats.CriticalChance > 0) details.Stats.Add(new StatViewModel { Name = "치명타 확률", ValueString = $"{stats.CriticalChance}%" });
            if (stats.HP > 0) details.Stats.Add(new StatViewModel { Name = "최대 체력", ValueString = stats.HP.ToString("F0") });
            if (stats.Defense > 0) details.Stats.Add(new StatViewModel { Name = "방어력", ValueString = stats.Defense.ToString("F0") });
            if (stats.Stamina > 0) details.Stats.Add(new StatViewModel { Name = "스태미나", ValueString = stats.Stamina.ToString("F0") });
            if (stats.MoveSpeed > 0) details.Stats.Add(new StatViewModel { Name = "이동 속도", ValueString = stats.MoveSpeed.ToString("F1") });
            // ... 다른 방어구 관련 스탯 추가 ...
            Debug.Log($"  [ArmorCraftingPresenter VM 스탯] {details.Stats.Count}개 스탯 정보 추가됨.");
        }

        return details;
    }

    private void HandleCraftButtonPressed()
    {
        Debug.Log($"<color=yellow>[ArmorCraftingPresenter] HandleCraftButtonPressed 호출됨. 선택된 레시피: {_selectedRecipe?.name ?? "NULL"}</color>");
        if (_selectedRecipe != null && _craftingSystem.CanCraft(_selectedRecipe))
        {
            string itemName = GetItemName(_selectedRecipe.ResultItem);
            Debug.Log($"[ArmorCraftingPresenter] 제작 가능 확인. 팝업 표시 요청: '{itemName}'");
            _view.ShowConfirmationPopup($"'{itemName}' 을(를) 제작하시겠습니까?");
        }
        else if (_selectedRecipe == null)
        {
            Debug.LogWarning("[ArmorCraftingPresenter] 제작할 레시피가 선택되지 않았습니다.");
            _view.ShowMessage("제작할 아이템을 선택해주세요.");
        }
        else
        {
            Debug.LogWarning($"[ArmorCraftingPresenter] 재료 부족 또는 다른 조건 불충족: '{GetItemName(_selectedRecipe.ResultItem)}'");
            _view.ShowMessage("재료가 부족하거나 제작 조건을 만족하지 않습니다.");
        }
    }

    private void HandleConfirmCraft()
    {
        if (_selectedRecipe == null || !_craftingSystem.CanCraft(_selectedRecipe))
        {
            Debug.LogError("[ArmorCraftingPresenter] 제작 확정 실패: 레시피가 유효하지 않거나 제작할 수 없는 상태입니다.");
            _view.HideConfirmationPopup();
            return;
        }

        bool success = _craftingSystem.TryCraftItem(_selectedRecipe);

        if (success)
        {
            _view.ShowMessage($"'{GetItemName(_selectedRecipe.ResultItem)}' 제작 성공!");
            // 제작 성공 후 UI 갱신 (재료 수량 변경 등)
            RefreshCurrentCategoryView();
        }
        else
        {
            _view.ShowMessage("제작에 실패했습니다.");
        }
        _view.HideConfirmationPopup(); // 성공/실패 여부와 관계없이 팝업은 닫음
    }

    private void HandleCancelCraft()
    {
        Debug.Log("[ArmorCraftingPresenter] Cancel Craft Action Received.");
        _view.HideConfirmationPopup(); // 팝업 닫기
    }

    // InventorySystem.OnSlotChanged 이벤트 핸들러 (WeaponCraftingPresenter와 동일한 시그니처 가정)
    // 실제 InventorySystem의 이벤트 시그니처에 맞춰 파라미터 수정 필요
    private void HandleInventoryChanged()
    {
        Debug.Log("[ArmorCraftingPresenter] 인벤토리 변경 감지. 현재 카테고리 UI 갱신.");
        RefreshCurrentCategoryView();
    }

    // 인벤토리 변경이나 제작 성공 시 현재 카테고리 뷰를 새로고침하는 메서드
    public void RefreshCurrentCategoryView()
    {
        // 현재 선택된 카테고리가 있다면 해당 카테고리 레시피 목록 및 상세 정보 갱신
        if (_currentCategory != default(ArmorClass)) // 기본값이 아닌 유효한 카테고리일 때
        {
            HandleCategorySelected(_currentCategory);
        }
        else if (_selectedRecipe != null) // 선택된 레시피가 있다면 해당 레시피 상세 정보만 갱신
        {
            HandleRecipeSelected(_selectedRecipe);
        }
    }
    private void HandleInventorySlotChanged(ItemType type, int slotIndex, ItemStack stack)
    {
        Debug.Log($"[ArmorCraftingPresenter] 인벤토리 슬롯 변경 감지 (Type: {type}, Index: {slotIndex}). UI 갱신.");
        RefreshCurrentCategoryView();
    }

    // --- 아이템 이름/설명 가져오는 헬퍼 ---
    private string GetItemName(ItemData item) => item?.DisplayNameKey ?? "이름 없음";
    private string GetItemDescription(ItemData item) => item?.DescriptionKey ?? "설명 없음";
}
