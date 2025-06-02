using UnityEngine;
using DG.Tweening;

namespace ProjectRaid.Extensions
{
    public static class AnimatorExtensions
    {
        public static Tween DOWeight(this Animator animator, int layerIndex, float targetWeight, float duration)
        {
            // 현재 weight를 가져오기 위한 변수
            float currentWeight = animator.GetLayerWeight(layerIndex);

            // DOTween 트윈 생성
            return DOTween.To(() => currentWeight,
                x => {
                    currentWeight = x;
                    animator.SetLayerWeight(layerIndex, currentWeight);
                },
                targetWeight,
                duration
            );
        }
    }
}
