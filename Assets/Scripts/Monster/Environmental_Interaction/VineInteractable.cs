using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider))]
public class VineInteractable : MonoBehaviour, IInteractable
{
    [Header("줄기 설정")]
    [Tooltip("플레이어가 따라갈 줄기 경로점들의 목록 (순서대로)")]
    [SerializeField] private List<Transform> vinePathPoints = new List<Transform>();
    [SerializeField] private float travelSpeed = 5.0f;
    [SerializeField] private bool disableAfterUse = true;
    [SerializeField] private float cooldownTime = 10.0f;


    private Collider interactCollider;
    private bool isCoolingDown = false;

    // 아웃라인 효과 등을 위한 컴포넌트 참조 
    [Header("하이라이트 (선택적)")]
    [SerializeField] private Outline outlineEffect;

    // --- 초기화 ---
    private void Awake()
    {
        interactCollider = GetComponent<Collider>();
        // 콜라이더 및 isTrigger 확인 (이전과 동일)
        if (interactCollider == null) Debug.LogError("Collider 없음!", this);
        else if (!interactCollider.isTrigger) Debug.LogWarning("Collider의 Is Trigger가 꺼져있습니다.", this);

        // 아웃라인 컴포넌트 가져오기 (없어도 오류 안 나게)
        outlineEffect = GetComponent<Outline>(); // 이름이 다르면 수정
        if (outlineEffect != null) outlineEffect.enabled = false; // 기본적으로 비활성화
    }


    // ★★★ IInteractable.Interact 구현 ★★★
    /// <summary>
    /// 플레이어가 줄기와 상호작용했을 때 호출됩니다. (IInteractable 구현)
    /// </summary>
    /// <param name="player">상호작용을 시도한 플레이어 컨트롤러</param>
    public void Interact(PlayerController player) // <<< 파라미터 PlayerController
    {
        if (isCoolingDown) { Debug.Log("줄기 쿨다운 중..."); return; }

        // ★ 수정: 경로점 리스트 유효성 검사 ★
        if (vinePathPoints == null || vinePathPoints.Count == 0)
        {
            Debug.LogError("줄기 경로점이 설정되지 않았습니다!", this);
            return;
        }
        // 경로점 중 null이 있는지 추가 확인 (선택적)
        foreach (var point in vinePathPoints)
        {
            if (point == null)
            {
                Debug.LogError("줄기 경로점 목록에 비어있는(null) 항목이 있습니다!", this);
                return;
            }
        }        

        Debug.Log($"플레이어 {player.gameObject.name}이(가) 줄기 타기 시작!");

        if (travelSpeed <= 0) // 즉시 이동 (마지막 지점으로)
        {
            //  마지막 경로점으로 텔레포트 
            TeleportPlayer(player.gameObject, vinePathPoints[vinePathPoints.Count - 1].position);
            if (disableAfterUse) StartCooldown();
        }
        else // 부드러운 이동 (경로 따라)
        {
            if (interactCollider != null) interactCollider.enabled = false;
            HideHighlight();
            // ★ 수정: 코루틴 호출 시 경로 리스트 전달은 필요 없음 (멤버 변수 사용) ★
            StartCoroutine(MovePlayerAlongPathCoroutine(player.transform, travelSpeed));
        }
    }


    // ★★★ IInteractable.ShowHighlight 구현 ★★★
    public void ShowHighlight()
    {
        // 쿨다운 중이 아닐 때만 하이라이트 표시
        if (!isCoolingDown)
        {
            // Debug.Log($"Vine '{gameObject.name}' Highlighted.");
            if (outlineEffect != null) outlineEffect.enabled = true;
            // 또는 다른 시각 효과 켜기
        }
    }    

    // ★★★ IInteractable.HideHighlight 구현 ★★★
    public void HideHighlight()
    {
        // Debug.Log($"Vine '{gameObject.name}' Highlight Removed.");
        if (outlineEffect != null) outlineEffect.enabled = false;
        // 또는 다른 시각 효과 끄기
    }


    [SerializeField] private InteractableData data;

    public InteractableData GetInteractableData()
    {
        if (data == null) Debug.LogWarning("[VineInteractable] 상호작용 데이터가 등록되지 않았습니다.");
        return data;
    }


    // --- 내부 로직 (TeleportPlayer, MovePlayerCoroutine, StartCooldown, EndCooldown) ---

    /// <summary> 플레이어를 지정된 위치로 즉시 이동 </summary>
    private void TeleportPlayer(GameObject player, Vector3 destination)
    {
        // CharacterController 비활성화/활성화 필요
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        player.transform.position = destination; // 위치 이동
        if (cc != null) cc.enabled = true;
        Debug.Log($"Player teleported to {destination}");
    }

    /// <summary> 플레이어를 목표 지점까지 부드럽게 이동시키는 코루틴 </summary>
    private IEnumerator MovePlayerAlongPathCoroutine(Transform playerTransform, float speed)
    {
        // 이동 전 플레이어 컨트롤 비활성화 등 (필요 시)
        // playerTransform.GetComponent<PlayerController>()?.DisableMovement();

        CharacterController cc = playerTransform.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false; // 이동 중 CC 비활성화

        // 경로점 순회
        for (int i = 0; i < vinePathPoints.Count; i++)
        {
            Transform targetPoint = vinePathPoints[i];
            Vector3 destination = targetPoint.position;
            Debug.Log($"Moving to point {i + 1}: {destination}");

            // 현재 경로점까지 이동 루프
            while (Vector3.Distance(playerTransform.position, destination) > 0.1f)
            {
                // 이동
                playerTransform.position = Vector3.MoveTowards(playerTransform.position, destination, speed * Time.deltaTime);

                yield return null; // 다음 프레임까지 대기
            }

            // 정확한 위치 보정
            playerTransform.position = destination;
            Debug.Log($"Reached point {i + 1}");
        }

        // 모든 경로점 이동 완료 후 처리
        if (cc != null) cc.enabled = true; // CC 다시 활성화

        // 이동 완료 후 플레이어 컨트롤 활성화 (필요 시)
        // playerTransform.GetComponent<PlayerController>()?.EnableMovement();
        Debug.Log($"Player finished moving along the vine path.");

        // 줄기 쿨다운/비활성화 처리
        if (disableAfterUse)
        {
            StartCooldown();
        }
        else // 재사용 가능하면 콜라이더 다시 켜기
        {
            if (interactCollider != null) interactCollider.enabled = true;
        }
    }

    /// <summary> 쿨다운 시작 </summary>
    private void StartCooldown()
    {
        if (cooldownTime > 0)
        {
            isCoolingDown = true;
            if (interactCollider != null) interactCollider.enabled = false;
            Debug.Log($"Vine '{gameObject.name}' starting cooldown for {cooldownTime} seconds.");
            Invoke(nameof(EndCooldown), cooldownTime);
        }
        else // 영구 비활성화
        {
            Debug.Log($"Vine '{gameObject.name}' permanently disabled.");
            gameObject.SetActive(false); // 또는 콜라이더만 비활성화 유지
        }
    }

    /// <summary> 쿨다운 종료 </summary>
    private void EndCooldown()
    {
        isCoolingDown = false;
        if (interactCollider != null) interactCollider.enabled = true; // 다시 상호작용 가능
        Debug.Log($"Vine '{gameObject.name}' cooldown finished.");


    }
}
