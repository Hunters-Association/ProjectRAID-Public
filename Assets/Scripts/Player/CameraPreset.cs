using UnityEngine;

[CreateAssetMenu(fileName = "CameraPreset", menuName = "Camera/Camera Preset")]
public class CameraPreset : ScriptableObject
{
    public Vector3 shoulderOffset = new(0.25f, 0.1f, 0f);
    public float cameraSide = 1f;
    public float cameraDistance = 6f;
    public float transitionDuration = 0.5f;
}
