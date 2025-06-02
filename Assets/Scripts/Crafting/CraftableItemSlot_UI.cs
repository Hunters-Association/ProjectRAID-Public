using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using ProjectRaid.Data; // 네임스페이스 사용

public class CraftableItemSlot_UI : MonoBehaviour
{
    [SerializeField] private Image backgroundImage, iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Button button;
    [SerializeField] private Sprite normalSprite, selectedSprite;
    [SerializeField] private GameObject cantCraftOverlay; // 제작 불가 표시

    public RecipeData AssociatedRecipe { get; private set; }
    private Action<RecipeData> onClickCallback;

    // RecipeDataSO를 받도록 수정
    public void Setup(RecipeData recipe, Action<RecipeData> callback)
    {
        AssociatedRecipe = recipe;
        onClickCallback = callback;

        if (AssociatedRecipe.ResultItem != null)
        {
            ItemData resultItem = AssociatedRecipe.ResultItem;
            iconImage.sprite = resultItem.Icon;
            iconImage.enabled = iconImage.sprite != null;
            // 이름 표시 (현지화 필요 시 키 사용 후 변환)
            nameText.text = resultItem.DisplayNameKey ?? "이름 없음"; // 이름 키 사용
            button?.onClick.RemoveAllListeners();
            button?.onClick.AddListener(OnClick);
        }
        else { /* 초기화 */ }
        SetSelected(false);
    }

    public void SetSelected(bool isSelected) => backgroundImage.sprite = isSelected ? selectedSprite : normalSprite;
    private void OnClick()
    {
        // ★★★ 슬롯 클릭 로그 추가 ★★★
        string recipeName = AssociatedRecipe?.name ?? "NULL_RECIPE";
        string resultItemName = AssociatedRecipe?.ResultItem?.DisplayNameKey ?? "NULL_ITEM";
        Debug.Log($"<color=lightblue>[Slot UI] OnClick 호출됨! Recipe: {recipeName}, Result: {resultItemName}</color>");
        // ★★★★★★★★★★★★★★★★★★★

        if (AssociatedRecipe != null && onClickCallback != null)
        {
            onClickCallback(AssociatedRecipe); // Presenter에게 이벤트 전달
        }
        else
        {
            Debug.LogError($"[Slot UI] 클릭 콜백 실행 불가: Recipe={recipeName}, Callback null?:{onClickCallback == null}", this);
        }
    }

    // Presenter가 전달한 CanCraft 정보 사용
    public void UpdateCraftabilityVisuals(bool canCraft)
    {
        if (cantCraftOverlay != null) cantCraftOverlay.SetActive(!canCraft);
        // 이미지/텍스트 색상 변경 등 추가 가능
    }
}
