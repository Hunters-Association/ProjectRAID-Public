using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class MinimapBlip : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private BlipSettings settings;

    private MinimapSystem minimap;
    private RectTransform rectTransform;
    private Image image;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
    }

    public void Initialize(Transform target, MinimapSystem minimap, BlipSettings settings)
    {
        this.target = target;
        this.minimap = minimap;
        this.settings = settings;
        image.sprite = settings.icon;
        image.color = settings.color;
    }

    public void UpdateBlip(Vector3 minimapCenter, float mapScale, float minimapRotation, RectTransform minimapTransform)
    {
        if (target == null)
        {
            gameObject.SetActive(false);
            return;
        }

        Vector3 offset = target.position - minimapCenter;
        offset.y = 0f;
        Vector2 blipPos = new Vector2(offset.x, offset.z) * mapScale;
        float distance = blipPos.magnitude;

        float minimapRadius = minimapTransform.rect.width * 0.5f;

        // 지도 범위 안에 있을 때
        if (distance < minimapRadius - 10f)
        {
            rectTransform.anchoredPosition = blipPos;
        }
        else
        {
            Vector2 clampedPos = blipPos.normalized * (minimapRadius - 10f); // 약간 안쪽
            rectTransform.anchoredPosition = clampedPos;
        }

        // 회전 처리
        if (minimap.RotateWithTarget)
        {
            Vector3 rotation = new(0f, 0f, -minimapRotation);
            rectTransform.localEulerAngles = rotation;
        }
        else
        {
            rectTransform.localEulerAngles = Vector3.zero;
        }

        // 거리 투명도 처리
        float alpha = 1f;

        if (offset.magnitude > minimapRadius)
        {
            alpha = Mathf.Clamp01(1f - (offset.magnitude - minimapRadius) / (minimap.MaxVisibleDistance - minimapRadius));
        }

        var color = image.color;
        color.a = alpha;
        image.color = color;
    }
}