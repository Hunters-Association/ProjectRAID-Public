using System;
using ProjectRaid.EditorTools;
using UnityEngine;

// 파괴될 파츠가 여러개로 나누어져있는 파츠인경우
public class BossBodyDestructionListPart : BossHighParts
{
    public BossDestructionType destructionType;

    [ShowIf(nameof(destructionType), BossDestructionType.Disable)]
    public GameObject disableObject;        // 파괴 되었을 때 사라질 오브젝트

    public float dstValue;

    public int itemId;

    [SerializeField] protected Material originMaterial;
    [SerializeField] protected Material destructionMaterial;       // 파괴 되었을 때 바뀔 메테리얼

    protected override void Init()
    {
        base.Init();

        if (disableObject != null) disableObject.SetActive(true);

        if (partData == null)
            Debug.LogAssertion("파츠 데이터가 연결이 되어있지 않습니다.");
        else
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
                if (disableObject != null)
                    disableObject.SetActive(false);
                else
                    gameObject.SetActive(false);
                break;
        }
    }

    public bool IsDestruction()
    {
        return dstValue == 0;
    }
}
