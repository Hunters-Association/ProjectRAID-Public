using UnityEngine;
using ProjectRaid.EditorTools;

/// <summary>
/// Player HUD 전체를 중재(Mediator)하는 클래스
/// </summary>
public class PlayerHUD : MonoBehaviour
{
    [FoldoutGroup("HUD", ExtendedColor.Silver)]
    public GaugeBarView healthBar;
    public GaugeBarView staminaBar;
    public BuffPanelView buffPanel;

    private StatController stats;
    private BuffSystem buffs;

    public void Bind(StatController statController, BuffSystem buffSystem)
    {
        stats = statController;
        buffs = buffSystem;

        var runtime = stats.Runtime;
        runtime.OnHealthChanged += HandleHealth;
        runtime.OnStaminaChanged += HandleStamina;
        runtime.OnLevelChanged += HandleLevelUp;
        buffs.OnBuffAdded += buffPanel.AddIcon;
        buffs.OnBuffRemoved += buffPanel.RemoveIcon;

        // 초기값 동기화
        HandleHealth(runtime.CurrentHealth, runtime.MaxHealth);
        HandleStamina(runtime.CurrentStamina, runtime.MaxStamina);
    }

    private void OnDisable()
    {
        if (stats != null)
        {
            var r = stats.Runtime;
            r.OnHealthChanged -= HandleHealth;
            r.OnStaminaChanged -= HandleStamina;
            r.OnLevelChanged -= HandleLevelUp;
        }
        if (buffs != null)
        {
            buffs.OnBuffAdded -= buffPanel.AddIcon;
            buffs.OnBuffRemoved -= buffPanel.RemoveIcon;
        }
    }

    private void HandleHealth(float cur, float max) => healthBar.SetRatio(cur / max, GaugeBarType.Width);
    private void HandleStamina(float cur, float max) => staminaBar.SetRatio(cur / max, GaugeBarType.Width);
    private void HandleLevelUp(int lv)
    {
        // TODO: 레벨업 연출
    }
}
