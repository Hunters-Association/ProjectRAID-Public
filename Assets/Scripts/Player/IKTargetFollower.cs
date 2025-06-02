using UnityEngine;
using ProjectRaid.EditorTools;

public class IKTargetFollower : MonoBehaviour
{
    public Transform target;
    // [SerializeField] private float transitionDuration = 0.25f;
    [SerializeField] private float positionFollowSpeed = 10f;
    [SerializeField] private float rotationFollowSpeed = 10f;

    // private float transitionTimeElapsed = 0f;
    private bool isTransitioning = false;

    public void SetFollowTarget(Transform newTarget)
    {
        if (newTarget == null) return;

        target = newTarget;
        // transitionTimeElapsed = 0f;
        isTransitioning = true;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        if (isTransitioning)
        {
            // transitionTimeElapsed += Time.deltaTime;
            // float t = Mathf.Clamp01(transitionTimeElapsed / transitionDuration);

            // // 부드럽게 보간
            // transform.SetPositionAndRotation(
            //     position: Vector3.Lerp(transform.position, target.position, t),
            //     rotation: Quaternion.Slerp(transform.rotation, target.rotation, t)
            // );

            // // 도착 후 고정 모드 전환
            // if (t >= 1f)
            // {
            //     isTransitioning = false;
            //     transform.SetPositionAndRotation(target.position, target.rotation);
            // }

            transform.position = Vector3.Lerp(transform.position, target.position, Time.deltaTime * positionFollowSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, Time.deltaTime * rotationFollowSpeed);
        }
        else
        {
            // 완전히 고정 상태
            transform.SetPositionAndRotation(target.position, target.rotation);
        }
    }
}
