using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System;

[RequireComponent(typeof(CanvasGroup))]
public class TutorialText : MonoBehaviour
{
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] TextMeshProUGUI textTMP;

    public Ease fadeEase;
    public float fadeDuration;

    public void Enable() => canvasGroup.alpha = 1;
    public void Disable() => canvasGroup.alpha = 0;

    public void Init()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (textTMP == null) textTMP = GetComponentInChildren<TextMeshProUGUI>();

        Disable();
    }

    public Sequence ShowText(float showDuration = 0.5f, Action onComplete = null)
    {
        Sequence showTextSequence = DOTween.Sequence();

        showTextSequence.Append(FadeInText());
        showTextSequence.AppendInterval(showDuration);
        showTextSequence.Append(FadeOutText());

        if (onComplete != null)
            showTextSequence.OnComplete(()=>onComplete?.Invoke());

        return showTextSequence;
    }

    public Tween FadeInText()
    {
        if (canvasGroup.alpha != 0) canvasGroup.alpha = 0;

        return canvasGroup.DOFade(1, fadeDuration)
            .SetEase(fadeEase);
    }

    public Tween FadeOutText()
    {
        if (canvasGroup.alpha != 1) canvasGroup.alpha = 1;

        return canvasGroup.DOFade(0, fadeDuration)
            .SetEase(fadeEase);
    }

    public void SetText(string text)
    {
        textTMP.text = text;
    }
}
