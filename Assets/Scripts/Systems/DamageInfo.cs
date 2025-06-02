using UnityEngine;

public enum CrowdControlType
{
    Default,
    Roar,
    Stunable,
    Count
}

public struct DamageInfo
{
    public float damageAmount;
    public float cutDamage;
    public float destDamage;
    public bool isCritical;
    public GameObject attacker;
    public GameObject receiver;
    public CrowdControlType ccType;     // 군중 제어 타입
    public float ccEnableTime;          // 군중 제어 유지 시간

    public DamageInfo(float damageAmount, float cutDamage, float destDamage, bool isCritical, GameObject attacker, GameObject receiver, CrowdControlType ccType = CrowdControlType.Default, float ccEnableTime = 0f)
    {
        this.damageAmount = damageAmount;
        this.cutDamage = cutDamage;
        this.destDamage = destDamage;
        this.isCritical = isCritical;
        this.attacker = attacker;
        this.receiver = receiver;
        this.ccType = ccType;
        this.ccEnableTime = ccEnableTime;
    }
}