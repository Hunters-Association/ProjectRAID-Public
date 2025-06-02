using UnityEngine;
using DG.Tweening;

public class AimController : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private LayerMask passThroughMask;
    [SerializeField] private float rayDistance = 100f;
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private Ease easing = Ease.OutQuad;

    [SerializeField] private Mesh gizmoSphere;

    private readonly RaycastHit[] hits = new RaycastHit[10];
    private float currentZ = 0f;
    private float targetZ = 0f;
    private Tweener zTween;

    private void Start()
    {
        if (cam == null) cam = Camera.main;

        // DOTween 트윈 생성 (한 번만 실행)
        zTween = DOTween.To(() => currentZ, z =>
        {
            currentZ = z;
            transform.localPosition = new(0, 0, currentZ);
        }, targetZ, 1f / followSpeed)
        .SetEase(easing)
        .SetUpdate(true)
        .SetAutoKill(false)
        .Pause();
    }

    void Update()
    {
        Ray ray = cam.ViewportPointToRay(new(0.5f, 0.5f));
        int hitCount = Physics.RaycastNonAlloc(ray, hits, rayDistance);

        float closest = rayDistance;
        bool foundHit = false;

        for (int i = 0; i < hitCount; i++)
        {
            var hit = hits[i];
            int layer = hit.collider.gameObject.layer;

            if ((passThroughMask.value & (1 << layer)) != 0) continue;
            if ((layerMask.value & (1 << layer)) == 0) continue;

            float distance = Vector3.Distance(cam.transform.position, hit.point);

            if (distance < closest)
            {
                closest = distance;
                foundHit = true;
            }
        }

        targetZ = foundHit ? closest : rayDistance;

        if (Mathf.Abs(currentZ - targetZ) > 0.01f)
        {
            zTween.ChangeEndValue(targetZ, true).Play();
        }
    }

    private void OnDestroy() => zTween?.Kill();

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (gizmoSphere == null)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.8f);
            Gizmos.DrawSphere(transform.position, 0.1f);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.1f);
        }
        else
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.8f);
            Gizmos.DrawMesh(gizmoSphere, transform.position, transform.rotation, Vector3.one * 0.2f);
        }
    }

#endif
}
