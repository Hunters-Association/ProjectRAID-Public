using UnityEngine;
using DG.Tweening;

namespace ProjectRaid.Extensions
{
    public static class ColorExtensions
    {
        public static Tween DOColorHSV(this Renderer renderer, string propertyName, Vector3 targetHSV, float duration)
        {
            MaterialPropertyBlock mpb = new();
            renderer.GetPropertyBlock(mpb);
            Color currentColor = mpb.GetColor(propertyName);
            Color.RGBToHSV(currentColor, out float h, out float s, out float v);
            Vector3 startHSV = new(h, s, v);

            return DOTween.To(() => startHSV, x =>
            {
                Color newColor = Color.HSVToRGB(x.x, x.y, x.z, true);
                mpb.SetColor(propertyName, newColor);
                renderer.SetPropertyBlock(mpb);
            }, targetHSV, duration);
        }

        public static Tween DOColorH(this Renderer renderer, string propertyName, float targetH, float duration)
        {
            MaterialPropertyBlock mpb = new();
            renderer.GetPropertyBlock(mpb);
            Color currentColor = mpb.GetColor(propertyName);

            // 1. Intensity 계산 (최댓값 기반)
            float intensity = Mathf.Max(currentColor.r, currentColor.g, currentColor.b);

            // 2. Intensity를 나눈 Normalized RGB → HSV 변환
            Color.RGBToHSV(currentColor / intensity, out float h, out float s, out float v);

            float startH = h;

            return DOTween.To(() => startH, x =>
            {
                // 3. HSV → RGB (HDR 지원)
                Color newColor = Color.HSVToRGB(x, s, v, true);

                // 4. 다시 Intensity 곱해서 밝기 복원
                newColor *= intensity;

                // 5. 적용
                mpb.SetColor(propertyName, newColor);
                renderer.SetPropertyBlock(mpb);
            }, targetH, duration);
        }

        public static Tween DOColorS(this Renderer renderer, string propertyName, float targetS, float duration)
        {
            MaterialPropertyBlock mpb = new();
            renderer.GetPropertyBlock(mpb);
            Color currentColor = mpb.GetColor(propertyName);
            Color.RGBToHSV(currentColor, out float h, out float s, out float v);
            float startS = s;

            return DOTween.To(() => startS, x =>
            {
                Color newColor = Color.HSVToRGB(h, x, v, true);
                mpb.SetColor(propertyName, newColor);
                renderer.SetPropertyBlock(mpb);
            }, targetS, duration);
        }

        public static Tween DOColorV(this Renderer renderer, string propertyName, float targetV, float duration)
        {
            MaterialPropertyBlock mpb = new();
            renderer.GetPropertyBlock(mpb);
            Color currentColor = mpb.GetColor(propertyName);
            Color.RGBToHSV(currentColor, out float h, out float s, out float v);
            float startV = v;

            return DOTween.To(() => startV, x =>
            {
                Color newColor = Color.HSVToRGB(h, s, x, true);
                mpb.SetColor(propertyName, newColor);
                renderer.SetPropertyBlock(mpb);
            }, targetV, duration);
        }
    }
}
