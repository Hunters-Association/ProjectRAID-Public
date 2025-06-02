using UnityEngine;
using UnityEngine.UI;

// ★★★ IInteractable 인터페이스 구현 ★★★
[RequireComponent(typeof(Collider))]
public class HealingHerb : MonoBehaviour, IInteractable // <<< 인터페이스 이름 수정 및 구현
{
    [Header("회복초 설정")]
    [SerializeField] private float healRadius = 1.5f; // 회복 반경 약간 증가
    [SerializeField] private int healAmount = 50;    // 회복량
    // [SerializeField] private LayerMask playerLayerMask; // OverlapSphere에서 사용 가능하나, 여기서는 불필요할 수 있음
    [SerializeField] private float respawnTime = 30.0f; // 리스폰 시간 (0 이하면 리스폰 안 함)
    [SerializeField] private GameObject gatherEffectPrefab; // 채집 시 효과 (선택적)
    [SerializeField] private AudioClip gatherSound;       // 채집 시 사운드 (선택적)

    private Collider interactCollider;
    private MeshRenderer meshRenderer; // 시각적 비활성화를 위해 추가 (선택적)
    private bool isAvailable = true; // 현재 채집 가능한지 여부

    // 아웃라인 효과 등을 위한 컴포넌트 참조 (선택적)
    [Header("하이라이트 (선택적)")]
    [SerializeField] private Outline outlineEffect; // 예시: Outline 컴포넌트 사용

    // --- 초기화 ---
    private void Awake()
    {
        interactCollider = GetComponent<Collider>();
        if (interactCollider != null) interactCollider.isTrigger = true; // 상호작용은 Trigger 기반 권장
        else Debug.LogError("HealingHerb에 Collider가 없습니다!", this);

        meshRenderer = GetComponentInChildren<MeshRenderer>(); // 자식 포함해서 찾기
        if (meshRenderer == null) Debug.LogWarning("HealingHerb에 MeshRenderer가 없어 시각적 비활성화가 안 될 수 있습니다.", this);

        outlineEffect = GetComponent<Outline>(); // 아웃라인 컴포넌트 가져오기
        if (outlineEffect != null) outlineEffect.enabled = false; // 기본 비활성화
    }

    // --- IInteractable 구현 ---

    // ★★★ IInteractable.Interact 구현 ★★★
    /// <summary>
    /// 플레이어가 회복초와 상호작용했을 때 호출됩니다.
    /// </summary>
    /// <param name="player">상호작용을 시도한 플레이어 컨트롤러</param>
    public void Interact(PlayerController player) // <<< 파라미터 PlayerController
    {
        // 사용 불가능 상태면 리턴
        if (!isAvailable)
        {
            Debug.Log("이 회복초는 이미 사용되었거나 아직 재생성되지 않았습니다.");
            // TODO: 상호작용 불가 피드백 (소리 등)
            return;
        }

        Debug.Log($"[{Time.timeSinceLevelLoad:F1}s] 플레이어 {player.gameObject.name}이(가) 회복초 사용! HP {healAmount} 회복.");
        player.Stats.Heal(healAmount);

        // 플레이어의 체력 회복 로직 호출
        // PlayerHealth 컴포넌트를 가져와서 회복 함수 호출
        //PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        //if (playerHealth != null)
        //{
        //    // playerHealth.Heal(healAmount); // PlayerHealth에 Heal 메서드 구현 필요
        //    Debug.Log($" - {player.gameObject.name} HP 회복 실행 (PlayerHealth.Heal 호출 필요)");
        //}
        //else
        //{
        //    Debug.LogWarning($"Player '{player.gameObject.name}' is missing PlayerHealth component. Cannot heal.");
        //}

        // 시각/청각 효과 재생
        if (gatherEffectPrefab != null) Instantiate(gatherEffectPrefab, transform.position, Quaternion.identity);
        // if (gatherSound != null) AudioSource.PlayClipAtPoint(gatherSound, transform.position); // 간단한 사운드 재생

        // 비활성화 및 리스폰 처리
        StartRespawnTimer();
    }
    // ★★★★★★★★★★★★★★★★★★

    // ★★★ IInteractable.ShowHighlight 구현 ★★★
    public void ShowHighlight()
    {
        // 사용 가능할 때만 하이라이트 표시
        if (isAvailable)
        {
            if (outlineEffect != null) outlineEffect.enabled = true;
        }
    }
    // ★★★★★★★★★★★★★★★★★★★★

    // ★★★ IInteractable.HideHighlight 구현 ★★★
    public void HideHighlight()
    {
        if (outlineEffect != null) outlineEffect.enabled = false;
    }


    [SerializeField] private InteractableData data;

    public InteractableData GetInteractableData()
    {
        if (data == null) Debug.LogWarning("[HealingHerb] 상호작용 데이터가 등록되지 않았습니다.");
        return data;
    }
    
    // ★★★★★★★★★★★★★★★★★★★★

    // ★★★ InteractionPrompt 프로퍼티 제거 또는 수정 ★★★
    // PlayerInteractionController에서 텍스트를 생성하는 것이 더 유연함
    /*
    public string InteractionPrompt => isAvailable ? $"[{KeyCode.E}] 회복초 사용" : "이미 사용됨";
    */
    // ★★★★★★★★★★★★★★★★★★★★★★


    // --- 내부 로직 ---

    /// <summary> 회복초를 비활성화하고 리스폰 타이머를 시작합니다. </summary>
    private void StartRespawnTimer()
    {
        isAvailable = false; // 사용 불가 상태로 변경
        HideHighlight(); // 하이라이트 끄기

        // 시각적으로 숨김 (메쉬 끄기 등)
        if (meshRenderer != null) meshRenderer.enabled = false;
        // 상호작용 콜라이더 끄기
        if (interactCollider != null) interactCollider.enabled = false;

        // 리스폰 시간이 설정되어 있으면 지정된 시간 후 Respawn 함수 호출
        if (respawnTime > 0)
        {
            Invoke(nameof(Respawn), respawnTime);
            // 또는 코루틴 사용: StartCoroutine(RespawnCoroutine(respawnTime));
        }
        // 리스폰 시간이 0 이하면 영구 비활성화 (또는 파괴 Destroy(gameObject);)
    }

    /// <summary> 회복초를 다시 활성화합니다. </summary>
    private void Respawn()
    {
        isAvailable = true; // 사용 가능 상태로 변경

        // 시각적으로 다시 표시
        if (meshRenderer != null) meshRenderer.enabled = true;
        // 상호작용 콜라이더 다시 켜기
        if (interactCollider != null) interactCollider.enabled = true;

        Debug.Log($"Healing Herb at {transform.position} has respawned.");
    }

    // 기즈모 표시는 그대로 유지
    private void OnDrawGizmosSelected() { Gizmos.color = Color.green; Gizmos.DrawWireSphere(transform.position, healRadius); }
}
