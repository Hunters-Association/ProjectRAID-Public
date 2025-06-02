using System.Collections.Generic;
using UnityEngine;
using ProjectRaid.EditorTools;
using ProjectRaid.Extensions;
using DG.Tweening;

public class DummyBot : MonoBehaviour, IDamageable
{
    [FoldoutGroup("레퍼런스", ExtendedColor.Cyan)]
    [SerializeField] private Animator animator;
    [SerializeField] private ParticleSystem particle;
    [SerializeField] private List<Renderer> renderers;
    [SerializeField] private float duration = 0.25f;

    private readonly List<Material> materials = new();
    private readonly List<Tween> tweens = new();

    private readonly int hashHit = Animator.StringToHash("Hit");
    private readonly string emissionProperty = "_EmissionColor";

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (particle == null) particle = GetComponentInChildren<ParticleSystem>();

        materials.Clear();

        foreach (var renderer in renderers)
        {
            foreach (var material in renderer.materials)
            {
                materials.Add(material);
            }
        }
    }

    public void TakeDamage(DamageInfo info)
    {
        if (animator != null) animator.SetTrigger(hashHit);
        if (particle != null) particle.Play();
        if (renderers != null)
        {
            TweenHue(0f);
            DOVirtual.DelayedCall(duration, () => TweenHue(30f));
            // foreach (var renderer in renderers)
            // {
            //     renderer.DOColorH(emissionProperty, 0f, duration)
            //         .SetEase(Ease.InOutSine);
            // }
        }

        Debug.Log($"더미 인형 타격! ({info.damageAmount:F1} 데미지)");
    }

    private void TweenHue(float targetHue)
    {
        // 기존 트윈 중지
        foreach (var tween in tweens)
        {
            tween.Kill();
        }
        tweens.Clear();

        // 현재 색상 → HSV 변환
        foreach (var mat in materials)
        {
            Color currentColor = mat.GetColor(emissionProperty);
            float intensity = Mathf.Max(currentColor.r, currentColor.g, currentColor.b);
            Color.RGBToHSV(currentColor / intensity, out float h, out float s, out float v);

            float startHue = h;
            float endHue = targetHue / 360f;

            // 트윈 생성
            var t = DOTween.To(() => startHue, x =>
            {
                Color newColor = Color.HSVToRGB(x, s, v, true) * intensity;
                mat.SetColor(emissionProperty, newColor);
            }, endHue, duration).SetEase(Ease.InOutSine);

            tweens.Add(t);
        }
    }
}
