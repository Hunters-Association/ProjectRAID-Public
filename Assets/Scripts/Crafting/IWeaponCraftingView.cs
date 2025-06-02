using System;
using System.Collections.Generic;
using ProjectRaid.Data; // 네임스페이스 사용
using ProjectRaid.Core;





// --- View Interface ---
public interface IWeaponCraftingView
{
    event Action<WeaponClass> OnCategorySelected; // WeaponClass 사용
    event Action<RecipeData> OnRecipeSelected;
    event Action OnCraftButtonPressed;
    event Action OnConfirmCraft;
    event Action OnCancelCraft;
    //event Action OnCloseButtonPressed;

    void SetAvailableCategories(List<WeaponClass> categories);
    void DisplayCraftableRecipes(List<RecipeViewModel> recipes);
    void DisplaySelectedRecipeDetails(RecipeDetailsViewModel details);
    void UpdateCraftButtonInteractable(bool interactable);
    void ShowConfirmationPopup(string message);
    void HideConfirmationPopup();
    void ShowMessage(string message);
    void HighlightSelectedSlot(RecipeData recipeToSelect);
    //void Show();
    //void Hide();
}
