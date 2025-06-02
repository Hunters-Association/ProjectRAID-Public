using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class WebProjectile : MonoBehaviour
{
    [Header("설정")]
    public float speed = 10f;
    public float lifeTime = 5f;
    public LayerMask hitLayerMask; // Initialize에서 설정됨
    public GameObject hitEffectPrefab;

    [Header("거미줄 효과")]
    [Tooltip("거미줄이 적용할 슬로우 효과 지속 시간")]
    public float slowDuration = 3.0f;
    [Tooltip("이동 속도 감소율 (0.0 ~ 1.0)")]
    [Range(0f, 1f)]
    public float slowAmount = 0.5f; // 50% 감소

    // 런타임 변수
    private Rigidbody rb;
    private GameObject attacker;
    private int damageAmount; // ★ 데미지 저장 변수 (0 가능) ★

    void Awake() // Start 대신 Awake 사용 권장
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null) { rb.useGravity = false; rb.isKinematic = false; /*...*/ }
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true; // 거미줄은 Trigger 사용
        else Debug.LogError("Missing Collider!", this);
        Destroy(gameObject, lifeTime);
    }

    // ★ Initialize 함수 수정 (데미지 파라미터 추가) ★
    /// <summary>
    /// 거미줄 투사체를 초기화합니다. Monster에서 호출됩니다.
    /// </summary>
    /// <param name="attackerRef">발사자</param>
    /// <param name="spd">속도</param>
    /// <param name="targetMask">타겟 레이어</param>
    /// <param name="damage">기본 데미지 (0 가능)</param>
    public void Initialize(GameObject attackerRef, float spd, LayerMask targetMask, int damage)
    {
        this.attacker = attackerRef;
        this.speed = spd;
        this.hitLayerMask = targetMask;
        this.damageAmount = damage; // ★ 데미지 값 저장 ★

        if (rb != null) rb.velocity = transform.forward * speed;
    }


    private void OnTriggerEnter(Collider other)
    {        
        // 발사자 충돌 무시
        if (other.gameObject == attacker)
        {
            Debug.Log("WebProjectile hit attacker, ignoring.");
            return;
        }
        if (!other.gameObject.CompareTag("Player")) return;

        // 타겟 레이어인지 확인
        if (((1 << other.gameObject.layer) & hitLayerMask.value) != 0)
        {            

            // ★★★ 데미지 처리 로직 추가 (DamageInfo 사용) ★★★
            IDamageable damageableTarget = other.gameObject.GetComponentInParent<IDamageable>();
            if (damageableTarget != null) // 데미지 가능한 대상인지 확인
            {
                // 데미지 정보 생성 (거미줄 데미지는 낮거나 0)
                DamageInfo damageInfo = new DamageInfo(
                    this.damageAmount,   // 저장된 데미지 (0일 수 있음)
                    0f,                 // cutDamage
                    0f,                 // destDamage
                    false,              // isCritical
                    this.attacker,      // attacker
                    other.gameObject   // receiver
                    
                );

                // 대상의 TakeDamage 호출 (데미지가 0이라도 호출하여 피격 판정은 알릴 수 있음)
                damageableTarget.TakeDamage(damageInfo);
                if (this.damageAmount > 0)
                {
                    Destroy(gameObject);
                }
            }          
                       
           
        }
        // 환경 충돌 처리
        else if (((1 << other.gameObject.layer) & LayerMask.GetMask("Default", "Environment")) != 0)
        {            
            if (hitEffectPrefab != null) Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}

