using ProjectRaid.EditorTools;
using System;
using UnityEngine;

// 보스의 각 부위들
// 데미지를 받게 됨
public class BossBodyPartBase : MonoBehaviour, IDamageable
{ 
    [SerializeField] protected bool hasPartsData;

    public BossBodyPartData partData;
    protected float partDef;

    [ShowIf(nameof(hasPartsData), true)]
    public GameObject dropObj;          // 파괴나 절단시 생성될 오브젝트

    public event Action<float> onTakeDamage;
    public event Action onDestPart;     // 파괴 되었을 때 이벤트
    public event Action onCutPart;      // 절단 되었을 때 이벤트

    [ShowIf(nameof(hasPartsData), true)]
    public LayerMask groundLayerMask;

    // ================ VFX ================
    [ShowIf(nameof(hasPartsData), true)] public ParticleSystem destructionVFX;

    private void OnEnable()
    {
        if(partData != null)
            partDef = partData.partDef;

        Init();
    }

    protected virtual void Init()
    {
        groundLayerMask = LayerMask.GetMask("Environment");
    }

    public virtual void TakeDamage(DamageInfo info)
    {
        onTakeDamage?.Invoke(CalculateDamage(info));
    }

    public float CalculateDamage(DamageInfo info)
    {
        return Mathf.Max(0f, info.damageAmount/* - partDef*/);
    }

    protected void CutEventInvoke()
    {
        onCutPart?.Invoke();
    }

    protected void DestEventInvoke()
    {
        onDestPart?.Invoke();
    }

    // 유실물이나 절단 된 부위의 오브젝트가 생성될 위치
    public Vector3 GetPoint(Transform transform = null)
    {
        if (transform == null) transform = this.transform;

        Ray ray = new Ray(transform.position, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hit, 20f, groundLayerMask))
        {
            return hit.point;
        }

        return transform.position;
    }
}
