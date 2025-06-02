using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using ProjectRaid.Core;
using DG.Tweening;
using HitboxType = PlayerCombatController.HitboxType;

[System.Serializable]
public class HitboxPreset
{
    public Vector2 timing = new(0.25f, 0.75f);
    public float motionValue = 1f;
    public bool isSuperStance = false;
    public List<HitboxType> hitboxType = new();
}

[System.Serializable]
public class SfxPreset
{
    public AudioClip clip;
    [Range(0f, 1f)] public float Volume = 0.5f;
    [Range(0f, 1f)] public float timing = 0.5f;
}

[System.Serializable]
public class VfxPreset
{
    public PlayerAttackVfxType type;
    [Range(0f, 1f)] public float timing = 0.5f;
}

/// <summary>
/// 각 AttackN 상태에 붙여서
/// 1) 콤보 윈도우 On/Off
/// 2) 입력 버퍼 체크 → AttackQueued bool 세팅
/// 3) 히트박스 활성/비활성 콜백
/// 4) 선택적 RootMotion 스케일링
/// 을 관리한다.
/// </summary>
public class AttackStateBehaviour : StateMachineBehaviour
{
    private enum HitboxState { Off, On, Done }
    // ──────────────────────────────────────
    #region INSPECTOR
    [Space(15), Header("1) 루트모션"), Space(5)]
    public bool useRootMotion = true;
    [Range(0f, 1f)] public float rootMotionScale = 1f;
    public List<Vector2> lockTurnTimings = new();
    public AnimationCurve motionSpeedCurve = AnimationCurve.Linear(0, 1, 1, 1);
    public float baseSpeed = 1f;

    [Space(15), Header("2) 콤보"), Space(5)]
    public Vector2 comboInputTiming = new(0.25f, 0.75f);
    public int comboIndex = -1;
    public bool isLastCombo = false;

    [Space(15), Header("3) 히트박스"), Space(5)]
    public List<HitboxPreset> hitboxPresets = new();

    [Space(15), Header("4) FX"), Space(5)]
    public List<SfxPreset> sfxPresets;
    public List<VfxPreset> vfxPresets;
    #endregion
    // ──────────────────────────────────────

    private PlayerController player;
    private List<bool> sfxPlayed;
    private List<bool> vfxPlayed;

    private HitboxState[] presetStates;
    private bool queuedNextAttack;
    private bool runQueued;
    private bool dodgeQueued;
    private bool dodgePerformed;

    // private float zoom;
    // private bool zoomPerformed;

    // 1) OnStateEnter : 초기화
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        CacheRefs(animator);

        useRootMotion &= rootMotionScale > 0f;
        animator.SetBool(PlayerAnimatorParams.UseRootMotion, useRootMotion);
        animator.SetBool(PlayerAnimatorParams.LockTurn, false);
        animator.SetBool(PlayerAnimatorParams.AttackQueued, false);
        animator.ResetTrigger(PlayerAnimatorParams.AttackTrigger);
        animator.ResetTrigger(PlayerAnimatorParams.RunTrigger);


        vfxPlayed = vfxPresets != null ? new List<bool>(new bool[vfxPresets.Count]) : null;
        sfxPlayed = sfxPresets != null ? new List<bool>(new bool[sfxPresets.Count]) : null;

        queuedNextAttack = false;
        runQueued = false;
        dodgeQueued = false;
        dodgePerformed = false;

        if (hitboxPresets != null && hitboxPresets.Count > 0)
        {
            hitboxPresets.Sort((a, b) => a.timing.x.CompareTo(b.timing.x));
            presetStates = new HitboxState[hitboxPresets.Count];
        }
    }

    // 2) OnStateUpdate : 콤보 입력 감지
    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 0~1 범위로 고정 (클립 loop=off 가정)
        float t = stateInfo.normalizedTime % 1f;
        float multiplier = motionSpeedCurve.Evaluate(t);
        animator.speed = baseSpeed * multiplier;

        bool canCombo = t >= comboInputTiming.x && t <= comboInputTiming.y;
        // animator.SetBool(PlayerAnimatorParams.CanCombo, canCombo);

        // 입력 버퍼링 : Combat 스크립트에서 입력 순간 bool 깜빡임
        bool charge = player.CombatController.ConsumeChargeBuffered();
        bool attack = charge || player.CombatController.ConsumeAttackBuffered();

        if (canCombo && !queuedNextAttack && attack && !animator.GetBool(PlayerAnimatorParams.DodgeQueued))
        {
            if (charge)
            {
                queuedNextAttack = true;

                animator.SetBool(PlayerAnimatorParams.Charge, charge);
                animator.SetTrigger(PlayerAnimatorParams.AttackTrigger);
            }
            else if (canCombo)
            {
                queuedNextAttack = true;

                animator.SetBool(PlayerAnimatorParams.Charge, false);
                animator.SetTrigger(PlayerAnimatorParams.AttackTrigger);
            }

            // animator.SetBool(PlayerAnimatorParams.AttackQueued, true);
        }

        if (t >= comboInputTiming.x)
        {
            DOTween.Kill(player.WeaponManager.ComboUI);
            player.WeaponManager.ComboUI.DOFade(1f, 0.2f);
            player.WeaponManager.ComboCount.text = comboIndex.ToString();

            player.ComboIndex = comboIndex;
        }

        if (t >= comboInputTiming.y)
        {
            DOTween.Kill(player.WeaponManager.ComboUI);
            player.WeaponManager.ComboUI.DOFade(0f, 0.2f);
            player.WeaponManager.ComboCount.text = "";

            if (!queuedNextAttack && (dodgeQueued || runQueued) && !dodgePerformed)
                animator.SetBool(PlayerAnimatorParams.UseRootMotion, false);

            if (runQueued) animator.SetTrigger(PlayerAnimatorParams.RunTrigger);
        }

        if (player.InputHandler.IsRunPressed)
            runQueued = true;

        // 히트박스
        if (hitboxPresets != null)
        {
            for (int i = 0; i < hitboxPresets.Count; i++)
            {
                if (presetStates[i] == HitboxState.Done) continue;

                var preset = hitboxPresets[i];
                Vector2 hit = preset.timing;

                if (preset.isSuperStance && presetStates[i] == HitboxState.Off)
                {
                    if (i == 0)
                    {
                        if (t >= Mathf.Max(hit.x - 0.2f, 0.15f))
                            animator.SetBool(PlayerAnimatorParams.SuperStance, true);
                    }
                    else
                    {
                        if (t >= Mathf.Max(hit.x - 0.2f, hitboxPresets[i - 1].timing.y + 0.05f))
                            animator.SetBool(PlayerAnimatorParams.SuperStance, true);
                    }
                }

                // ON
                if (presetStates[i] == HitboxState.Off && t >= hit.x)
                {
                    foreach (var type in preset.hitboxType)
                        player.CombatController.HitboxEnable(type);

                    player.WeaponManager.SetMotionValue(hitboxPresets[i].motionValue);
                    player.WeaponManager.CurrentWeapon.ResetHitRegistry();
                    presetStates[i] = HitboxState.On;
                    animator.SetBool(PlayerAnimatorParams.LockTurn, true);
                }

                // OFF
                if (presetStates[i] == HitboxState.On && t >= hit.y)
                {
                    foreach (var type in preset.hitboxType)
                        player.CombatController.HitboxDisable(type);

                    presetStates[i] = HitboxState.Done;
                    animator.SetBool(PlayerAnimatorParams.LockTurn, false);
                    animator.SetBool(PlayerAnimatorParams.SuperStance, false);
                }
            }
        }

        // 회전 잠금
        if (lockTurnTimings != null && lockTurnTimings.Count > 0)
        {
            foreach (var timing in lockTurnTimings)
            {
                if (!animator.GetBool(PlayerAnimatorParams.LockTurn) && t >= timing.x)
                    animator.SetBool(PlayerAnimatorParams.LockTurn, true);

                if (animator.GetBool(PlayerAnimatorParams.LockTurn) && t >= timing.y)
                    animator.SetBool(PlayerAnimatorParams.LockTurn, false);
            }
        }

        // 공격 후딜 캔슬 (회피)
        if (player.InputHandler.IsDodgePressed)
        {
            // animator.SetTrigger(PlayerAnimatorParams.Dodge);
            animator.SetBool(PlayerAnimatorParams.DodgeQueued, true);
        }

        // SFX 재생
        if (sfxPresets != null)
        {
            for (int i = 0; i < sfxPresets.Count; i++)
            {
                if (sfxPlayed[i]) continue;
                var preset = sfxPresets[i];
                if (t < preset.timing || preset.clip == null || player.AudioSource == null) continue;

                player.AudioSource.PlayOneShot(preset.clip, preset.Volume);
                sfxPlayed[i] = true;
            }
        }

        // VFX 재생
        if (vfxPresets != null && vfxPresets.Count > 0)
        {
            for (int i = 0; i < vfxPresets.Count; i++)
            {
                if (vfxPlayed[i]) continue;
                var preset = vfxPresets[i];
                if (t < preset.timing) continue;

                var particle = player.VfxController.PlayerAttackVfxs[preset.type];
                if (particle == null) continue;

                particle.Play();
                vfxPlayed[i] = true;
            }
        }
    }

    // 3) OnStateExit : 뒷정리
    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // 히트박스 OFF
        if (hitboxPresets != null)
        {
            for (int i = 0; i < hitboxPresets.Count; i++)
                if (presetStates[i] == HitboxState.On)
                    foreach (var type in hitboxPresets[i].hitboxType)
                        player.CombatController.HitboxDisable(type);
        }

        animator.speed = 1f;

        // if (!queuedNextAttack) // 캐시 값으로 분기
        // {
        //     player.StateMachine.ChangeState
        //     (
        //         player.PreviousState is AimingState ? player.AimingState : player.CombatState
        //     );

        //     animator.SetBool(PlayerAnimatorParams.UseRootMotion, false);
        // }

        queuedNextAttack = false; // 캐시 리셋
        runQueued = false;
        dodgeQueued = false;

        // animator.SetBool(PlayerAnimatorParams.CanCombo, false);
        animator.SetBool(PlayerAnimatorParams.LockTurn, false);
        animator.SetBool(PlayerAnimatorParams.SuperStance, false);
        animator.SetBool(PlayerAnimatorParams.DodgeQueued, false);
        // animator.SetBool(PlayerAnimatorParams.AttackQueued, false);

        animator.ResetTrigger(PlayerAnimatorParams.AttackTrigger);
        animator.ResetTrigger(PlayerAnimatorParams.RunTrigger);
    }

    // 4) 루트모션 스케일링
    public override void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!useRootMotion) return;

        // 애니메이터가 계산한 deltaPosition을 원하는 스케일로 적용
        Vector3 delta = animator.deltaPosition * rootMotionScale;
        delta.y = 0f;
        player.CharacterController.Move(delta);
    }

    #region PRIVATE-HELPER
    private void CacheRefs(Animator animator)
    {
        if (player == null)
            player = animator.GetComponentInParent<PlayerController>();
    }
    #endregion
}
