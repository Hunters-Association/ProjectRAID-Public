using UnityEngine.Animations.Rigging;
using DG.Tweening;

namespace ProjectRaid.Extensions
{
    public static class ConstraintExtensions
    {
        /// <summary>
        /// Rig 전체의 weight를 트윈으로 부드럽게 조정
        /// </summary>
        public static Tween DOWeight(this Rig rig, float target, float duration)
        {
            return DOTween.To(() => rig.weight, x => rig.weight = x, target, duration);
        }

        /// <summary>
        /// MultiParentConstraint의 weight를 트윈으로 부드럽게 조정
        /// </summary>
        public static Tween DOWeight(this MultiParentConstraint constraint, float target, float duration)
        {
            return DOTween.To(() => constraint.weight, x => constraint.weight = x, target, duration);
        }

        /// <summary>
        /// MultiAimConstraint의 weight를 트윈으로 부드럽게 조정
        /// </summary>
        public static Tween DOWeight(this MultiAimConstraint constraint, float target, float duration)
        {
            return DOTween.To(() => constraint.weight, x => constraint.weight = x, target, duration);
        }

        /// <summary>
        /// MultiPositionConstraint의 weight를 트윈으로 부드럽게 조정
        /// </summary>
        public static Tween DOWeight(this MultiPositionConstraint constraint, float target, float duration)
        {
            return DOTween.To(() => constraint.weight, x => constraint.weight = x, target, duration);
        }

        /// <summary>
        /// TwoBoneIKConstraint의 weight를 트윈으로 부드럽게 조정
        /// </summary>
        public static Tween DOWeight(this TwoBoneIKConstraint constraint, float target, float duration)
        {
            return DOTween.To(() => constraint.weight, x => constraint.weight = x, target, duration);
        }

        /// <summary>
        /// MultiParentConstraint의 소스 가중치를 각각 부드럽게 조정
        /// </summary>
        public static Tween DOSourceWeights(this MultiParentConstraint constraint, float duration, params float[] targetWeights)
        {
            if (constraint == null || constraint.data.sourceObjects.Count == 0)
            {
                UnityEngine.Debug.LogWarning("[DOSourceWeights] constraint 또는 sourceObjects가 비어 있습니다.");
                return null;
            }
            
            var sources = constraint.data.sourceObjects;
            int count = sources.Count;

            if (targetWeights.Length != count)
            {
                UnityEngine.Debug.LogWarning($"[DOSourceWeights] targetWeights의 개수({targetWeights.Length})가 소스 개수({count})와 일치하지 않습니다.");
                return null;
            }

            // 현재 가중치 복사
            float[] currentWeights = new float[count];
            for (int i = 0; i < count; i++)
                currentWeights[i] = sources.GetWeight(i);

            // Sequence로 트윈 구성
            Sequence sequence = DOTween.Sequence();
            for (int i = 0; i < count; i++)
            {
                int index = i; // 클로저 방지용

                sequence.Join(DOTween.To(() => currentWeights[index], x =>
                {
                    currentWeights[index] = x;
                    ApplyWeights(constraint, currentWeights);
                }, targetWeights[index], duration));
            }

            return sequence;
        }

        private static void ApplyWeights(MultiParentConstraint constraint, float[] weights)
        {
            var sources = constraint.data.sourceObjects;
            for (int i = 0; i < sources.Count; i++)
            {
                sources.SetWeight(i, weights[i]);
            }
            constraint.data.sourceObjects = sources;
        }
    }
}
