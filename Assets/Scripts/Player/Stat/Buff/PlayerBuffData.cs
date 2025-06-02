using UnityEngine;

[CreateAssetMenu(fileName = "PlayerBuffData", menuName = "Data/Player/BuffData")]
public class PlayerBuffData : ScriptableObject
{
    public string buffName;
    [TextArea] public string description;
    public Sprite icon;
    public Color iconTint = Color.white;
    public float duration = 10f;
    public StatType targetStat = StatType.AttackPower;
    public float value = 10f;
}