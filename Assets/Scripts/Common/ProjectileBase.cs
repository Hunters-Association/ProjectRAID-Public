using UnityEngine;
using ProjectRaid.EditorTools;

public abstract class ProjectileBase : MonoBehaviour
{
    [FoldoutGroup("공통 속성", ExtendedColor.White)]
    [SerializeField] protected float speed = 50f;
    [SerializeField] protected float maxDistance = 100f;
    [SerializeField] protected float lifeTime = 5f;

    protected Vector3 startPosition;

    protected virtual void Start()
    {
        startPosition = transform.position;
        Destroy(gameObject, lifeTime); // 일정 시간 후 자동 파괴
    }

    protected virtual void Update()
    {
        Move();

        if (Vector3.Distance(startPosition, transform.position) > maxDistance)
            Destroy(gameObject);
    }

    protected abstract void Move();
    protected abstract void OnHit(Collider target);
}
