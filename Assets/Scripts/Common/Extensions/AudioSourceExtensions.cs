using UnityEngine;

namespace ProjectRaid.Extensions
{
    public static class AudioSourceExtensions
    {
        public static void PlayOneShot(this AudioSource source, AudioClip clip, float volume, float pitch)
        {
            float originalPitch = source.pitch;
            source.pitch = pitch;
            source.PlayOneShot(clip, volume);
            source.pitch = originalPitch; // 재생 직후 원래대로 되돌리기
        }
    }
}
