using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider))] // 이 오브젝트에 반드시 Collider 필요
public class CorpseInteractionTrigger : MonoBehaviour, IInteractable
{
    [Header("참조")]
    [Tooltip("부모 오브젝트의 Monster 스크립트 참조 (자동으로 찾거나 직접 할당)")]
    [SerializeField] // 인스펙터에서도 보이도록 SerializeField 추가
    private Monster parentMonster; // 부모 Monster 스크립트 참조

    // (선택적) 하이라이트 효과 컴포넌트 (부모 또는 이 오브젝트에 있을 수 있음)
    [Header("하이라이트 (선택적)")]
    [SerializeField] private Outline outlineEffect;

    void Awake()
    {
        // 부모 Monster 스크립트 자동으로 찾기 (없으면 경고)
        if (parentMonster == null)
        {
            parentMonster = GetComponentInParent<Monster>();
        }
        if (parentMonster == null)
        {
            Debug.LogError("CorpseInteractionTrigger requires a Monster component in its parent hierarchy!", this);
            enabled = false; // Monster 없으면 비활성화
            return;
        }

        // 이 오브젝트의 Collider를 Trigger로 설정 (상호작용 감지용)
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        else
        {
            Debug.LogError("CorpseInteractionTrigger requires a Collider component on this GameObject!", this);
        }

        // 하이라이트 컴포넌트 찾기 (선택적)
        if (outlineEffect == null) outlineEffect = GetComponentInParent<Outline>(); // 부모에서 찾기 시도
        if (outlineEffect != null) outlineEffect.enabled = false;

        // 시작 시에는 비활성화 상태일 수 있음 (몬스터가 살아있을 때)
        // 또는 부모 Monster의 상태에 따라 활성화/비활성화 제어 필요
        UpdateInteractableState();
    }

    /// <summary> 부모 몬스터의 상태에 따라 상호작용 가능 여부 업데이트 </summary>
    public void UpdateInteractableState()
    {
        bool canInteract = CanInteractNow();
        // 콜라이더 활성화/비활성화 (상호작용 가능할 때만 켜기)
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = canInteract;

        // 하이라이트도 상태에 따라 갱신 (선택적)
        if (!canInteract) HideHighlight();
    }

    /// <summary> 현재 상호작용(갈무리)이 가능한 상태인지 확인 </summary>
    private bool CanInteractNow()
    {
        // 부모 Monster가 있고, 죽었고, 갈무리 가능 상태일 때
        return parentMonster != null && parentMonster.IsDead() && parentMonster.CanBeGathered;
    }

    // --- IInteractable 구현 ---

    public void Interact(PlayerController player)
    {
        // 최종적으로 상호작용 가능한지 다시 확인
        if (!CanInteractNow()) return;

        Debug.Log($"{player.gameObject.name} interacts with corpse trigger for {parentMonster.gameObject.name}");

        // ★★★ 부모 Monster의 갈무리 로직 호출 ★★★
        // Monster 클래스의 Interact 메서드를 호출하거나 필요한 함수 직접 호출
        // parentMonster.Interact(player.gameObject); // Monster의 Interact가 GameObject를 받는다면
        // 또는 필요한 함수 직접 호출:
        parentMonster.ProcessDropTable(player.gameObject);
        parentMonster.SetGatherable(false); // 순서 변경 가능
        parentMonster.NotifyInteractionStart(player.gameObject);
        parentMonster.gameObject.SetActive(false); // 예시: 부모 오브젝트 비활성화

        // 상호작용 후 이 트리거도 비활성화
        this.enabled = false; // 스크립트 비활성화
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        HideHighlight();
    }

    public void ShowHighlight()
    {
        // 상호작용 가능할 때만 하이라이트
        if (CanInteractNow())
        {
            if (outlineEffect != null) outlineEffect.enabled = true;
        }
    }

    public void HideHighlight()
    {
        if (outlineEffect != null) outlineEffect.enabled = false;
    }

    public bool CanInteract(GameObject interactor) // interactor는 여기선 사용 안 함
    {
        return CanInteractNow(); // 내부 상태 확인 메서드 재사용
    }


    [SerializeField] private InteractableData data;

    public InteractableData GetInteractableData()
    {
        if (data == null) Debug.LogWarning("[CorpseInteractionTrigger] 상호작용 데이터가 등록되지 않았습니다.");
        return data;
    }

    /// <summary>
    /// 상호작용 UI에 표시될 안내 문구를 생성하여 반환합니다.
    /// </summary>
    public string GetInteractionPrompt(GameObject interactor)
    {
        if (CanInteractNow()) // 상호작용 가능할 때만 텍스트 생성
        {
            // 부모 몬스터 데이터에서 이름과 액션 텍스트 가져오기
            string targetName = parentMonster?.monsterData?.monsterName ?? "사체"; // 몬스터 이름 없으면 기본값
            string actionText = parentMonster?.gatherInteractionText ?? "갈무리하기"; // 설정된 갈무리 텍스트 없으면 기본값

            // ★★★ 오류 수정: interactionKey 변수 선언 추가 ★★★
            // 실제 상호작용에 사용하는 키 코드로 설정해야 합니다.
            // PlayerInput 등 다른 시스템에서 가져오는 것이 더 좋습니다.
            KeyCode interactionKey = KeyCode.E;
            // ★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★

            // 최종 프롬프트 문자열 조합
            return $"[{interactionKey}] {targetName} {actionText}"; // 이제 interactionKey 변수를 인식합니다.
        }
        else
        {
            return string.Empty; // 상호작용 불가능 시 빈 문자열 반환
        }
    }
}
