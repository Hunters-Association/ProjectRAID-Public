using UnityEngine;

/// <summary>
/// 카메라를 바라보도록 항상 회전하는 Billboard 컴포넌트
/// </summary>
public class Billboard : MonoBehaviour
{
    private Transform cam;

    private void Start()
    {
        SetCamera();
    }

    private void LateUpdate()
    {
        if (cam == null) return;
        transform.forward = cam.forward;
    }

    public void SetCamera()
    {
        cam = Camera.main.transform;
    }
}