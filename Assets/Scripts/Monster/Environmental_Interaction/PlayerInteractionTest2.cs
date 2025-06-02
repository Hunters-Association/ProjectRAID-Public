using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInteractionTest2 : MonoBehaviour
{
    [Header("상호작용 설정")]
    [SerializeField] private KeyCode interactionKey = KeyCode.E; // 상호작용 키
    [SerializeField] private float interactionRadius = 3.0f; // 상호작용 가능 반경
    [SerializeField] private LayerMask interactableLayerMask; // 상호작용 가능한 레이어

    [Header("UI 설정 (선택적)")]
    [SerializeField] private Text interactionPromptText; // UI Text 참조
    // public TMPro.TextMeshProUGUI interactionPromptText; // TextMeshPro 사용

    private IInteractableTest currentInteractable = null; // 현재 상호작용 가능한 객체

    void Update()
    {
        FindClosestInteractable(); // 주변 상호작용 객체 탐색
        UpdateInteractionPrompt(); // UI 업데이트

        // 상호작용 키 입력 처리
        if (Input.GetKeyDown(interactionKey) && currentInteractable != null)
        {
            currentInteractable.Interact(gameObject); // 감지된 객체의 Interact 메서드 호출
        }
    }

    /// <summary>
    /// 주변에서 가장 가까운 IInteractable 객체를 찾음
    /// </summary>
    void FindClosestInteractable()
    {
        currentInteractable = null; // 초기화
        float closestDistSqr = interactionRadius * interactionRadius; // 제곱 거리 비교

        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, interactionRadius, interactableLayerMask);

        foreach (var col in nearbyColliders)
        {
            // ★ IInteractable 인터페이스를 구현한 컴포넌트를 찾음
            IInteractableTest interactable = col.GetComponent<IInteractableTest>();
            // GetComponentInChildren 또는 GetComponentInParent 사용 고려 가능

            if (interactable != null) // 인터페이스를 찾았다면
            {
                float distSqr = (col.transform.position - transform.position).sqrMagnitude;
                if (distSqr < closestDistSqr) // 가장 가까운지 확인
                {
                    closestDistSqr = distSqr;
                    currentInteractable = interactable; // 가장 가까운 객체 저장
                }
            }
        }
    }

    /// <summary>
    /// 상호작용 안내 UI를 업데이트합니다.
    /// </summary>
    void UpdateInteractionPrompt()
    {
        if (interactionPromptText == null) return;

        if (currentInteractable != null)
        {
            // ★ 인터페이스의 프로퍼티를 통해 안내 문구 가져오기
            interactionPromptText.text = currentInteractable.InteractionPrompt;
            interactionPromptText.enabled = true;
        }
        else
        {
            interactionPromptText.text = "";
            interactionPromptText.enabled = false;
        }
    }

    // 범위 표시 기즈모 
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
