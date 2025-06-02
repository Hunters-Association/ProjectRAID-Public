using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ProjectRaid.Data;
using ProjectRaid.Runtime;
using ProjectRaid.EditorTools;
using DG.Tweening;
using TMPro;

/// <summary>
/// 인벤토리 슬롯 UI ‑ 아이콘, 수량, 선택 하이라이트 표시 전용
/// </summary>
public class InventorySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [FoldoutGroup("UI References", ExtendedColor.Silver)]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private CanvasGroup hoverHighlight;
    [SerializeField] private Outline selectionOutline;

    [FoldoutGroup("Animation", ExtendedColor.Cyan)]
    [SerializeField] private float hoverFade = 0.1f;
    [SerializeField] private float selectFade = 0.15f;

    [field: FoldoutGroup("Index", ExtendedColor.DodgerBlue)]
    [field: ShowNonSerializedField] public int Index { get; set; }

    private bool isSelected;

    private void Awake()
    {
        if (hoverHighlight) hoverHighlight.alpha = 0f; // 기본 숨김
    }

    public void Refresh(in ItemStack stack, ItemDatabase db)
    {
        bool isEmpty = stack.itemID == 0;

        iconImage.enabled = !isEmpty;
        quantityText.enabled = !isEmpty;

        if (isEmpty) return;

        ItemData data = db.GetItem(stack.itemID);
        iconImage.sprite = data.Icon;
        quantityText.text = stack.quantity > 1 ? stack.quantity.ToString() : string.Empty;
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        if (!selectionOutline) return;
        selectionOutline.DOFade(selected ? 1f : 0f, selectFade);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverHighlight) hoverHighlight.DOFade(1f, hoverFade);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoverHighlight) hoverHighlight.DOFade(0f, hoverFade);
    }
}
