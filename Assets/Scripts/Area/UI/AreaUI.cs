using UnityEngine;
using UnityEngine.UI;
using ProjectRaid.EditorTools;
using TMPro;

public class AreaUI : MonoBehaviour
{
    [FoldoutGroup("UI", ExtendedColor.Silver)]
    [SerializeField] private TextMeshProUGUI regionText;
    [SerializeField] private TextMeshProUGUI subregionText;
    [SerializeField] private Image dangerBarImage;

    [FoldoutGroup("Setting", ExtendedColor.Crimson)]
    [SerializeField] private Color safeAreaColor;
    [SerializeField] private Color warningAreaColor;
    [SerializeField] private Color dangerousAreaColor;

    public void UpdateArea(AreaInfo info)
    {
        if (dangerBarImage == null) return;

        regionText.text = info.regionName;
        subregionText.text = info.subregionName;
        dangerBarImage.color = GetColorByDangerLevel(info.dangerLevel);
    }

    private Color GetColorByDangerLevel(ZoneDangerLevel level)
    {
        return level switch
        {
            ZoneDangerLevel.Safe => safeAreaColor,
            ZoneDangerLevel.Warning => warningAreaColor,
            ZoneDangerLevel.Dangerous => dangerousAreaColor,
            _ => Color.white
        };
    }
}