using DG.Tweening;
using System.Collections;
using UnityEngine;

public class BearGomProjectile : MonoBehaviour
{
    public BearGomProjectileManager projectileManager;
    public Boss boss;
    public float power;
    public float damageThreshold = 3f; // 데미지를 줄 수 있는 최대 속도

    public ParticleSystem bombParticle;     // 터지는 파티클

    private float aliveTime = 10f;     // 투사체가 발사되고 살아있을 시간
    private float enableTime;          // 투사체가 활성화 된 시간
    private float lightingTime = 1.5f; // 투사체가 밝아지는 시간 (터지기 전)

    private MeshRenderer bombMesh;
    private Rigidbody rigid;
    // 땅에 닿았을 때 플레이어를 감지할 콜라이더
    private Collider detectCollider;
    private Collider hitboxCollider;
    private Collider bombHitboxCollider;   // 터질 때 사용할 콜라이더

    //================ [Coroutine] ================
    // 투사체 속도 확인
    private Coroutine checkProjectileSpeedCo;
    // 투사체가 살아있을 시간인지 확인
    private Coroutine checkAliveTimeCo;
    private Coroutine bombLavaCo;

    private Material lavaMaterial;

    // 데미지를 줄 수 있는 속도인가?
    public bool IsDamageableSpeed() => rigid.velocity.magnitude > damageThreshold;
    public bool IsAliveTime() => Time.time - enableTime < aliveTime;

    private void Awake()
    {
        bombMesh = transform.GetChild(0).GetComponent<MeshRenderer>();
        rigid = GetComponent<Rigidbody>();
        detectCollider = GetComponent<Collider>();
        hitboxCollider = transform.GetChild(1).GetComponent<Collider>();
        bombHitboxCollider = transform.GetChild(2).GetComponent<Collider>();
        lavaMaterial = bombMesh.GetComponent<Renderer>().material;
    }

    private void Start()
    {
        BossHitbox[] hitboxes = GetComponentsInChildren<BossHitbox>(true);

        for (int i = 0; i < hitboxes.Length; i++)
        {
            hitboxes[i].boss = boss;
        }
    }

    private void OnEnable()
    {
        if (boss == null) return;

        bombMesh.enabled = true;

        lavaMaterial.SetColor("_EmissionColor", Color.white);

        enableTime = Time.time;

        rigid.velocity = Vector3.zero;

        rigid.AddForce(boss.transform.forward * power, ForceMode.Impulse);

        hitboxCollider.gameObject.SetActive(true);
        bombHitboxCollider.gameObject.SetActive(false);
        detectCollider.enabled = false;

        // 투사체의 속도 확인
        checkProjectileSpeedCo = StartCoroutine(CheckProjectileSpeed());
        checkAliveTimeCo = StartCoroutine(CheckAliveTime());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator CheckProjectileSpeed()
    {
        yield return null;

        // 속도가 줄어들 때 까지 대기
        yield return new WaitWhile(()=> IsDamageableSpeed());

        // 속도가 줄어들었다면 콜라이더 꺼줌
        hitboxCollider.gameObject.SetActive(false);

        // 감지 콜라이더 켜줌
        detectCollider.enabled = true;
    }

    private IEnumerator CheckAliveTime()
    {
        yield return new WaitWhile(() => IsAliveTime());

        bombLavaCo = StartCoroutine(BombLava());
    }


    // 플레이어가 감지되었다면 폭발
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            StopCoroutine(CheckAliveTime());
            bombLavaCo = StartCoroutine(BombLava());
        }
    }


    // 용암 덩어리 폭발
    private IEnumerator BombLava()
    {
        // 색 밝게 만들어주기
        lavaMaterial.DOColor(new Color(6, 6, 6, 1), "_EmissionColor", lightingTime);

        yield return new WaitForSeconds(lightingTime);

        if(bombParticle != null)
        {
            // 터지는 파티클 실행
            bombParticle.Play();
            bombHitboxCollider.gameObject.SetActive(true);

            bombMesh.enabled = false;

            // 파티클이 끝날 때까지 대기
            yield return new WaitWhile(() => bombParticle.IsAlive());
        }

        if (boss is BossBearGom)
        {
            BossBearGom bearGom = boss as BossBearGom;

            bearGom.projectileManager.ReturnProjectile(gameObject);
        }
    }

}
