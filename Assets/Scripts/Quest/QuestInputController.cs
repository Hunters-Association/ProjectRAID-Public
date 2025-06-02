using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestInputController : MonoBehaviour
{
    [Header("Interaction Settings")]
    public KeyCode interactionKey = KeyCode.F; // 상호작용 키 변경
    public float interactionDistance = 2.5f; // 상호작용 거리 조정
    public LayerMask interactableLayerMask; // 상호작용 대상 레이어 (NPC, MonsterCorpse 등)

    [Header("UI Settings")]
    public Text interactionPromptText; // 상호작용 안내 UI
    // public TMPro.TextMeshProUGUI interactionPromptText;

    private IInteractable currentInteractable = null; // 현재 상호작용 가능한 대상
    private IInteractable previousInteractable = null; // 이전에 상호작용 가능했던 대상

    void Update()
    {
        FindInteractable(); // 매 프레임 상호작용 대상 탐색
        HandleHighlighting(); // 하이라이트 처리
        HandleInteractionInput(); // 입력 처리
    }

    /// <summary> 주변에서 상호작용 가능한 IInteractable 객체를 찾습니다. </summary>
    private void FindInteractable()
    {
        previousInteractable = currentInteractable; // 이전 대상 저장
        currentInteractable = null; // 일단 초기화
        float closestDistSqr = interactionDistance * interactionDistance;

        // 주변 콜라이더 탐색 (OverlapSphere 사용)
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, interactionDistance, interactableLayerMask);

        foreach (var col in nearbyColliders)
        {
            // ★★★ IInteractable 인터페이스 컴포넌트 가져오기 ★★★
            IInteractable interactable = col.GetComponentInParent<IInteractable>(); // 부모에서도 찾기

            if (interactable != null)
            {
                // TODO: 추가적인 상호작용 가능 조건 체크 (예: 몬스터가 죽었는지 등)
                // Monster monster = col.GetComponentInParent<Monster>();
                // if (monster != null && !(monster.CurrentStateEnum == MonsterState.Dead && monster.CanBeGathered)) {
                //     continue; // 갈무리 불가능한 몬스터는 제외
                // }

                float distSqr = (col.transform.position - transform.position).sqrMagnitude;
                if (distSqr < closestDistSqr)
                {
                    closestDistSqr = distSqr;
                    currentInteractable = interactable; // 가장 가까운 대상 저장
                }
            }
        }
    }

    /// <summary> 상호작용 대상 변경에 따라 하이라이트 효과를 관리합니다. </summary>
    private void HandleHighlighting()
    {
        // 이전 대상과 현재 대상이 다르면
        if (previousInteractable != currentInteractable)
        {
            // 이전에 대상이 있었다면 하이라이트 숨김
            previousInteractable?.HideHighlight();
            // 현재 새로운 대상이 있다면 하이라이트 표시
            currentInteractable?.ShowHighlight();
        }
        // UI 업데이트
        UpdateInteractionPrompt();
    }

    /// <summary> 상호작용 키 입력을 처리합니다. </summary>
    private void HandleInteractionInput()
    {
        // 상호작용 키를 눌렀고, 상호작용 가능한 대상이 있다면
        if (Input.GetKeyDown(interactionKey) && currentInteractable != null)
        {
            TryInteract();
        }
    }

    /// <summary> 현재 상호작용 가능한 대상과 상호작용을 시도합니다. </summary>
    private void TryInteract()
    {
        if (currentInteractable == null) return;

        Debug.Log($"Player attempting to interact with {((MonoBehaviour)currentInteractable).gameObject.name}");
        // ★★★ IInteractable 인터페이스의 Interact 메서드 호출 ★★★
        // PlayerController 참조를 전달해야 함 (this)
        currentInteractable.Interact(GetComponent<PlayerController>()); // <<< PlayerController 참조 전달

        // 상호작용 후 하이라이트 즉시 제거 (선택적)
        currentInteractable.HideHighlight();
        currentInteractable = null; // 한 번 상호작용 후 대상 해제
        UpdateInteractionPrompt();
    }

    /// <summary> 상호작용 안내 UI를 업데이트합니다. </summary>
    private void UpdateInteractionPrompt()
    {
        if (interactionPromptText == null) return;

        if (currentInteractable != null)
        {
            // 인터페이스를 구현한 MonoBehaviour에서 게임 오브젝트 이름 가져오기
            string targetName = ((MonoBehaviour)currentInteractable).gameObject.name;
            // TODO: 인터페이스에 GetInteractionPromptText() 같은 메서드를 추가하여
            //       대상마다 다른 상호작용 텍스트를 표시하도록 개선 가능
            //       예: npcInteraction.GetInteractionPromptText() -> "대화하기"
            //           monsterCorpse.GetInteractionPromptText() -> "갈무리하기"
            string interactionType = "상호작용"; // 기본 텍스트
            if (currentInteractable is NPCQuestInteraction) interactionType = "대화하기";
            else if (currentInteractable is Monster m && m.CurrentStateEnum == MonsterState.Dead) interactionType = m.gatherInteractionText; // Monster의 텍스트 사용

            interactionPromptText.text = $"[{interactionKey}] {targetName} {interactionType}";
            interactionPromptText.enabled = true;
        }
        else
        {
            interactionPromptText.text = "";
            interactionPromptText.enabled = false;
        }
    }

    // 기즈모 표시는 이전과 동일
    private void OnDrawGizmosSelected() { Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, interactionDistance); }
}
