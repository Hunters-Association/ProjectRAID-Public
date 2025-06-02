using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;

// 보스 절단 부위
public class BossBodyCuttingPart : BossBodyPartBase
{
    public float cutValue;

    protected override void Init()
    {
        base.Init();
        cutValue = partData.cutValue;
    }

    public override void TakeDamage(DamageInfo info)
    {
        base.TakeDamage(info);

        if (!IsCut())
        {
            cutValue = Mathf.Max(cutValue - info.cutDamage, 0);

            if (IsCut())
            {
                CutEventInvoke();

                if (destructionVFX != null)
                    destructionVFX.Play();

                // 절단, 오브젝트 생성
                Instantiate(dropObj, GetPoint(), Quaternion.identity);
            }
        }
    }

    public bool IsCut()
    {
        return cutValue == 0;
    }
}
