using UnityEngine;

/// <summary>
/// Animator 파라미터 해시값을 관리하는 클래스
/// </summary>
public static class PlayerAnimatorParams
{
    // 루트 모션
    public static readonly int UseRootMotion    = Animator.StringToHash("UseRootMotion");
    public static readonly int LockTurn         = Animator.StringToHash("LockTurn");

    // 이동
    public static readonly int MotionSpeed      = Animator.StringToHash("MotionSpeed");
    public static readonly int Speed            = Animator.StringToHash("Speed");
    public static readonly int RunTrigger       = Animator.StringToHash("RunTrigger");
    // public static readonly int Run              = Animator.StringToHash("Run");

    // 전투
    public static readonly int InIdle           = Animator.StringToHash("InIdle");
    public static readonly int InCombat         = Animator.StringToHash("InCombat");
    public static readonly int InAttack         = Animator.StringToHash("InAttack");
    public static readonly int AttackTrigger    = Animator.StringToHash("AttackTrigger");
    public static readonly int AttackQueued     = Animator.StringToHash("AttackQueued");
    public static readonly int AttackIndex      = Animator.StringToHash("AttackIndex");
    public static readonly int Charge           = Animator.StringToHash("Charge");
    public static readonly int Aim              = Animator.StringToHash("Aim");
    public static readonly int Equip            = Animator.StringToHash("Equip");
    public static readonly int Unequip          = Animator.StringToHash("Unequip");
    // public static readonly int CanCombo         = Animator.StringToHash("CanCombo");
    // public static readonly int WeaponID         = Animator.StringToHash("WeaponID");

    // 회피 & 피격
    public static readonly int Cancel           = Animator.StringToHash("Cancel");
    public static readonly int DodgeTrigger     = Animator.StringToHash("DodgeTrigger");
    public static readonly int DodgeQueued      = Animator.StringToHash("DodgeQueued");
    public static readonly int Hit              = Animator.StringToHash("Hit");
    public static readonly int HitDirection     = Animator.StringToHash("HitDirection");
    public static readonly int SuperStance      = Animator.StringToHash("SuperStance");
    public static readonly int Dead             = Animator.StringToHash("Dead");

    // 상호작용
    public static readonly int Interact         = Animator.StringToHash("Interact");
}