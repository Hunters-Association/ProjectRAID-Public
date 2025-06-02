using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ProjectileAttackType
{
    Generic, // 일반적인 투사체, 또는 타입 미지정
    Web,     // 거미줄
    Poison,  // 독침
    // 필요에 따라 다른 투사체 타입을 여기에 추가 (예: Fireball, IceShard 등)
}

[CreateAssetMenu(fileName = "ProjectileAttack_Data", menuName = "Monster/Attack Data/Projectile Attack")]
public class ProjectileAttackData : AttackData
{
    [Header("투사체 특정 속성")]
    [Tooltip("생성할 투사체 프리팹")]
    public GameObject projectilePrefab;

    [Tooltip("투사체가 날아가는 속도")]
    public float projectileSpeed = 15f;

    [Tooltip("기본 데미지 값 (투사체에 전달하거나 직접 사용할 수 있음)")]
    public int damage = 8; // 투사체 자체 데미지와 별개로, 발사 시 데미지 판정 등에 사용 가능

    [Tooltip("(선택 사항) 몬스터 프리팹 내 특정 자식 게임 오브젝트 Transform을 발사 지점으로 사용하려면 해당 이름을 지정합니다.")]
    public string spawnPointName = "ProjectileSpawnPoint"; // Monster.cs 에서 이 이름으로 찾아서 사용

    [Header("애니메이션 이벤트 연동용")]
    [Tooltip("이 투사체 공격의 타입을 지정합니다. 애니메이션 이벤트에서 이 타입으로 공격을 식별합니다.")]
    public ProjectileAttackType attackType = ProjectileAttackType.Generic;
    
}
