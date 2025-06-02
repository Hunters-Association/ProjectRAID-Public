using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))] // NavMeshAgent 필수
public class Morven : Monster
{
    // Morven 만의 특별한 변수가 필요하면 여기에 추가
    // 예: private float lastBurrowTime;

    protected override void Awake()
    {
        base.Awake();
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.speed = MoveSpeed;
            // ★ CS0103 해결: AttackRange 대신 engagementDistance 사용 ★
            // 또는 기본 근접 공격 데이터를 찾아서 그 범위를 사용할 수도 있지만, engagementDistance가 더 간단합니다.
            if (monsterData != null)
            {
                agent.stoppingDistance = monsterData.engagementDistance * 0.8f; // 교전 거리 기준으로 설정
            }
            else
            {
                agent.stoppingDistance = 1.0f; // MonsterData 없을 경우 기본값
                Debug.LogWarning($"[{gameObject.name}] MonsterData not assigned in Awake. Using default stopping distance.");
            }
            agent.acceleration = 12f;
            agent.angularSpeed = 360f;
        }
        //if (visualRoot == null)
        //{
        //    Transform visualTransform = transform.Find("Visuals");
        //    if (visualTransform != null) visualRoot = visualTransform.gameObject;
        //    else Debug.LogError($"[{gameObject.name}] Visual Root not found or assigned!", this);
        //}
    }
    protected override void Update()
    {
        base.Update();
        float speed  = agent.velocity.magnitude;
        animator.SetFloat("Speed",speed);
    }

    // ★ CS0115 & CS7036 해결: GetStateInstance 시그니처 수정 및 Context 사용 ★
    protected override MonsterBaseState GetStateInstance(MonsterState stateEnum, StateContext context)
    {
        MonsterBaseState instance = null; // 인스턴스 변수 선언

        // --- Morven 전용 상태 처리 ---
        switch (stateEnum)
        {
            case MonsterState.Idle:
                // Morven 전용 Idle 상태 사용 시
                if (stateCache.TryGetValue(stateEnum, out var cachedIdle)) return cachedIdle;
                instance = new MorvenIdleState(this);
                stateCache[stateEnum] = instance;
                return instance; // 즉시 반환

            case MonsterState.Attack:
                // Morven 전용 Attack 상태 사용 시
                if (stateCache.TryGetValue(stateEnum, out var cachedAttack)) return cachedAttack;
                instance = new MorvenAttackState(this);
                stateCache[stateEnum] = instance;
                return instance; // 즉시 반환

            case MonsterState.Burrowing:
                // 캐시 사용 또는 새로 생성
                if (stateCache.TryGetValue(stateEnum, out var cachedBurrowing)) return cachedBurrowing;
                instance = new MorvenBurrowState(this);
                stateCache[stateEnum] = instance;
                return instance; // 즉시 반환

            case MonsterState.Burrowed:
                // 캐시 사용 또는 새로 생성
                if (stateCache.TryGetValue(stateEnum, out var cachedBurrowed)) return cachedBurrowed;
                instance = new MorvenBurrowedState(this);
                stateCache[stateEnum] = instance;
                return instance; // 즉시 반환

            case MonsterState.Emerging:
                // Emerging 상태는 항상 새로 생성 (캐시 사용 안 함)
                Vector3 emergePos = context?.TargetPosition ?? this.transform.position;
                instance = new MorvenEmergeState(this, emergePos);
                // Emerging 상태는 캐시에 저장하지 않을 수 있음 (선택적)
                return instance; // 즉시 반환

            // --- Morven 전용 상태 외에는 부모 클래스에 위임 ---
            default:
                // Debug.Log($"[{gameObject.name}] Morven 전용 상태 아님 ({stateEnum}). 부모 GetStateInstance 호출.");
                // ★★★ 부모 클래스의 GetStateInstance 호출 ★★★
                instance = base.GetStateInstance(stateEnum, context);
                // 부모에서 생성 실패 시 로그는 부모 클래스에서 처리할 것이므로 여기서 추가 로그 불필요
                return instance;
        }

        // 이론상 이 라인에는 도달하지 않음
        // return instance;
    }

    // 필요하다면 다른 메서드들(Initialize, Reset 등)도 Override 가능
    public override void InitializeMonster()
    {
        base.InitializeMonster();
        // Morven 만의 추가 초기화 로직
    }

    public override void ResetMonster()
    {
        base.ResetMonster();
        // Morven 만의 추가 리셋 로직
    }

    public void OnProjectile() 
    {
        (stateCache[MonsterState.Attack] as MorvenAttackState)?.OnProjectileHook();
    }
    public void OnMeleeAttack()
    {
        (stateCache[MonsterState.Attack] as MorvenAttackState)?.OnMeleeAttackHook();
    }
    // TakeDamage 는 부모 클래스에서 isInvulnerable 체크하므로 보통 Override 불필요
}
