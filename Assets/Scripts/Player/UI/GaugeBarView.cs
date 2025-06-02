using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using ProjectRaid.EditorTools;

public enum GaugeBarType
{
    FillAmount,
    Width,
    Scale,
}

public class GaugeBarView : MonoBehaviour
{
    [FoldoutGroup("게이지 UI", ExtendedColor.Cyan)]
    [SerializeField] private RectTransform fill;
    [SerializeField] private RectTransform delay;
    [SerializeField] private float maxSize;

    [FoldoutGroup("Tween 설정", ExtendedColor.SeaGreen)]
    [SerializeField] private Ease animEase = Ease.OutCubic;
    [SerializeField] private float animTime = 0.25f;
    [SerializeField] private float delayTime = 0.4f;

    private Coroutine delayRoutine;

    /// <summary>
    /// 0~1 비율로 채움 (DOTween 애니메이션)
    /// </summary>
    public void SetRatio(float ratio, GaugeBarType type = GaugeBarType.FillAmount, bool isTween = true, bool isDelay = false)
    {
        var bar = isDelay ? delay : fill;

        if (type is GaugeBarType.FillAmount)
        {
            ratio = Mathf.Clamp01(ratio);
            var image = bar.GetComponent<Image>();

            DOTween.Kill(this);

            if(isTween)
            {
                DOTween.To(() => image.fillAmount, x => image.fillAmount = x, ratio, animTime)
                    .SetEase(animEase)
                    .SetTarget(this);
            }
            else
            {
                image.fillAmount = ratio;
            }
        }
        else if (type is GaugeBarType.Width)
        {
            var size = Mathf.Clamp01(ratio) * maxSize;

            if (Mathf.Approximately(bar.sizeDelta.x, size)) return;

            Vector2 sizeDelta = new Vector2(size, bar.sizeDelta.y);

            if(isTween)
            {
                DOTween.Kill(this);
                bar.DOSizeDelta(sizeDelta, 0.5f)
                    .SetEase(animEase)
                    .SetTarget(this);
            }
            else
            {
                bar.sizeDelta = sizeDelta;
            }
        }
        else if (type is GaugeBarType.Scale)
        {
            // 스케일 모드 구현
        }

        if (delayRoutine != null) StopCoroutine(delayRoutine);
        if (delay != null) delayRoutine = StartCoroutine(DelayedAnimate(ratio, type));
    }

    private IEnumerator DelayedAnimate(float ratio, GaugeBarType type = GaugeBarType.FillAmount)
    {
        yield return new WaitForSeconds(delayTime);
        SetRatio(ratio, type, true);
    }

    private void OnDisable()
    {
        DOTween.Kill(this);
    }

    private void OnDestroy()
    {
        DOTween.Kill(this);
    }
}
