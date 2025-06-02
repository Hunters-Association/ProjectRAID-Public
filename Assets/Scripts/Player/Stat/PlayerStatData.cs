using UnityEngine;
using ProjectRaid.EditorTools;

[CreateAssetMenu(fileName = "PlayerStatData", menuName = "Data/Player/StatData")]
public class PlayerStatData : ScriptableObject
{
    [FoldoutGroup("스탯 곡선 : X = 레벨,  Y = 값", ExtendedColor.Aqua)]
    public AnimationCurve attackCurve = AnimationCurve.Linear(1, 10, 99, 200);
    public AnimationCurve attackSpeedCurve = AnimationCurve.Linear(1, 1, 99, 1);
    public AnimationCurve critChanceCurve = AnimationCurve.Linear(1, 5, 99, 25); // %
    public AnimationCurve critDamageCurve = AnimationCurve.Linear(1, 50, 99, 250); // %
    public AnimationCurve maxHealthCurve = AnimationCurve.Linear(1, 100, 99, 1000);
    public AnimationCurve defenseCurve = AnimationCurve.Linear(1, 0, 99, 150);

    [FoldoutGroup("스태미나 / 이동 스탯", ExtendedColor.White)]
    public float maxStamina = 100f;
    public float baseMoveSpeed = 2f;
    public float baseRunSpeed = 6f;

    [FoldoutGroup("특수 스탯", ExtendedColor.White)]
    public float knockbackResistance = 0f;
    public float invincibleDuration = 0f;
}
