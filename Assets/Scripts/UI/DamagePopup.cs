using UnityEngine;
using TMPro;
using DG.Tweening;

/// <summary>
/// 데미지 숫자를 띄우고 애니메이션 후 자동 반환하는 컴포넌트
/// </summary>
public class DamagePopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] normalTexts;
    [SerializeField] private TextMeshProUGUI[] criticalTexts;
    [SerializeField] private float floatUpDistance = 1f;
    [SerializeField] private float duration = 0.8f;

    private Sequence animSequence;

    public void Show(float damage, bool isCritical, Vector3 position)
    {
        GetComponent<Billboard>().SetCamera();
        GetComponent<CanvasCameraAssigner>().AssignCamera();

        transform.position = position;

        var damageTexts = isCritical ? criticalTexts : normalTexts;

        foreach (var text in normalTexts)
            text.gameObject.SetActive(!isCritical);

        foreach (var text in criticalTexts)
            text.gameObject.SetActive(isCritical);

        foreach (var text in damageTexts)
            text.text = Mathf.RoundToInt(damage).ToString();

        transform.localScale = Vector3.zero;

        animSequence?.Kill();
        animSequence = DOTween.Sequence()
            .Append(transform.DOScale(Vector3.one * 0.01f, 0.15f).SetEase(Ease.OutBack))
            .Join(transform.DOMoveY(position.y + floatUpDistance, duration))
            .AppendCallback(() => ReturnToPool());
    }

    private void ReturnToPool()
    {
        GameManager.Instance.DamagePopup.ReturnToPool(this);
    }
}
