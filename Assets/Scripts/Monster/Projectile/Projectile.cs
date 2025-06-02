using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Projectile : MonoBehaviour // 돌멩이 스크립트
{
    public float speed = 12f;
    public int damage = 8;
    public float lifeTime = 4f;
    public LayerMask hitLayerMask;
    public GameObject hitEffectPrefab;

    private GameObject attacker;
    private Rigidbody rb;
    private bool _isDestroyed = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;  // ★ 돌멩이는 중력 사용 (포물선) ★
            rb.isKinematic = false; // ★ 물리적 이동 및 충돌 위해 false ★
            // CollisionDetectionMode 설정은 그대로 두어도 좋음
            //rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
        else { Debug.LogError("Missing Rigidbody!", this); }

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            // ★★★ isTrigger 를 false 로 설정하여 물리적 충돌 사용 ★★★
            col.isTrigger = true;
        }
        else { Debug.LogError("Missing Collider!", this); }

        Destroy(gameObject, lifeTime);
    }

    public void Initialize(GameObject attackerRef, int dmg, float spd, LayerMask targetMask)
    {        
        this.attacker = attackerRef;
        this.damage = dmg;
        this.speed = spd;
        this.hitLayerMask = targetMask;
        if (rb != null)
        {
            // ★ 초기 속도 설정 (발사 방향으로 힘 가하기) ★
            // velocity를 직접 설정해도 되지만, AddForce가 더 자연스러울 수 있음
            // rb.velocity = transform.forward * speed;
            // 또는 Impulse 모드로 순간적인 힘 가하기
            rb.AddForce(transform.forward * speed, ForceMode.Impulse);            
        }
    }

    // ★★★ OnCollisionEnter 사용 (isTrigger=false) ★★★
    private void OnTriggerEnter(Collider other)
    {
        if (_isDestroyed) return; // 이미 파괴 처리 중이면 무시

        // 발사자 자신과는 충돌하지 않도록
        if (attacker != null && other.gameObject == attacker)
        {
            // Debug.Log("Projectile hit attacker, ignoring.");
            return;
        }

        // 충돌 대상이 플레이어 태그를 가지고 있고, 지정된 레이어 마스크에 속하는지 확인
        // (주석: 원래 코드에서는 플레이어 태그만 체크하고 있었으나, hitLayerMask도 사용하는 것이 일반적)
        bool hitValidTarget = false;
        if (((1 << other.gameObject.layer) & hitLayerMask.value) != 0) // LayerMask.value 사용
        {
            if (other.gameObject.CompareTag("Player")) // 플레이어 태그도 확인해야 한다면 이 조건 추가
            {
                hitValidTarget = true;
            }
            hitValidTarget = true; // 여기서는 레이어 마스크만으로 유효 타겟 판단
        }


        if (hitValidTarget)
        {
            IDamageable damageableTarget = other.gameObject.GetComponentInParent<IDamageable>(); // 부모에서 검색
            if (damageableTarget == null) // 부모에 없으면 자식에서도 검색 (선택적)
            {
                damageableTarget = other.gameObject.GetComponentInChildren<IDamageable>(true);
            }


            if (damageableTarget != null)
            {
                DamageInfo damageInfo = new DamageInfo(
                    this.damage, 0f, 0f, false, this.attacker, other.gameObject // 피격자는 other.gameObject
                );
                damageableTarget.TakeDamage(damageInfo);
                // Debug.Log($"돌멩이 충돌! 대상: {other.gameObject.name}, 데미지: {this.damage}");
            }
            // 유효 타겟에 맞았으므로 이펙트 생성 및 파괴
            SpawnImpactEffectAndDestroy(other.ClosestPoint(transform.position), Quaternion.LookRotation(transform.position - other.transform.position));
        }
        // 환경 또는 지정되지 않은 레이어와 충돌 (트리거가 아닌 콜라이더와 물리적 충돌은 OnCollisionEnter에서 처리,
        // 여기서는 isTrigger=true 이므로 모든 충돌이 OnTriggerEnter로 들어옴)
        // "Default" 또는 "Environment" 레이어에 맞았을 때도 파괴 및 이펙트
        else if (((1 << other.gameObject.layer) & LayerMask.GetMask("Default", "Environment")) != 0 && !other.isTrigger) // 트리거가 아닌 환경 요소와 충돌 시
        {
            // Debug.Log($"돌멩이 환경 충돌! 대상: {other.gameObject.name}");
            SpawnImpactEffectAndDestroy(other.ClosestPoint(transform.position), Quaternion.LookRotation(transform.position - other.transform.position));
        }
        // 그 외의 트리거와 충돌했으나 유효 타겟이 아닌 경우, 무시하거나 관통하도록 둘 수 있습니다.
        // 만약 어떤 트리거든 맞으면 사라져야 한다면, else 블록을 추가합니다.
        // else if (other.isTrigger) { /* 특정 트리거에 대한 처리 */ }
    }

    private void DestroyByLifeTime()
    {
        if (!_isDestroyed) // 아직 파괴되지 않았다면
        {
            // Debug.Log("돌멩이 수명 다함. 파괴 및 이펙트 생성.");
            SpawnImpactEffectAndDestroy(transform.position, transform.rotation); // 현재 위치와 방향으로 이펙트
        }
    }

    private void SpawnImpactEffectAndDestroy(Vector3 position, Quaternion rotation)
    {
        if (_isDestroyed) return;
        _isDestroyed = true; // 파괴 처리 시작 플래그

        CancelInvoke(nameof(DestroyByLifeTime)); // 혹시 모를 중복 파괴 방지를 위해 Invoke 취소

        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, position, rotation);
            // Debug.Log("Impact effect instantiated at " + position);
        }
        Destroy(gameObject);
        // Debug.Log("Projectile destroyed.");
    }
    // ★★★ OnTriggerEnter 제거 ★★★
    // private void OnTriggerEnter(Collider other) { ... }
}

