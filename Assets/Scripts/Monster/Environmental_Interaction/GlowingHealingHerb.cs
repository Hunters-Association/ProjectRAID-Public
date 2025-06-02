using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider))]
public class GlowingHealingHerb : MonoBehaviour, IInteractable 
{
    public event Action OnUse;

    [Header("빛나는 회복초 설정")]
    [SerializeField] private float healRadius = 4.0f; // 회복 반경    
    [SerializeField] private float respawnTime = 60.0f; // 리스폰 시간 (1분)
    [SerializeField] private GameObject gatherEffectPrefab; // 채집 시 효과
    [SerializeField] private AudioClip gatherSound;       // 채집 시 사운드

    private Collider interactCollider;
    private MeshRenderer meshRenderer;
    private bool isAvailable = true; // 채집 가능 여부

    [Header("하이라이트 (선택적)")]
    [SerializeField] private Outline outlineEffect;

    // --- 초기화 ---
    private void Awake()
    {
        interactCollider = GetComponent<Collider>();
        if (interactCollider != null) interactCollider.isTrigger = true;
        else Debug.LogError("GlowingHealingHerb에 Collider가 없습니다!", this);

        meshRenderer = GetComponentInChildren<MeshRenderer>();
        outlineEffect = GetComponent<Outline>();
        if (outlineEffect != null) outlineEffect.enabled = false;
    }


    // --- IInteractable 구현 ---

    // ★★★ IInteractable.Interact 구현 ★★★
    /// <summary>
    /// 플레이어가 빛나는 회복초와 상호작용 시 호출됩니다.
    /// 상호작용한 플레이어의 체력을 전체 회복시킵니다.
    /// </summary>
    /// <param name="player">상호작용한 플레이어 컨트롤러</param>
    public void Interact(PlayerController player) // <<< 파라미터 PlayerController
    {
        OnUse?.Invoke();

        if (!isAvailable) { Debug.Log("이 빛나는 회복초는 이미 사용되었거나 아직 재생성되지 않았습니다."); return; }

        Debug.Log($"[{Time.timeSinceLevelLoad:F1}s] 플레이어 {player.gameObject.name}이(가) 빛나는 회복초 사용! 전체 HP 회복.");
        player.Stats.Heal(player.Stats.Runtime.MaxHealth);

        // ★★★ 상호작용한 플레이어의 체력 전체 회복 로직 호출 ★★★
        //PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        //if (playerHealth != null)
        //{
        //    // playerHealth.HealToFull(); // PlayerHealth에 HealToFull 메서드 구현 필요
        //    // 또는 Heal 메서드를 최대 체력 값으로 호출
        //    playerHealth.Heal(playerHealth.maxHealth);
        //    Debug.Log($" - {player.gameObject.name} 전체 HP 회복 실행 (PlayerHealth.Heal 호출)");
        //}
        //else { Debug.LogWarning($"Player '{player.gameObject.name}' is missing PlayerHealth component. Cannot heal."); }
        

        // 효과 재생
        if (gatherEffectPrefab != null) Instantiate(gatherEffectPrefab, transform.position, Quaternion.identity);
        // if (gatherSound != null) AudioSource.PlayClipAtPoint(gatherSound, transform.position);

        // 비활성화 및 리스폰 처리
        StartRespawnTimer();
    }

    public void ShowHighlight()
    {
        if (isAvailable && outlineEffect != null)
            outlineEffect.enabled = true;
    }


    public void HideHighlight()
    {
        if (outlineEffect != null)
            outlineEffect.enabled = false;
    }


    [SerializeField] private InteractableData data;

    public InteractableData GetInteractableData()
    {
        if (data == null) Debug.LogWarning("[GlowingHealingHerb] 상호작용 데이터가 등록되지 않았습니다.");
        return data;
    }


    // --- 내부 로직 (리스폰) ---
    /// <summary> 회복초를 비활성화하고 리스폰 타이머를 시작합니다. </summary>
    private void StartRespawnTimer()
    {
        isAvailable = false;
        HideHighlight();
        if (meshRenderer != null) meshRenderer.enabled = false;
        if (interactCollider != null) interactCollider.enabled = false;
        if (respawnTime > 0) Invoke(nameof(Respawn), respawnTime);
        else gameObject.SetActive(false); // 영구 비활성화
    }

    /// <summary> 회복초를 다시 활성화합니다. </summary>
    private void Respawn()
    {
        isAvailable = true;
        if (meshRenderer != null) meshRenderer.enabled = true;
        if (interactCollider != null) interactCollider.enabled = true;
        Debug.Log($"Glowing Healing Herb at {transform.position} has respawned.");
    }

    // 기즈모 표시는 그대로 유지
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, healRadius);
    }
}   // 기즈모 색상 변경

