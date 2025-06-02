using System.Collections.Generic;
using UnityEngine;
using ProjectRaid.Data;
using ProjectRaid.Extensions;
using ProjectRaid.EditorTools;
using System.Linq;

public class AttackComponent : MonoBehaviour, IDamageSource
{
    [FoldoutGroup("기본 설정", ExtendedColor.White)]
    [SerializeField] private ItemData data;
    [SerializeField] private Collider hitbox;
    [SerializeField] private GameObject mesh;
    [SerializeField] private Transform iKTarget;
    [SerializeField] private float motionValue = 1f;

    [FoldoutGroup("투사체 설정 (Rifle only)", ExtendedColor.GreenYellow)]
    [SerializeField] private BulletProjectile projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private LayerMask aimLayerMask;
    
    // private bool hasAttackBossOnce = false;
    private float baseMotionValue = 1f;
    private HitRegistry hitRegistry;
    private WeaponData weaponData;
    private PlayerController player;

    public Transform IKTarget => iKTarget;

    void Awake()
    {
        if (!TryGetComponent(out hitRegistry))
            hitRegistry = gameObject.AddComponent<HitRegistry>();

        player = GetComponentInParent<PlayerController>();

        weaponData = GetWeaponData();
        baseMotionValue = motionValue;
    }

    public Collider GetCollider() => hitbox;
    public ItemData GetItemData() => data;
    public WeaponData GetWeaponData() => (WeaponData)data.Equipment;
    public List<Transform> GetTransforms()
    {
        List<Transform> transforms = new();

        if (hitbox != null) transforms.Add(hitbox.transform);
        if (mesh != null) transforms.Add(mesh.transform);

        return transforms;
    }

    public void SetMotionValue(float value = 1f) => motionValue = value;
    public void SetHitbox(bool on) => hitbox.enabled = on;
    public void SetMesh(bool on) => mesh.GetComponent<Renderer>().enabled = on;
    public void Fire(float motion = 1f)
    {
        Vector3 direction = GetCameraRayDirection();
        Quaternion rotation = Quaternion.LookRotation(direction);

        BulletProjectile bullet = Instantiate(projectilePrefab, firePoint.position, rotation);

        (float damage, bool isCritical) = DamageCalculator.CalculateWeaponDamage(weaponData, motion);
        float cutDamage = weaponData.CutValue;
        float destDamage = weaponData.DestructionValue;
        DamageInfo info = new(damage, cutDamage, destDamage, isCritical, bullet.gameObject, null);
        bullet.Setup(info);
    }

    private Vector3 GetCameraRayDirection()
    {
        Ray ray = Camera.main.ScreenPointToRay(new(Screen.width / 2, Screen.height / 2));
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, aimLayerMask))
        {
            return (hit.point - firePoint.position).normalized;
        }
        else
        {
            return ray.direction;
        }
    }

    public void OnTriggerEnterHook(Collider other)
    {
        if (other.CompareTag("Player")) return;
        if (other.CompareTag("NPC")) return;
        if (!other.TryGetInterfaceInParent(out IDamageable hitbox)) return;

        var monster = other.GetComponentInParent<Monster>();
        var boss = other.GetComponentInParent<Boss>();

        (GameObject root, int id) = monster ? (monster.gameObject, monster.monsterData.monsterID)
            : boss ? (boss.gameObject, boss.bossData.bossID)
                : (other.gameObject, -1);

        if (hitRegistry == null) return;
        if (hitRegistry.HasHit(root)) return;
        hitRegistry.Register(root);

        var Info = ApplyDamage(hitbox, root);

        var questID = QuestManager.Instance.PlayerQuestDataManager.TrackedQuestID;

        if (questID == 0) questID = QuestManager.Instance.PlayerQuestDataManager.QuestData.activeQuests.Count != 0 ? QuestManager.Instance.PlayerQuestDataManager.QuestData.activeQuests.Keys.Max() : 0;
        if (questID == 0) questID = -1;

        if (boss != null)
        {
            string currentPattern = (((boss.stateMachine.currentState as MainState)
                ?.currentSubState as SubState)
                    ?.currentPattern as AttackPattern)
                        ?.GetType().Name ?? "None";

            AnalyticsManager.TryAttackEvent(
                questID,
                player.WeaponManager.CurrentData.ID,
                (int)player.WeaponManager.CurrentData.Class,
                player.ComboIndex,
                player.transform.position.x,
                player.transform.position.y,
                id,
                root.transform.position.x,
                root.transform.position.y,
                currentPattern,
                Info.damageAmount
            );
        }
        else
        {
            AnalyticsManager.TryAttackEvent(
                questID,
                player.WeaponManager.CurrentData.ID,
                (int)player.WeaponManager.CurrentData.Class,
                player.ComboIndex,
                player.transform.position.x,
                player.transform.position.y,
                id,
                root.transform.position.x,
                root.transform.position.y,
                "None",
                Info.damageAmount
            );
        }
    }

    private DamageInfo ApplyDamage(IDamageable hitbox, GameObject receiver)
    {
        (float dmg, bool crit) = DamageCalculator.CalculateWeaponDamage(weaponData, motionValue);

        DamageInfo info = new
        (
            damageAmount:   dmg,
            cutDamage:      weaponData.CutValue,
            destDamage:     weaponData.DestructionValue,
            isCritical:     crit,
            attacker:       GetComponentInParent<PlayerController>().gameObject,
            receiver:       receiver
        );

        hitbox.TakeDamage(info);
        GameManager.Instance.DamagePopup.ShowPopup(info.damageAmount, info.isCritical, transform.position);

        motionValue = baseMotionValue;

        return info;
    }

    // 공격이 끝날 때 또는 다시 시작할 때 호출
    public void ResetHitRegistry() => hitRegistry.ClearHits();
}
