using UnityEngine;

public class BossBodyCuttingListParts : BossHighParts
{
    public float cutValue;

    [SerializeField] private Transform dropTransform;
    [SerializeField] GameObject disableObject;

    protected override void Init()
    {
        base.Init();

        if (disableObject != null) disableObject.SetActive(true);

        if (partData == null)
            Debug.LogAssertion("파츠 데이터가 연결이 되어있지 않습니다.");
        else
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

                partDef = 0f;

                if (disableObject != null) disableObject.SetActive(false);

                if(destructionVFX != null)
                    destructionVFX.Play();

                // 절단, 오브젝트 생성
                Instantiate(
                    dropObj, 
                    GetPoint(dropTransform != null? dropTransform : null), 
                    Quaternion.identity
                    );
            }
        }
    }

    public bool IsCut()
    {
        return cutValue == 0;
    }
}
