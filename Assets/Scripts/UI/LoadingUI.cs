using TMPro;
using UnityEngine;

public class LoadingUI : MonoBehaviour
{
    [SerializeField] private GaugeBarView loadingBar;
    [SerializeField] private TextMeshProUGUI percentTxt;

    public void Init()
    {
        loadingBar.SetRatio(0, GaugeBarType.Width, false);
        percentTxt.text = "0";
    }

    public void SetProgress(float progress)
    {
        // TODO: 로딩 게이지 구현
        loadingBar.SetRatio(progress, GaugeBarType.Width);
        percentTxt.text = ((int)(progress * 100f)).ToString();
    }
}