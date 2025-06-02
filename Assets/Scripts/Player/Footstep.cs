using UnityEngine;
using ProjectRaid.EditorTools;

/// <summary>
/// 발소리 재생을 위한 클래스
/// </summary>
public class Footstep : MonoBehaviour
{
    [FoldoutGroup("오디오", ExtendedColor.Gold)]
    [SerializeField] [Range(0, 1)] private float audioVolume = 0.5f;
    [SerializeField] private CharacterController controller;
    [SerializeField] private AudioClip[] footstepClips;

    /// <summary>
    /// 애니메이션 이벤트: 발걸음에 맞게 사운드 재생
    /// </summary>
    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            if (footstepClips != null && footstepClips.Length > 0)
            {
                var index = Random.Range(0, footstepClips.Length);

                if (controller != null)
                {
                    AudioSource.PlayClipAtPoint(footstepClips[index], transform.TransformPoint(controller.center), audioVolume);
                }
                else
                {
                    AudioSource.PlayClipAtPoint(footstepClips[index], transform.position, audioVolume);
                }
            }
        }
    }
}