using UnityEngine;
using Cinemachine;using DG.Tweening;

/// <summary>
/// 카메라 셰이크를 관리하는 매니저
/// </summary>
public class CameraShakeManager : MonoBehaviour
{
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float shakeAmplitude = 2f;
    [SerializeField] private float shakeFrequency = 2f;

    private CinemachineVirtualCameraBase activeCam;
    private CinemachineBasicMultiChannelPerlin activePerlin;

    private void ShakeFreeLook(CinemachineFreeLook freeLook)
    {
        foreach (var rig in new[] { freeLook.GetRig(0), freeLook.GetRig(1), freeLook.GetRig(2) })
        {
            if (rig.TryGetComponent(out CinemachineVirtualCamera vcam))
            {
                var perlin = vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
                if (perlin == null) continue;

                DOTween.To(() => perlin.m_AmplitudeGain, x => perlin.m_AmplitudeGain = x, 0f, shakeDuration)
                    .SetEase(Ease.OutSine)
                    .From(shakeAmplitude);
                DOTween.To(() => perlin.m_FrequencyGain, x => perlin.m_FrequencyGain = x, 0f, shakeDuration)
                    .SetEase(Ease.OutSine)
                    .From(shakeFrequency);
            }
        }
    }

    /// <summary>
    /// 현재 활성화된 카메라 기준으로 셰이크 시작
    /// </summary>
    public void Shake()
    {
        // var brain = Camera.main.GetComponent<CinemachineBrain>();
        // if (brain == null || brain.ActiveVirtualCamera == null) return;

        // activeCam = brain.ActiveVirtualCamera as CinemachineVirtualCameraBase;

        // switch (activeCam)
        // {
        //     case CinemachineFreeLook freeLook:
        //         ShakeFreeLook(freeLook);
        //         break;

        //     case CinemachineVirtualCamera virtualCam:
        //         activePerlin = virtualCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        //         if (activePerlin != null)
        //         {
        //             activePerlin.m_AmplitudeGain = shakeAmplitude;
        //             activePerlin.m_FrequencyGain = shakeFrequency;
        //         }
        //         break;
        // }

        // DOVirtual.DelayedCall(shakeDuration, StopShake).SetUpdate(true);
    }

    private void StopShake()
    {
        switch (activeCam)
        {
            case CinemachineFreeLook freeLook:
                foreach (var rig in new[] { freeLook.GetRig(0), freeLook.GetRig(1), freeLook.GetRig(2) })
                {
                    if (rig.TryGetComponent(out CinemachineVirtualCamera vcam))
                    {
                        var perlin = vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
                        if (perlin != null)
                        {
                            perlin.m_AmplitudeGain = 0;
                            perlin.m_FrequencyGain = 0;
                        }
                    }
                }
                break;

            case CinemachineVirtualCamera:
                if (activePerlin != null)
                {
                    activePerlin.m_AmplitudeGain = 0;
                    activePerlin.m_FrequencyGain = 0;
                }
                break;
        }
    }
}
