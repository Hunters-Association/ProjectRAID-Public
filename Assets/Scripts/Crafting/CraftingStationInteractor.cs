using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider))] // 상호작용 감지를 위해 Collider 필요
public class CraftingStationInteractor : MonoBehaviour, IInteractable
{
    [Header("상호작용 설정")]
    [Tooltip("플레이어에게 보여줄 상호작용 안내 문구")]
    [SerializeField] private string interactionPrompt = "제작하기"; // 인스펙터에서 수정 가능

    [Tooltip("이 제작대와 상호작용 시 열릴 초기 UI의 타입 이름 (UIManager에서 사용)")]
    [SerializeField] private string initialUIToOpen = "InitialCraftingMenu_UI"; // 기본값, 필요시 변경

    // 하이라이트 효과를 위한 참조 (선택적)
    [Header("하이라이트 (선택적)")]
    [SerializeField] private Outline outlineEffect; // 또는 다른 하이라이트 컴포넌트

    private Collider interactionCollider;

    private void Awake()
    {
        interactionCollider = GetComponent<Collider>();
        // 상호작용을 위해 콜라이더가 Trigger 모드인지 확인 (권장)
        if (interactionCollider != null)
        {
            if (!interactionCollider.isTrigger)
            {
                Debug.LogWarning($"CraftingStationInteractor ({gameObject.name}): Collider is not set to IsTrigger. Interaction might not work as expected depending on PlayerInteraction detection method.", this);
            }
        }
        else
        {
            Debug.LogError($"CraftingStationInteractor ({gameObject.name}) requires a Collider component!", this);
        }

        // 아웃라인 컴포넌트 찾기 (없어도 오류 안 나게)
        if (outlineEffect == null)
        {
            outlineEffect = GetComponentInChildren<Outline>(true); // 자식 포함 검색 시도
        }
        HideHighlight(); // 시작 시 하이라이트 끄기
    }

    // --- IInteractable 인터페이스 구현 ---

    /// <summary>
    /// 플레이어가 이 제작대와 상호작용할 때 호출됩니다.
    /// UIManager를 통해 지정된 초기 제작 UI를 엽니다.
    /// </summary>
    /// <param name="player">상호작용한 플레이어의 컨트롤러.</param>
    public void Interact(PlayerController player) // ★ PlayerController 타입 사용 ★
    {
        if (player == null) return;

        Debug.Log($"플레이어 '{player.gameObject.name}'가 제작대 '{gameObject.name}'와 상호작용 시작.");

        // UIManager 인스턴스를 통해 UI 표시 요청
        if (UIManager.Instance != null)
        {
            // ★ initialUIToOpen 문자열에 해당하는 타입의 UI를 열도록 시도 ★
            // UIManager에 string으로 UI를 여는 메서드가 필요하거나,
            // 또는 타입을 직접 지정하는 방식 사용
            // 예시 1: 타입 직접 지정 (더 안전함)
            if (initialUIToOpen == typeof(InitialCraftingMenu_UI).Name)
            {
                UIManager.Instance.ShowUI<InitialCraftingMenu_UI>();
            }
            else if (initialUIToOpen == typeof(WeaponCrafting_UI).Name) // 바로 무기 제작 열기
            {
                UIManager.Instance.ShowUI<WeaponCrafting_UI>();
            }
            // ... 다른 UI 타입 처리 ...
            else
            {
                Debug.LogError($"CraftingStationInteractor: 열려고 하는 UI 타입({initialUIToOpen})을 처리할 수 없습니다.", this);
            }

            // 예시 2: UIManager에 string 기반 ShowUI 메서드가 있다고 가정 (덜 안전)
            // UIManager.Instance.ShowUI(initialUIToOpen);
        }
        else
        {
            Debug.LogError("UIManager 인스턴스를 찾을 수 없습니다! UI를 열 수 없습니다.");
        }

        // 선택적: CraftingSystem에 현재 제작대 정보 전달
        // CraftingSystem craftingSystem = FindObjectOfType<CraftingSystem>(); // 비권장
        // craftingSystem?.SetCurrentCraftingStation(this.gameObject);
    }

    /// <summary>
    /// 상호작용 가능한 상태일 때 하이라이트 효과를 표시합니다.
    /// </summary>
    public void ShowHighlight()
    {
        if (outlineEffect != null)
        {
            outlineEffect.enabled = true;
        }
        // 다른 하이라이트 로직 추가 가능
    }

    /// <summary>
    /// 하이라이트 효과를 숨깁니다.
    /// </summary>
    public void HideHighlight()
    {
        if (outlineEffect != null)
        {
            outlineEffect.enabled = false;
        }
        // 다른 하이라이트 로직 추가 가능
    }


    [SerializeField] private InteractableData data;

    public InteractableData GetInteractableData()
    {
        if (data == null) Debug.LogWarning("[CraftingStationInteractor] 상호작용 데이터가 등록되지 않았습니다.");
        return data;
    }


    /// <summary>
    /// 플레이어가 이 제작대와 상호작용 가능한지 여부를 반환합니다.
    /// </summary>
    public bool CanInteract(GameObject interactor)
    {
        // 항상 상호작용 가능하게 하거나, 특정 조건(예: 퀘스트 완료) 추가 가능
        return true;
    }

    /// <summary>
    /// UI에 표시될 상호작용 안내 문구를 반환합니다.
    /// </summary>
    public string GetInteractionPrompt(GameObject interactor)
    {
        // TODO: 실제 게임의 상호작용 키를 가져오는 로직 필요
        KeyCode key = KeyCode.E; // 임시 키
        return $"[{key}] {interactionPrompt}"; // 인스펙터에서 설정한 텍스트 사용
    }

    // --- IInteractable 인터페이스 구현 끝 ---
}
