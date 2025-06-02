using UnityEngine;
using TMPro;
using ProjectRaid.EditorTools;

[DisallowMultipleComponent]
public class PlayerInteractionController : MonoBehaviour
{
    [FoldoutGroup("상호작용", ExtendedColor.Plum)]
    [SerializeField] private SphereCollider interactionTrigger; // 상호작용 트리거
    [SerializeField] private LayerMask interactableLayer;       // 상호작용 가능한 레이어
    [SerializeField] private CanvasGroup interactionUI;         // 상호작용 UI (프롬프트 포함)
    [SerializeField] private TextMeshProUGUI interactionPrompt; // 상호작용 프롬프트 텍스트

    [FoldoutGroup("디버그/기즈모", ExtendedColor.DodgerBlue)]
    [SerializeField] private bool showGizmo = false;
    [SerializeField] private Mesh gizmoMesh;
    [SerializeField] private Color gizmoColor;

    [FoldoutGroup("디버그/로그", ExtendedColor.DodgerBlue)]
    [SerializeField] private bool showLog = false;


    #region INIT
    // ────────────────────────────────────────────────────────────────
    public void Init(PlayerController player)
    {
        if (showLog) Debug.Log("초기화 완료!");
    }
    #endregion


#if UNITY_EDITOR
    #region EDITOR
    // ────────────────────────────────────────────────────────────────
    // Gizmo로 지면 판정 영역 확인
    private void OnDrawGizmosSelected()
    {
        if (!showGizmo) return;

        // Gizmos.color = gizmoColor;
        // var playerCenter = transform.position + characterController.center;

        // if (gizmoMesh != null)
        //     Gizmos.DrawMesh(gizmoMesh, groundCheck.position, transform.localRotation, Vector3.one * groundRadius);
        // else
        //     Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
    }
    #endregion
#endif
}
