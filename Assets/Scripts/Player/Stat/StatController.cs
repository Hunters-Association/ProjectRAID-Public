using UnityEngine;
using ProjectRaid.EditorTools;

/// <summary>
/// SO + ModifierStack을 이용해 최종 스탯을 계산 (경험치, 피해 처리 등을 담당)
/// </summary>
public class StatController : MonoBehaviour
{
    [FoldoutGroup("Data", ExtendedColor.Silver)]
    [SerializeField] private PlayerStatData baseData;
    [SerializeField] private int expBase = 100;
    [SerializeField] private int expStep = 50;

    [FoldoutGroup("스태미나 설정", ExtendedColor.Cyan)]
    [SerializeField] private float staminaConsumeRate = 2.5f;
    [SerializeField] private float staminaRegenRate = 20f;
    [SerializeField] private float regenDelay = 1f;
    // [SerializeField] private float consumeDelay = 1f;

    [FoldoutGroup("디버그/체력", ExtendedColor.DodgerBlue)]
    [SerializeField] private float health = 100f;
    [SerializeField] private float stamina = 100f;

    private float regenCooldownTimer = 0f;

    public float StaminaConsumeRate => staminaConsumeRate;
    public PlayerStatsRuntime Runtime { get; private set; } = new();
    private readonly ModifierStack modifierStack = new();

    public void ApplyBaseStats()
    {
        Runtime.Level = 1;

        Runtime.AttackPower = baseData.attackCurve.Evaluate(Runtime.Level);
        Runtime.AttackSpeed = baseData.attackSpeedCurve.Evaluate(Runtime.Level);
        Runtime.CritChance = baseData.critChanceCurve.Evaluate(Runtime.Level) / 100f;
        Runtime.CritDamage = baseData.critDamageCurve.Evaluate(Runtime.Level) / 100f;
        Runtime.MaxHealth = baseData.maxHealthCurve.Evaluate(Runtime.Level);
        Runtime.Defense = baseData.defenseCurve.Evaluate(Runtime.Level);

        Runtime.MaxStamina = baseData.maxStamina;
        Runtime.MoveSpeed = baseData.baseMoveSpeed;
        Runtime.RunSpeed = baseData.baseRunSpeed;

        Runtime.InitHealth(baseData.maxHealthCurve.Evaluate(Runtime.Level));
        Runtime.InitStamina(baseData.maxStamina);

        regenCooldownTimer = 0f;
    }

    #region EXP/Level
    private int ExpToNext(int lv) => expBase + (lv - 1) * expStep;

    public void GainExp(int amount)
    {
        Runtime.Exp += amount;
        while (Runtime.Exp >= ExpToNext(Runtime.Level))
        {
            Runtime.Exp -= ExpToNext(Runtime.Level);
            Runtime.LevelUp();
            ApplyBaseStats();
        }
    }
    #endregion

    #region Modifier API
    public void AddModifier(StatModifier mod)
    {
        modifierStack.Add(mod);
    }

    public void RemoveModifiersFrom(object source)
    {
        modifierStack.RemoveBySource(source);
    }

    private float Final(StatType t) => Base(t) + modifierStack[t];

    private float Base(StatType t) => t switch
    {
        StatType.MaxHealth => Runtime.MaxHealth,
        StatType.AttackPower => Runtime.AttackPower,
        StatType.Defense => Runtime.Defense,
        StatType.CriticalChance => Runtime.CritChance,
        StatType.CriticalDamage => Runtime.CritDamage,
        _ => 0
    };
    #endregion

    #region Damage / Heal
    public void TakeDamage(float amount)
    {
        // float finalDmg = amount;
        float finalDmg = Mathf.Max(0f, amount - Final(StatType.Defense));
        health = Runtime.CurrentHealth - finalDmg;
        Runtime.SetHealth(health);
    }

    public void Heal(float amount)
    {
        health = Runtime.CurrentHealth + amount;
        Runtime.SetHealth(health);
    }
    #endregion

    #region Stamina
    public void ConsumeStamina(float amount)
    {
        if (Runtime.CurrentStamina <= 0f)
        {
            regenCooldownTimer = regenDelay;
            return;
        }
        
        stamina = Mathf.Max(0f, Runtime.CurrentStamina - amount);
        Runtime.SetStamina(stamina);
        regenCooldownTimer = regenDelay;
    }

    public void RecoverStamina(float amount)
    {
        stamina = Runtime.CurrentStamina + amount;
        Runtime.SetStamina(stamina);
    }

    public void TickStamina(float deltaTime)
    {
        if (regenCooldownTimer > 0f)
        {
            regenCooldownTimer -= deltaTime;
            return;
        }

        if (Runtime.CurrentStamina < Runtime.MaxStamina)
        {
            stamina = Runtime.CurrentStamina + staminaRegenRate * deltaTime;
            Runtime.SetStamina(stamina);
        }
    }

    public void TriggerStaminaRegenDelay()
    {
        regenCooldownTimer = regenDelay;
    }
    #endregion
}
