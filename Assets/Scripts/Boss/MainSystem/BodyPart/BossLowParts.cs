using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossLowParts : MonoBehaviour, IDamageable
{
    public BossHighParts highParts;

    private void Awake()
    {
        if (highParts == null)
            Debug.Log("상위 파츠가 연결이 안되어있습니다.");
        else
            highParts.partsList.Add(this);
    }

    public void TakeDamage(DamageInfo info)
    {
        highParts.TakeDamage(info);
    }
}
