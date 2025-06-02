using UnityEngine;
using ProjectRaid.EditorTools;

public class BulletProjectile : ProjectileBase
{
    [FoldoutGroup("개별 속성", ExtendedColor.White)]
    // [SerializeField] private TrailRenderer trail;
    [SerializeField] private LayerMask hurtboxMask;

    [FoldoutGroup("HS_Projectile Effects")]
    [SerializeField] private GameObject hitGO;
    [SerializeField] private ParticleSystem hitPS;
    [SerializeField] private ParticleSystem projectilePS;
    [SerializeField] private GameObject flash;
    [SerializeField] private GameObject[] detached;
    [SerializeField] private Light lightSource;
    [SerializeField] private Vector3 rotationOffset;
    [SerializeField] private float hitOffset = 0f;
    [SerializeField] private bool useFirePointRotation = false;

    private DamageInfo info;
    private bool hit = false;

    public virtual void Setup(DamageInfo info)
    {
        this.info = info;
    }

    protected override void Start()
    {
        base.Start();

        // if (trail != null) trail.Clear();
        if (flash != null) flash.transform.parent = null;
        if (lightSource != null) lightSource.enabled = true;

        Destroy(gameObject, lifeTime);
    }

    protected override void Move()
    {
        if (hit) return;

        Vector3 currentPosition = transform.position;
        Vector3 nextPosition = currentPosition + Time.deltaTime * speed * transform.forward;
        Vector3 direction = nextPosition - currentPosition;
        float distance = direction.magnitude;

        if (Physics.Raycast(currentPosition, direction.normalized, out RaycastHit hitInfo, distance, hurtboxMask))
        {
            if (hitGO != null) PlayHitEffect(hitInfo);
            OnHit(hitInfo.collider);
            transform.position = hitInfo.point;

            hit = true;

            Debug.Log($"[BulletProjectile] {hitInfo.collider.name} 명중!");
        }
        else
        {
            transform.position = nextPosition;
        }
    }

    protected override void OnHit(Collider other)
    {
        if (other.TryGetComponent(out IDamageable target))
        {
            info.receiver = other.gameObject;
            target.TakeDamage(info);

            GameManager.Instance.DamagePopup.ShowPopup(info.damageAmount, info.isCritical, transform.position);
        }

        if (projectilePS != null)
        {
            projectilePS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        foreach (var d in detached)
        {
            if (d != null && d.TryGetComponent(out ParticleSystem ps)) ps.Stop();
        }

        if (lightSource != null) lightSource.enabled = false;

        Destroy(gameObject, hitPS != null ? hitPS.main.duration : 1f);
    }

    private void PlayHitEffect(RaycastHit hitInfo)
    {
        Vector3 pos = hitInfo.point + hitInfo.normal * hitOffset;
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);

        if (hitGO != null)
        {
            hitGO.transform.position = pos;

            if (useFirePointRotation)
                hitGO.transform.rotation = transform.rotation * Quaternion.Euler(0, 180f, 0);
            else if (rotationOffset != Vector3.zero)
                hitGO.transform.rotation = Quaternion.Euler(rotationOffset);
            else
                hitGO.transform.rotation = rot;

            if (hitPS != null)
                hitPS.Play();
        }
    }
}
