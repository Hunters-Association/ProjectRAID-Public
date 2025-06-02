using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossDamageTest : MonoBehaviour, IDamageable
{
    public void TakeDamage(DamageInfo info)
    {
        Debug.Log($"허수아비 타격! ({info.damageAmount} 데미지)");
    }
}
