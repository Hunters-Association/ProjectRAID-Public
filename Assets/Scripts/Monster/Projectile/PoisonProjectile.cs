using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class PoisonProjectile : MonoBehaviour
{
    [Header("투사체 기본 설정")]
    public float speed = 20f; 
    public float lifeTime = 3f;

    [Header("충돌 설정")]
    public LayerMask hitLayerMask; // Initialize에서 설정됨
    public GameObject hitEffectPrefab;

    // 런타임 변수
    private Rigidbody rb;
    private GameObject attacker; // 발사자 (DamageInfo에 사용)
    private int damageAmount;
    private bool canCrit;// ★★★ 추가: 전달받을 데미지 값 ★★★


    void Awake() // Start 대신 Awake 사용 권장 (Rigidbody 참조 등)
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = false; // Kinematic이면 충돌 이벤트(Trigger 제외) 발생 안 함. Velocity 설정 위해 false 유지.
            //rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative; // 충돌 감지 모드 설정
        }
        else { Debug.LogError("Projectile is missing Rigidbody!", this); }

        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true; // 트리거로 설정
        else Debug.LogError("Projectile is missing Collider!", this);

        Destroy(gameObject, lifeTime); // 수명 설정
    }

    // Start 대신 Awake에서 처리했으므로 제거 가능
    // void Start() { ... }

    /// <summary>
    /// 투사체를 초기화합니다. 몬스터에서 호출됩니다.
    /// </summary>
    /// <param name="attackerRef">발사한 몬스터의 GameObject</param>
    /// <param name="spd">투사체의 속도</param>
    /// <param name="targetMask">충돌할 대상 레이어 마스크</param>
    /// <param name="damage">투사체의 기본 데미지</param>
    // ★★★ Initialize 파라미터에 damage 추가 ★★★
    public void Initialize(GameObject attackerRef, float spd, LayerMask targetMask, int damage, bool canCritFlag)
    {
        this.attacker = attackerRef;
        this.speed = spd; // Inspector 기본값 대신 전달받은 속도 사용
        this.hitLayerMask = targetMask;
        this.damageAmount = damage; // ★ 데미지 값 저장 ★
        this.canCrit = canCritFlag;

        // Rigidbody가 Awake에서 준비되었으므로 바로 속도 설정 가능
        if (rb != null)
        {
            rb.velocity = transform.forward * this.speed; // 저장된 speed 사용
        }
    }

    /// <summary>
    /// 다른 Collider와 Trigger 충돌이 발생했을 때 호출됩니다.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("독 맞음");
        // 자기 자신 또는 발사자와의 충돌 무시
        if (other.gameObject == attacker) return;
        if (!other.gameObject.CompareTag("Player")) return;
        // 충돌한 대상이 설정된 hitLayerMask에 속하는지 확인
        if (((1 << other.gameObject.layer) & hitLayerMask.value) != 0) // ★ .value 사용 권장 ★
        {            

            // ★★★ IDamageable 인터페이스로 데미지 주기 로직 추가 ★★★
            IDamageable damageableTarget = other.gameObject.GetComponentInParent<IDamageable>();
            if (damageableTarget != null)
            {
                // ★★★ 수정: 정의된 6개 인수 생성자 호출 ★★★
                bool isCriticalHit = this.canCrit && Random.Range(0f, 100f) < 10f;
                float finalDamage = this.damageAmount;
                if (isCriticalHit) finalDamage *= 1.5f;

                DamageInfo damageInfo = new DamageInfo(
                    Mathf.FloorToInt(finalDamage), // 1. damageAmount (int로 가정, float이면 그대로)
                    0f,                            // 2. cutDamage
                    0f,                            // 3. destDamage
                    isCriticalHit,                 // 4. isCritical
                    this.attacker,                    // 5. attacker
                    other.gameObject               // 6. receiver
                                                   // 7번째 인수(attackOrigin) 제거됨
                );
                // ★★★★★★★★★★★★★★★★★★★★★★★★

                damageableTarget.TakeDamage(damageInfo);
                // Debug.Log(...);
                Destroy(gameObject);
            }
            
        }
        // 선택적: 환경 요소 충돌 시 파괴
        else if (((1 << other.gameObject.layer) & LayerMask.GetMask("Default", "Environment")) != 0) // 예시
        {
            //if (hitEffectPrefab != null) Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
