using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using ProjectRaid.Core;
using ProjectRaid.EditorTools;

/// <summary>
/// 전투 전용 로직(입력 버퍼, 히트박스, 루트모션)을 담당하는 컴포넌트.
/// PlayerController 는 이동·카메라·UI 등 일반적인 캐릭터 제어에 집중하고,
/// 이 스크립트가 순수 전투 흐름만 담당하도록 분리한다.
/// </summary>
[DisallowMultipleComponent]
public sealed class PlayerCombatController : MonoBehaviour
{
    #region ▶ 입력 버퍼 ===========================================================================
    private bool attackBuffered;
    private bool chargeBuffered;
    private bool dodgeBuffered;
    private float attackBufferTimer;
    private float chargeBufferTimer;
    private float dodgeBufferTimer;
    [FoldoutGroup("입력버퍼", ExtendedColor.Orange)]
    [SerializeField, Tooltip("버퍼 지속 시간 (초)")] private float bufferDuration = 0.25f;

    /// <summary>
    /// AttackStateBehaviour 에서 호출. true 를 반환하면서 즉시 버퍼를 소모한다.
    /// </summary>
    public bool ConsumeAttackBuffered()
    {
        if (!attackBuffered) return false;
        attackBuffered = false;
        return true;
    }

    public bool ConsumeChargeBuffered()
    {
        if (!chargeBuffered) return false;
        chargeBuffered = false;
        return true;
    }

    public bool ConsumeDodgeBuffered()
    {
        if (!dodgeBuffered) return false;
        dodgeBuffered = false;
        return true;
    }
    #endregion

    #region ▶ 히트박스 ===========================================================================
    public enum HitboxType { Weapon, Projectile, AOS }

    [Serializable]
    private class HitboxData
    {
        public HitboxType type;
        public Collider collider;
        public bool enabledBySkill;
    }

    [Serializable]
    private class HurtboxData
    {
        public Collider collider;
        public bool enabledBySkill;
    }

    [FoldoutGroup("히트박스", ExtendedColor.GreenYellow)]
    [SerializeField] private List<HitboxData> hitboxes = new();
    [SerializeField] private HurtboxData hurtbox;

    public void SetHitbox(AttackComponent weapon)
    {
        foreach (var hb in hitboxes)
        {
            if (hb.type is HitboxType.Weapon)
                hb.collider = weapon.GetCollider();

            hb.collider.GetComponent<HitTriggerRelay>().Setup(weapon);
        }
    }

    public void HitboxEnable(HitboxType id)
    {
        var hb = hitboxes.Find(h => h.type == id);
        if (hb != null && !hb.enabledBySkill)
        {
            hb.collider.enabled = true;
            hb.enabledBySkill = true;
        }
    }

    public void HitboxDisable(HitboxType id)
    {
        var hb = hitboxes.Find(h => h.type == id);
        if (hb != null && hb.collider != null && hb.enabledBySkill)
        {
            hb.collider.enabled = false;
            hb.enabledBySkill = false;
        }
    }

    public void HurtboxEnable()
    {
        if (hurtbox != null && hurtbox.collider != null && !hurtbox.enabledBySkill)
        {
            hurtbox.collider.enabled = true;
            hurtbox.enabledBySkill = true;
        }
    }

    public void HurtboxDisable()
    {

        if (hurtbox != null && hurtbox.enabledBySkill)
        {
            hurtbox.collider.enabled = false;
            hurtbox.enabledBySkill = false;
        }
    }
    #endregion

    #region ▶ 루트모션 ===========================================================================
    [FoldoutGroup("루트모션", ExtendedColor.Cyan)]
    [SerializeField] private bool useRootMotion = true;
    [Range(0f, 1f), SerializeField] private float rootMotionScale = 1f;

    // 캐시
    private CharacterController cc;
    private Animator anim;

    private void Awake()
    {
        cc = GetComponent<CharacterController>();
        anim = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        HandleAttackInput();
        UpdateBufferTimer();
    }

    private void OnAnimatorMove()
    {
        if (!useRootMotion || rootMotionScale <= 0f) return;
        if (!anim.GetBool(PlayerAnimatorParams.UseRootMotion)) return;

        Vector3 delta = anim.deltaPosition * rootMotionScale;
        delta.y = 0f;
        cc.Move(delta);
    }
    #endregion

    #region ▶ 내부 입력 처리 ===========================================================================
    private PlayerController player;
    private void Start() => player = GetComponent<PlayerController>();

    private void HandleAttackInput()
    {
        if (player == null) return;

        var input = player.InputHandler;
        if (input == null) return;

        if (player.CurrentState is CombatState or AimingState or AttackState)
        {
            if (input.IsAttackPressed)
            {
                attackBuffered = true;
                attackBufferTimer = bufferDuration;
            }

            if (input.IsChargeHolded)
            {
                chargeBuffered = true;
                chargeBufferTimer = bufferDuration;
            }

            if (input.IsDodgePressed)
            {
                dodgeBuffered = true;
                dodgeBufferTimer = bufferDuration;
            }
        }
    }

    private void UpdateBufferTimer()
    {
        if (attackBufferTimer > 0f)
        {
            attackBufferTimer -= Time.deltaTime;
            if (attackBufferTimer <= 0f) attackBuffered = false;
        }

        if (chargeBufferTimer > 0)
        {
            chargeBufferTimer -= Time.deltaTime;
            if (chargeBufferTimer <= 0f) chargeBuffered = false;
        }

        if (dodgeBufferTimer > 0)
        {
            dodgeBufferTimer -= Time.deltaTime;
            if (dodgeBufferTimer <= 0f) dodgeBuffered = false;
        }
    }
    #endregion
}
