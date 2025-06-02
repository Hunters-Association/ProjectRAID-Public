using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using ProjectRaid.EditorTools;

public class BossHealthBarTest : MonoBehaviour
{
    [FoldoutGroup("UI", ExtendedColor.White)]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Image mainBar;
    [SerializeField] private Image delayBar;
    public GaugeBarView screenHealthBar;
    public bool isScreenHealthTarget;

    [FoldoutGroup("체력", ExtendedColor.LimeGreen)]
    [SerializeField] private BossHealth health;

    [FoldoutGroup("Tween 설정", ExtendedColor.SeaGreen)]
    [SerializeField] private Ease animEase = Ease.OutQuad;
    [SerializeField] private float delayTime = 0.5f;
    [SerializeField] private float animDuration = 0.5f;

    private bool show;
    private float currentPercent = 1f;
    private Tween delayTween;
    private Coroutine delayRoutine;

    private void Start()
    {
        health.OnHit += UpdateUI;
        canvasGroup.alpha = 0f;

        mainBar.fillAmount = health.hp;
        delayBar.fillAmount = health.hp;
    }

    private void OnDestroy()
    {
        health.OnHit -= UpdateUI;
    }

    private void UpdateUI()
    {
        if (screenHealthBar != null && isScreenHealthTarget)
        {
            screenHealthBar.SetRatio(health.hp / health.maxHP, GaugeBarType.Width);
            return;
        }

        if (!show)
        {
            show = true;
            canvasGroup.DOFade(1f, animDuration).SetEase(animEase);
        }

        SetHealth(health.hp / health.maxHP);
    }
    
    /// <summary>
    /// 체력 상태를 0-1로 설정
    /// </summary>
    public void SetHealth(float normalized)
    {
        normalized = Mathf.Clamp01(normalized);
        currentPercent = normalized;

        mainBar.fillAmount = normalized;

        if (Mathf.Approximately(normalized, 0f))
        {
            if (delayRoutine != null)
                StopCoroutine(delayRoutine);

            delayBar.DOFillAmount(currentPercent, animDuration).SetEase(animEase)
                .OnComplete(() => canvasGroup.DOFade(0f, animDuration).SetEase(animEase));
            return;
        }

        if (delayRoutine != null)
        {
            StopCoroutine(delayRoutine);
        }
        if (delayTween != null && delayTween.IsActive())
        {
            delayTween.Kill();
        }

        delayRoutine = StartCoroutine(DelayedAnimate());

        if (healthText != null)
        {
            // TODO: 몬스터 스크립트와 체력바 사이의 연결 관계 조정 후 마저 구현현
            healthText.text = "";
        }
    }

    private IEnumerator DelayedAnimate()
    {
        yield return new WaitForSeconds(delayTime);

        delayTween = delayBar.DOFillAmount(currentPercent, animDuration)
            .SetEase(animEase);
    }
}