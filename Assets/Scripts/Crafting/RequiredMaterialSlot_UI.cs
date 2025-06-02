using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ProjectRaid.Data; // 네임스페이스 사용

public class RequiredMaterialSlot_UI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private Color sufficientColor = Color.white;
    [SerializeField] private Color insufficientColor = Color.red;

    // ★ 파라미터 수정: ItemData 대신 필요한 정보만 받기 ★
    public void Setup(int itemID, Sprite icon, string nameKey, int required, int owned)
    {
        iconImage.sprite = icon;
        iconImage.enabled = iconImage.sprite != null;
        nameText.text = nameKey; // 실제 이름 변환 필요
        countText.text = $"{owned} / {required}";
        countText.color = (owned >= required) ? sufficientColor : insufficientColor;
    }
}
