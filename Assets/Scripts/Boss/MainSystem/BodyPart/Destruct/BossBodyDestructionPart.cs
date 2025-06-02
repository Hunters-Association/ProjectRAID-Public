using UnityEngine;

// 보스 파괴 타입
public enum BossDestructionType
{
    Texture,    // 텍스처만 바뀌어야될 부위
    Disable,    // 사라져야 할 부위인지
}

// 보스 파괴 부위
// 파괴가 된 부위가 잘려야 될 부위가 있고 텍스쳐만 변경이 되야할 부위가 있다.
public class BossBodyDestructionPart : BossBodyPartBase
{
    public BossDestructionType destructionType;

    public float dstValue;

    public int itemId;

    protected override void Init()
    {
        base.Init();
        dstValue = partData.DstValue;
    }

    public override void TakeDamage(DamageInfo info)
    {
        base.TakeDamage(info);

        if (!IsDestruction())
        {
            dstValue = Mathf.Max(dstValue - info.destDamage, 0);

            if (IsDestruction())
            {
                DestEventInvoke();

                partDef = 0f;

                GameObject go = Instantiate(dropObj, GetPoint(), Quaternion.identity);

                // 아이템 데이터 설정 해주기
                if (go.TryGetComponent(out LostArticle lostArticle))
                    lostArticle.itemData = GameManager.Instance.Database.GetItem(itemId);

                Behaviour();
            }
        }
    }

    // 파괴 되었을 때 추가 행동
    public void Behaviour()
    {
        if (destructionVFX != null)
            destructionVFX.Play();

        // 파괴 타입에 따라 다른 로직 실행
        switch (destructionType)
        {
            case BossDestructionType.Texture:
                // TODO : 부위 파괴 시 해당 부위의 텍스쳐 변경
                break;
            case BossDestructionType.Disable:
                // 부위 파괴 시 사라져야 함
                gameObject.SetActive(false);
                break;
        }
    }

    public bool IsDestruction()
    {
        return dstValue == 0;
    }
}
