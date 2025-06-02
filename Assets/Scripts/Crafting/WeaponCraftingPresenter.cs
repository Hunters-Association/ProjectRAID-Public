using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectRaid.Data;
using ProjectRaid.Core; // Enums

public class WeaponCraftingPresenter
{
    private readonly IWeaponCraftingView view;
    private readonly ICraftingSystem craftingSystem;
    private readonly InventorySystem inventorySystem;
    // 이벤트 채널 대신 직접 구독

    private RecipeData selectedRecipe;
    private WeaponClass currentCategory; // WeaponClass 사용
    //private bool databasesChecked = false;

    // 생성자에서 인터페이스 타입 참조 받기
    public WeaponCraftingPresenter(IWeaponCraftingView view, ICraftingSystem crafting, InventorySystem inventory)
    {
        this.view = view;
        craftingSystem = crafting;
        inventorySystem = inventory;

        if (view == null || craftingSystem == null || inventorySystem == null) { /* 오류 처리 */ return; }
        
        SubscribeEvents();
    }

    private void SubscribeEvents()
    {
        view.OnCategorySelected += HandleCategorySelected;
        view.OnRecipeSelected += HandleRecipeSelected;
        view.OnCraftButtonPressed += HandleCraftButtonPressed;
        view.OnConfirmCraft += HandleConfirmCraft;
        view.OnCancelCraft += HandleCancelCraft;
    }

    // --- 현재 사용되지 않음 ---
    // public void Dispose() // 이벤트 구독 해제
    // {
    //     if (view != null) { /* View 이벤트 해제 */ }
    //     if (inventorySystem != null) inventorySystem.OnSlotChanged -= HandleInventoryChanged;
    // }

    public void InitializeView(WeaponClass initialCategory = WeaponClass.Rifle) // WeaponClass 사용
    {
        Debug.Log($"<color=yellow>[Presenter] InitializeView 호출됨. 초기 카테고리: {initialCategory}</color>");

        // ★ 데이터베이스 준비 상태 직접 확인 ★
        // ItemDatabase는 싱글톤이므로 직접 확인
        bool itemDbReady = GameManager.Instance.Database != null && GameManager.Instance.Database.IsInitialized;
        // RecipeDatabase 상태는 CraftingSystem을 통해 확인 (CraftingSystem에 메서드 추가 필요)
        bool recipeDbReady = craftingSystem.IsRecipeDatabaseReady(); // ★ CraftingSystem에 IsRecipeDatabaseReady() 필요 ★

        Debug.Log($"[Presenter] 데이터 준비 상태 확인 시도: ItemDB={itemDbReady}, RecipeDB={recipeDbReady}");

        // ★ 데이터베이스가 하나라도 준비 안됐으면 로딩 대기 또는 지연 처리 ★
        if (!itemDbReady || !recipeDbReady)
        {
            //databasesChecked = false; // 아직 확인 안 됨
            Debug.LogWarning("[Presenter] 데이터베이스가 아직 준비되지 않았습니다. 로딩을 기다립니다...");
            // view.ShowLoadingIndicator(); // 로딩 UI 표시

            // (선택적) GameManager의 OnDataReady 이벤트를 여기서 구독하거나,
            // 또는 간단히 다음 Update에서 다시 체크하도록 할 수 있음
            // 또는 async Task로 변경하여 await GameManager.Instance.WaitForDataReady(); 같은 메서드 호출
            return; // 데이터 로딩 완료 후 다시 시도하도록 여기서 종료
        }
    }

    // WeaponClass 사용
    private void HandleCategorySelected(WeaponClass category)
    {
        currentCategory = category;
        // ★ 데이터베이스 준비 상태 재확인 (안전 장치) ★
        bool itemDbReady = GameManager.Instance.Database != null && GameManager.Instance.Database.IsInitialized;
        bool recipeDbReady = craftingSystem.IsRecipeDatabaseReady(); // CraftingSystem 메서드 사용
        Debug.Log($"[Presenter] HandleCategorySelected({category}): 데이터 준비 상태 확인: ItemDB={itemDbReady}, RecipeDB={recipeDbReady}");

        if (!itemDbReady || !recipeDbReady) // ★ 하나라도 준비 안 됐으면 중단 ★
        {
            Debug.LogWarning($"[Presenter] HandleCategorySelected({category}): 데이터베이스 미준비. 실행 건너<0xEB><0x9B><0x84>뜀.");
            view.DisplayCraftableRecipes(new List<RecipeViewModel>()); // 빈 목록 표시
            HandleRecipeSelected(null); // 상세 정보 클리어
            return;
        }
        
        Debug.Log($"[Presenter] HandleCategorySelected({category}): CraftingSystem.GetRecipesByWeaponClass 호출 시도...");
        List<RecipeData> recipes = craftingSystem.GetRecipesByWeaponClass(category);

        List<RecipeViewModel> viewModels = recipes.Select(r => {
            // ★ 각 레시피 처리 시 로그 추가 ★
            if (r == null) return new RecipeViewModel();
            
            string recipeName = r.name ?? "NULL";
            string resultItemNameKey = r.ResultItem != null ? r.ResultItem.DisplayNameKey : "NULL";
            string finalDisplayName = GetItemName(r.ResultItem); // GetItemName 결과 확인
            Sprite icon = r.ResultItem != null ? r.ResultItem.Icon : null;
            bool canCraft = craftingSystem.CanCraft(r);
            Debug.Log($"[Presenter VM 생성] Recipe: {recipeName}, ResultKey: {resultItemNameKey}, FinalName: {finalDisplayName}, IconNull?: {icon == null}, CanCraft: {canCraft}");

            return new RecipeViewModel
            {
                Recipe = r,
                DisplayName = finalDisplayName, // GetItemName 결과 사용
                Icon = icon,
                CanCraft = canCraft
            };
        }).ToList();
        view.DisplayCraftableRecipes(viewModels);
        if (viewModels.Count > 0) HandleRecipeSelected(viewModels[0].Recipe);
        else HandleRecipeSelected(null);
    }

    private void HandleRecipeSelected(RecipeData recipe)
    {
        string recipeName = recipe.name ?? "NULL";
        Debug.Log($"<color=yellow>[Presenter] HandleRecipeSelected 호출됨. 선택된 Recipe: {recipeName}</color>");

        selectedRecipe = recipe;
        if (selectedRecipe != null && selectedRecipe.ResultItem != null)
        {
            RecipeDetailsViewModel details = CreateRecipeDetailsViewModel(selectedRecipe);
            
            Debug.Log($"[Presenter] view.DisplaySelectedRecipeDetails 호출 시도. Details null?: {details == null}");
            Debug.Log("[Presenter] view.DisplaySelectedRecipeDetails 호출 시도...");
            view.DisplaySelectedRecipeDetails(details);
            view.UpdateCraftButtonInteractable(craftingSystem.CanCraft(selectedRecipe));
            view.HighlightSelectedSlot(selectedRecipe);
        }
        else // 선택된 레시피가 없거나 유효하지 않으면
        {
            Debug.Log("[Presenter] 선택된 레시피 유효하지 않음. 상세 정보 클리어.");
            view.DisplaySelectedRecipeDetails(null);
            view.UpdateCraftButtonInteractable(false);
            view.HighlightSelectedSlot(null); // 선택 강조 해제
        }
        //view.HighlightSelectedSlot(selectedRecipe);
    }
    private RecipeDetailsViewModel CreateRecipeDetailsViewModel(RecipeData recipe)
    {
        RecipeDetailsViewModel details = new() { Stats = new List<StatViewModel>() }; // 초기화
        if (recipe == null || recipe.ResultItem == null) return null;

        details.Name = GetItemName(recipe.ResultItem);
        details.Description = GetItemDescription(recipe.ResultItem);
        details.Icon = recipe.ResultItem.Icon;
        // PreviewImage 로직 추가 필요

        // 필요 재료 ViewModel 생성
        details.RequiredMaterials = recipe.RequiredMaterials?.Select(req => {
            int owned = inventorySystem.GetItemCount(req.ItemID);
            // ★ 재료 ViewModel 생성 로그 ★
            // Debug.Log($"  [Presenter VM 재료] Mat: {ing.materialItem?.DisplayNameKey ?? "NULL"}, Req: {ing.count}, Owned: {owned}");
            return new MaterialViewModel
            {
                MaterialItem = GameManager.Instance.Database.GetItem(req.ItemID),
                RequiredCount = req.Quantity,
                OwnedCount = owned
            };
        }).ToList() ?? new List<MaterialViewModel>();

        // 스탯 ViewModel 생성 (StatBlock 사용)
        if (recipe.ResultItem.Equipment is EquipmentData equipment)
        {
            StatBlock stats = equipment.BaseStats;
            // ★ StatBlock 필드를 순회하며 ViewModel 생성 (예시) ★
            if (stats.Attack > 0f)          details.Stats.Add(new() { Name = "공격력",  ValueString = stats.Attack.ToString("F0") });
            if (stats.AttackSpeed > 1f)     details.Stats.Add(new() { Name = "공격 속도", ValueString = stats.AttackSpeed.ToString("F1") });
            if (stats.CriticalChance > 0f)  details.Stats.Add(new() { Name = "치명타 확률", ValueString = $"{stats.CriticalChance:F1}%" });
            if (stats.CriticalDamage > 0f)  details.Stats.Add(new() { Name = "치명타 피해", ValueString = $"{stats.CriticalDamage:F1}%" });
            if (stats.Defense > 0f)         details.Stats.Add(new() { Name = "방어력", ValueString = stats.Defense.ToString("F0") });
            if (stats.HP > 0f)              details.Stats.Add(new() { Name = "최대 체력", ValueString = stats.HP.ToString("F0") });
            if (stats.Stamina > 0f)         details.Stats.Add(new() { Name = "스태미나", ValueString = stats.Stamina.ToString("F0") });
            if (stats.MoveSpeed > 1f)       details.Stats.Add(new() { Name = "이동 속도", ValueString = stats.Stamina.ToString("F1") });

            Debug.Log($"[Presenter VM 스탯] {details.Stats.Count}개 스탯 정보 추가됨.");
        }

        return details;
    }

    private void HandleCraftButtonPressed()
    {
        Debug.Log($"<color=yellow>[Presenter] HandleCraftButtonPressed 호출됨. 선택된 레시피: {selectedRecipe?.name ?? "NULL"}</color>");
        if (selectedRecipe != null && craftingSystem.CanCraft(selectedRecipe))
        {
            // ★ View에게 확인 팝업 표시 요청 ★
            // 만들 아이템 이름을 가져와서 메시지에 포함
            string itemName = GetItemName(selectedRecipe.ResultItem); // 헬퍼 함수 사용
            Debug.Log($"[Presenter] 제작 가능 확인. 팝업 표시 요청: '{itemName}'");
            view.ShowConfirmationPopup($"'{itemName}' 을(를) 제작하시겠습니까?"); // ★ View의 메서드 호출 ★
        }
        else if (selectedRecipe == null)
        {
            Debug.LogWarning("[Presenter] 제작할 레시피가 선택되지 않았습니다.");
            view.ShowMessage("제작할 아이템을 선택해주세요."); // 사용자에게 알림
        }
        else // CanCraft가 false인 경우
        {
            Debug.LogWarning($"[Presenter] 재료 부족 또는 다른 조건 불충족: '{GetItemName(selectedRecipe.ResultItem)}'");
            view.ShowMessage("재료가 부족하거나 제작 조건을 만족하지 않습니다."); // 사용자에게 알림
        }
    }
    private void HandleConfirmCraft()
    {
        if (selectedRecipe == null || !craftingSystem.CanCraft(selectedRecipe))
        {
            Debug.LogError("[Presenter] 제작 확정 실패: 레시피가 유효하지 않거나 제작할 수 없는 상태입니다.");
            view.HideConfirmationPopup(); // 팝업 닫기
            return;
        }

        // CraftingSystem에 제작 요청
        bool success = craftingSystem.TryCraftItem(selectedRecipe);

        // 결과에 따른 피드백 (TryCraftItem에서 로그를 찍지만, UI 메시지도 표시)
        if (success)
        {
            view.ShowMessage($"'{GetItemName(selectedRecipe.ResultItem)}' 제작 성공!");
        }
        else
        {
            view.ShowMessage("제작에 실패했습니다."); // 실패 원인은 TryCraftItem 로그 확인
        }

    }
    private void HandleCancelCraft()
    {
        Debug.Log("[Presenter] Cancel Craft Action Received.");
        // 확인 팝업은 View에서 자동으로 닫히므로 여기서 추가 작업 필요 없음
        // view.HideConfirmationPopup(); // 여기서 호출할 필요 없음
    }

    // --- WeaponCraftingUI.cs와 로직 중복됨 ---
    // private void HandleInventoryChanged(ItemType type, int idx, InventorySystem.ItemStack stack)
    //     => HandleCategorySelected(currentCategory); // 간단 갱신

    private void HandleCloseButtonPressed()
    {
        Debug.Log("[Presenter] Close button pressed. View will handle closing itself via UIManager.");
        // view.Hide(); // 이 라인 삭제
    }
    public void RefreshCurrentCategory()
    {
        // Debug.Log($"[Presenter] RefreshCurrentCategory 호출됨. 현재 카테고리: {currentCategory}");
        // 기존의 카테고리 선택 처리 로직 재사용
        HandleCategorySelected(currentCategory);
    }


    // ---  실제 이름/설명 가져오는 헬퍼 (구현 필요) ---
    private string GetItemName(ItemData item) => item.DisplayNameKey ?? "???";
    private string GetItemDescription(ItemData item) => item.DescriptionKey ?? "";
    // 스탯 이름/값 형식 변환은 Presenter 또는 별도 유틸리티 클래스에서 처리
}
