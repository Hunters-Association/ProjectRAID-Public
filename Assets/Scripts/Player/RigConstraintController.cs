using UnityEngine;
using UnityEngine.Animations.Rigging;
using ProjectRaid.EditorTools;
using ProjectRaid.Extensions;
using DG.Tweening;

// public enum RigType { Body, Weapon, Hand }
// public enum WeaponConstraintType { WeaponPosition, WeaponAim, WeaponParent }

/// <summary>
/// 플레이어 상태에 따라 리깅 제약 가중치를 조절합니다.
/// </summary>
public class RigConstraintController : MonoBehaviour
{
    [FoldoutGroup("Rig 레이어", ExtendedColor.SteelBlue)]
    [SerializeField] private Rig rigBody;
    [SerializeField] private Rig rigWeapon;
    // [SerializeField] private Rig rigHand;

    [FoldoutGroup("무기 제약 컴포넌트", ExtendedColor.SteelBlue)]
    // [SerializeField] private MultiPositionConstraint weaponPosition;
    // [SerializeField] private MultiAimConstraint weaponAim;
    [SerializeField] private MultiParentConstraint weaponParent;

    // [FoldoutGroup("손 제약 컴포넌트", ExtendedColor.SteelBlue)]
    // [SerializeField] private TwoBoneIKConstraint leftHandIK;
    // [SerializeField] private TwoBoneIKConstraint rightHandIK;

    [FoldoutGroup("Tween 설정", ExtendedColor.SeaGreen)]
    [SerializeField] private float transitionDuration = 0.25f;

    private Tween rigTween;

    /// <summary>
    /// 전투 상태 - 기본 무기 제약만 유지
    /// </summary>
    public void SetToDefaultCombat()
    {
        KillCurrentTween();

        Sequence sequence = DOTween.Sequence();
        sequence.Join(rigBody.DOWeight(0f, transitionDuration));
        sequence.Join(rigWeapon.DOWeight(1f, transitionDuration));
        // sequence.Join(rigHand.DOWeight(0f, transitionDuration));

        // sequence.Join(weaponPosition.DOWeight(0f, transitionDuration));
        // sequence.Join(weaponAim.DOWeight(0f, transitionDuration));
        sequence.Join(weaponParent.DOWeight(1f, transitionDuration));
        sequence.Join(weaponParent.DOSourceWeights(transitionDuration, 1f, 0f, 0f));

        // sequence.Join(leftHandIK.DOWeight(0f, transitionDuration));
        // sequence.Join(rightHandIK.DOWeight(0f, transitionDuration));

        rigTween = sequence;
    }

    /// <summary>
    /// 전투 상태로 진입 - 모든 제약 활성화
    /// </summary>
    public void SetToCombat()
    {
        KillCurrentTween();

        Sequence sequence = DOTween.Sequence();
        sequence.Join(rigBody.DOWeight(0f, transitionDuration));
        sequence.Join(rigWeapon.DOWeight(1f, transitionDuration));
        // sequence.Join(rigHand.DOWeight(0f, transitionDuration));

        // sequence.Join(weaponPosition.DOWeight(0f, transitionDuration));
        // sequence.Join(weaponAim.DOWeight(0f, transitionDuration));
        sequence.Join(weaponParent.DOWeight(1f, transitionDuration));
        sequence.Join(weaponParent.DOSourceWeights(transitionDuration, 0f, 1f, 0f));

        // sequence.Join(leftHandIK.DOWeight(1f, transitionDuration));
        // sequence.Join(rightHandIK.DOWeight(1f, transitionDuration));

        rigTween = sequence;
    }

    /// <summary>
    /// 조준 상태로 진입 - 모든 제약 활성화
    /// </summary>
    public void SetToAiming()
    {
        KillCurrentTween();

        Sequence sequence = DOTween.Sequence();
        sequence.Join(rigBody.DOWeight(1f, transitionDuration));
        sequence.Join(rigWeapon.DOWeight(1f, transitionDuration));
        // sequence.Join(rigHand.DOWeight(1f, transitionDuration));

        // sequence.Join(weaponPosition.DOWeight(1f, transitionDuration));
        // sequence.Join(weaponAim.DOWeight(1f, transitionDuration));
        sequence.Join(weaponParent.DOWeight(1f, transitionDuration));
        sequence.Join(weaponParent.DOSourceWeights(transitionDuration, 0f, 1f, 0f));

        // sequence.Join(leftHandIK.DOWeight(1f, transitionDuration));
        // sequence.Join(rightHandIK.DOWeight(1f, transitionDuration));

        rigTween = sequence;
    }

    /// <summary>
    /// 공격 상태 진입 - 기본값은 조준 상태와 거의 동일하나, 세부 제약은 애니메이션 이벤트로 제어
    /// </summary>
    public void SetToAttack()
    {
        KillCurrentTween();

        Sequence sequence = DOTween.Sequence();
        sequence.Join(rigBody.DOWeight(1f, transitionDuration));
        sequence.Join(rigWeapon.DOWeight(1f, transitionDuration));
        // sequence.Join(rigHand.DOWeight(1f, transitionDuration));

        // sequence.Join(weaponPosition.DOWeight(1f, transitionDuration));
        // sequence.Join(weaponAim.DOWeight(1f, transitionDuration));
        sequence.Join(weaponParent.DOWeight(1f, transitionDuration));
        sequence.Join(weaponParent.DOSourceWeights(transitionDuration, 0f, 1f, 0f));
        // sequence.Join(weaponParent.DOSourceWeights(transitionDuration, 0f, 0f, 0f, 1f));

        // sequence.Join(leftHandIK.DOWeight(1f, transitionDuration));
        // sequence.Join(rightHandIK.DOWeight(1f, transitionDuration));

        rigTween = sequence;
    }

    /// <summary>
    /// Rig 전체의 weight를 설정 (애니메이션 이벤트에서 사용)
    /// </summary>
    // public void SetRigWeight(RigType type, float weight)
    // {
    //     switch (type)
    //     {
    //         case RigType.Body:
    //             rigBody.DOWeight(weight, transitionDuration);
    //             break;
    //         case RigType.Weapon:
    //             rigWeapon.DOWeight(weight, transitionDuration);
    //             break;
    //         case RigType.Hand:
    //             rigHand.DOWeight(weight, transitionDuration);
    //             break;
    //     }
    // }

    /// <summary>
    /// 타입으로 개별 무기 제약의 weight를 설정 (애니메이션 이벤트에서 사용)
    /// </summary>
    // public void SetConstraintWeight(WeaponConstraintType type, float weight)
    // {
    //     switch (type)
    //     {
    //         case WeaponConstraintType.WeaponPosition:
    //             weaponPosition.DOWeight(weight, transitionDuration);
    //             break;
    //         case WeaponConstraintType.WeaponAim:
    //             weaponAim.DOWeight(weight, transitionDuration);
    //             break;
    //         case WeaponConstraintType.WeaponParent:
    //             weaponParent.DOWeight(weight, transitionDuration);
    //             break;
    //     }
    // }

    /// <summary>
    /// 타입으로 개별 손 제약의 weight를 설정 (애니메이션 이벤트에서 사용)
    /// </summary>
    // public void SetConstraintWeight(WeaponManager.HandType type, float weight)
    // {
    //     switch (type)
    //     {
    //         case WeaponManager.HandType.Left:
    //             leftHandIK.DOWeight(weight, transitionDuration);
    //             break;
    //         case WeaponManager.HandType.Right:
    //             rightHandIK.DOWeight(weight, transitionDuration);
    //             break;
    //     }
    // }

    private void KillCurrentTween()
    {
        rigTween?.Kill();
        rigTween = null;
    }
}
