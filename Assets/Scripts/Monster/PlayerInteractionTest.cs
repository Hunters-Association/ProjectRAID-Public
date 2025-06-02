using TMPro;
using UnityEngine;

public class PlayerInteractionTest : MonoBehaviour
{
    [Header("상호작용 감지 설정")]
    [SerializeField]
    [Tooltip("상호작용 가능한 최대 거리")]
    private float interactionRadius = 3.0f;

    [SerializeField]
    [Tooltip("상호작용 대상을 감지할 레이어 마스크 (Interactable 레이어 권장)")]
    private LayerMask interactableLayerMask; // 상호작용 대상 레이어

    [Header("UI 설정")]
    [SerializeField]
    [Tooltip("상호작용 안내 문구를 표시할 UI Text 또는 TextMeshProUGUI 컴포넌트")]
    private TextMeshProUGUI interactionPromptText; // UI Text 컴포넌트 참조 (Inspector 연결)
    // public TMPro.TextMeshProUGUI interactionPromptText;

    

    /// <summary>
    /// 매 프레임 호출되어 주변 상호작용 대상을 찾고 UI를 업데이트합니다.
    /// </summary>
    void Update()
    {
        // 주변에서 가장 가까운 상호작용 가능 객체를 찾습니다.
        IInteractable nearestInteractable = FindNearestInteractable();
        // 찾은 객체를 기반으로 UI 안내 문구를 업데이트합니다.
        UpdateInteractionPrompt(nearestInteractable);
    }

    /// <summary>
    /// 플레이어 주변에서 가장 가까운 IInteractable 오브젝트를 찾아 반환합니다.
    /// </summary>
    /// <returns>가장 가까운 IInteractable 객체 또는 null</returns>
    private IInteractable FindNearestInteractable()
    {
        IInteractable nearest = null;
        float minDistanceSqr = interactionRadius * interactionRadius;
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, interactionRadius, interactableLayerMask);

        foreach (var col in nearbyColliders) // col은 Collider 타입
        {
            IInteractable interactable = null; // 찾은 인터페이스 저장 변수

            // ★★★ 수정: 부모 먼저 탐색, 없으면 자신 탐색 ★★★
            // 1. 부모 GameObject들로 올라가면서 IInteractable 찾기 시도
            interactable = col.GetComponentInParent<IInteractable>();

            // 2. 부모에게서 찾지 못했고(interactable == null), Collider가 붙은 GameObject가 IInteractable을 가지고 있다면 그것을 사용
            //    (자기 자신에게도 없고 부모에게도 없는 경우를 명확히 하기 위해)
            if (interactable == null)
            {
                // 자기 자신에게서 IInteractable 찾기 시도
                col.TryGetComponent<IInteractable>(out interactable);
                // 만약 TryGetComponent가 성공하면 interactable은 유효한 참조, 실패하면 null
            }
            // ★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★

            // interactable을 찾았다면 (부모 또는 자신에게서) 거리 계산 및 가장 가까운 대상 업데이트
            if (interactable != null)
            {
                // TODO: 추가 조건 체크
                float distanceSqr = (col.transform.position - transform.position).sqrMagnitude;
                if (distanceSqr < minDistanceSqr)
                {
                    minDistanceSqr = distanceSqr;
                    nearest = interactable;
                }
            }
        }
        return nearest;
    }

    /// <summary>
    /// 감지된 상호작용 대상에 따라 UI 안내 문구를 업데이트합니다.
    /// </summary>
    /// <param name="interactable">감지된 IInteractable 객체 (없으면 null)</param>
    private void UpdateInteractionPrompt(IInteractable interactable)
    {
        if (interactionPromptText == null) return;

        if (interactable != null)
        {
            string targetName = "대상"; // 기본 이름
            string actionText = "상호작용"; // 기본 액션 텍스트
            KeyCode interactionKey = KeyCode.E; // 기본 키 (실제 사용하는 키로 변경 필요)

            // 대상의 MonoBehaviour 컴포넌트 가져오기 (이름 등 접근 위함)
            MonoBehaviour mb = interactable as MonoBehaviour;
            if (mb != null)
            {
                targetName = mb.gameObject.name; // 기본적으로는 상호작용 오브젝트 이름 사용

                
                if (interactable is NPCQuestInteraction)
                {
                    actionText = "대화하기";
                    // 필요하다면 NPC 이름 설정: targetName = npc.npcName; (NPCQuestInteraction에 이름 필드 필요)
                }
                
                else if (interactable is CorpseInteractionTrigger corpseTrigger) // <<< 타입 체크 변경
                {
                    // CorpseInteractionTrigger의 부모 Monster 가져오기
                    Monster parentMonster = corpseTrigger.GetComponentInParent<Monster>(); // 부모에서 Monster 찾기

                    if (parentMonster != null && parentMonster.IsDead() && parentMonster.CanBeGathered)
                    {
                        // Monster의 갈무리 텍스트와 이름 사용
                        actionText = parentMonster.gatherInteractionText ?? "갈무리하기";
                        targetName = parentMonster.monsterData?.monsterName ?? targetName;
                    }
                    else
                    {
                        // 부모 몬스터를 못 찾거나 갈무리 불가능 상태면 상호작용 안 함
                        // (이 경우는 FindNearestInteractable에서 걸러졌어야 함)
                        interactable = null; // 상호작용 불가능 처리
                    }
                }
                // ★★★★★★★★★★★★★★★★★★★★★★★★★

                else if (interactable is HealingHerb)
                {
                    actionText = "사용";
                    targetName = "회복초";
                }
                else if (interactable is GlowingHealingHerb)
                {
                    actionText = "사용";
                    targetName = "빛나는 회복초";
                }
                else if (interactable is VineInteractable)
                {
                    actionText = "줄기 타기";
                    targetName = "덩굴";
                }
                // ... 다른 IInteractable 타입들 추가 ...
                else if(interactable is LostArticle)
                {
                    actionText = "줍기";
                    targetName = "유실물";
                }
                else if (interactable is BossInteratable)
                {
                    actionText = "갈무리";
                    targetName = "";
                }
            }
            // ★★★★★★★★★★★★★★★★★★★★★★★★★★★★

            // 최종 UI 텍스트 설정 (interactable이 유효할 때만)
            if (interactable != null)
            {
                // TODO: 실제 상호작용 키 가져오기
                interactionPromptText.text = $"[{interactionKey}] {targetName} {actionText}";
                interactionPromptText.enabled = true;
            }
            else // 위에서 상호작용 불가능 처리된 경우
            {
                interactionPromptText.text = "";
                interactionPromptText.enabled = false;
            }
        }
        // 상호작용 가능한 대상이 없을 때
        else
        {
            interactionPromptText.text = "";
            interactionPromptText.enabled = false;
        }
    }

    // --- TryInteract, HandleHighlighting 등 상호작용 실행/하이라이트 관련 함수 제거 ---
    // private void TryInteract() { ... }
    // private void HandleHighlighting() { ... }

    // 기즈모는 유지 (범위 확인용)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}

