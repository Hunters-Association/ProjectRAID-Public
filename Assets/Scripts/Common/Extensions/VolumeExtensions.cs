using UnityEngine.Rendering;
using DG.Tweening;

namespace ProjectRaid.Extensions
{
    public static class VolumeExtensions
    {
        public static Tween DOWeight(this Volume volume, float target, float duration)
        {
            return DOTween.To(() => volume.weight, x => volume.weight = x, target, duration);
        }
    }
}
