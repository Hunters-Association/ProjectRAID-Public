using ProjectRaid.EditorTools;
using ProjectRaid.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BossHitboxDamageType
{
    OneShot,        // 단발 데미지
    Continous,      // 지속 데미지
}

public struct ContinousDamageable
{
    public IDamageable player;          // 데미지를 입은 플레이어
    public float takeDamageTime;        // 데미지를 입은 시간
}

public class BossHitbox : MonoBehaviour
{
    public Boss boss;

    public float attackDamage;      // 0이면 보스의 기본 데미지

    public BossHitboxDamageType type;

    // =============== [CrowdControlType] ===============
    public CrowdControlType ccType;

    [ShowIf(nameof(ccType), CrowdControlType.Roar, CrowdControlType.Stunable)]
    public float ccEnableTime;

    // =============== [LowPartsHitbox] ===============

    [SerializeField] private bool hasLowPartsHitbox;

    [ShowIf(nameof(hasLowPartsHitbox), true)]
    public List<BossLowPartsHitbox> lowHitboxList = new();

    // =============== [OneShotOption] ===============
    private readonly List<IDamageable> damageableList = new(); // 해당 공격에 데미지를 입은 리스트

    // =============== [ContinousOption] ===============
    private readonly Dictionary<IDamageable, float> continousDamageableDic = new(); // 해당 공격에 데미지를 입은 리스트
    [ShowIf(nameof(type), BossHitboxDamageType.Continous)]
    public float takeDamageInterval;
    private Coroutine continousCheckDamageableCo;

    public bool HasLowPartsHitbox() => hasLowPartsHitbox;
    public void SetAttackDamage(float damage) => attackDamage = damage;

    // 데미지를 받은 적이 있는 적인지 확인
    private bool CheckDamageableList(IDamageable player) => damageableList.Contains(player);

    private bool CheckContinousDamageableList(IDamageable player) => continousDamageableDic.ContainsKey(player);

    private void Awake()
    {
        if(boss == null)
        {
            boss = GetComponentInParent<Boss>();
        }

        damageableList.Clear();

        attackDamage = (boss == null || attackDamage > 0f) ? attackDamage : boss.bossData.bossAD;
        // 공격의 cc타입이 포효이면 데미지가 0 아니면 보스의 데미지
        attackDamage = ccType == CrowdControlType.Roar ? 0 : attackDamage;
    }

    private void Start()
    {
        OffLowHitBoxes();
    }

    private void OnEnable()
    {
        if(type == BossHitboxDamageType.Continous)
            continousCheckDamageableCo = StartCoroutine(continousCheckDamageable());
    }

    private IEnumerator continousCheckDamageable()
    {
        while (true)
        {
            IDamageable[] keys = new IDamageable[4];
            int count = 0;
            int index = 0;

            foreach (var item in continousDamageableDic)
            {
                float takeDamageTime = item.Value;

                if (Time.time - takeDamageTime >= takeDamageInterval)
                {
                    keys[index++] = item.Key;
                    count++;
                }
            }

            for (int i = 0; i < count; i++)
            {
                continousDamageableDic.Remove(keys[i]);
            }
            yield return null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Transform player = null;
        if(other.CompareTag("Player"))
        {
           player = other.transform.parent;
        }

        if (type == BossHitboxDamageType.OneShot)
            AttackOneShot(player, other);
    }

    private void OnTriggerStay(Collider other)
    {
        Transform player = null;
        if (other.CompareTag("Player"))
        {
            player = other.transform.parent;
        }

        if (type == BossHitboxDamageType.Continous)
        {
            Debug.Log("Player OnTriggerStay");
            AttackContinous(player, other);
        }
    }

    public void AttackOneShot(Transform player, Collider other)
    {
        if (player != null && player.TryGetInterfaceInParent(out IDamageable damagable))
        {
            if (!CheckDamageableList(damagable))
            {
                DamageInfo damageInfo = new DamageInfo()
                {
                    attacker = boss.gameObject,
                    receiver = other.gameObject,
                    damageAmount = attackDamage,
                    ccType = this.ccType,
                    ccEnableTime = this.ccEnableTime
                };

                damagable.TakeDamage(damageInfo);

                damageableList.Add(damagable);
            }
        }
    }

    public void AttackContinous(Transform player, Collider other)
    {
        if (player != null && player.TryGetInterfaceInParent(out IDamageable damageable))
        {
            if (!CheckContinousDamageableList(damageable))
            {
                DamageInfo damageInfo = new DamageInfo()
                {
                    attacker = boss.gameObject,
                    receiver = other.gameObject,
                    damageAmount = attackDamage,
                    ccType = this.ccType,
                    ccEnableTime = this.ccEnableTime
                };

                damageable.TakeDamage(damageInfo);

                continousDamageableDic.Add(damageable, Time.time);
            }
        }
    }

    private void OnDisable()
    {
        damageableList.Clear();
        if (continousCheckDamageableCo != null) StopCoroutine(continousCheckDamageableCo);
    }

    public void OnLowHitBoxes()
    {
        for (int i = 0; i < lowHitboxList.Count; i++)
        {
            lowHitboxList[i].gameObject.SetActive(true);
        }
    }

    public void OffLowHitBoxes()
    {
        for (int i = 0; i < lowHitboxList.Count; i++)
        {
            lowHitboxList[i].gameObject.SetActive(false);
        }
    }
}
