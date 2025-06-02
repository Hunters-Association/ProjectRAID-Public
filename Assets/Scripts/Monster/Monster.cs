using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

// 모든 몬스터의 기반이 되는 추상 클래스

[RequireComponent(typeof(Animator), typeof(Collider))]
public abstract class Monster : MonoBehaviour, IDamageable
{
    [Header("필수 참조")]
    public MonsterData monsterData; // ★ 이제 공격 세부 정보는 여기의 availableAttacks 리스트를 통해 참조 ★
    public Animator animator;
    public Collider monsterCollider;
    [HideInInspector] public NavMeshAgent agent;

    [Header("몬스터 상태")]
    [SerializeField] protected MonsterState currentStateEnum;
    public MonsterState CurrentStateEnum => currentStateEnum;
    protected MonsterBaseState currentStateObject;
    [SerializeField] protected int currentHp;
    public int CurrentHp => currentHp;
    [SerializeField] protected bool isInvulnerable = false;
    public bool IsInvulnerable => isInvulnerable;
    [field: SerializeField]
    [Tooltip("저체력으로 스폰 지점 복귀 후 온순해진 상태")]
    public bool isPacified { get; protected set; } = false;
    public bool IsPacified => isPacified;

    [Header("시각/상호작용")]
    [Tooltip("몬스터의 시각적 표현 루트 (숨김/표시용)")]
    //[SerializeField] protected GameObject visualRoot;
    [SerializeField] protected bool canBeGathered = false;
    public bool CanBeGathered => canBeGathered;
    [Tooltip("갈무리 시 상호작용 텍스트")]
    public string gatherInteractionText = "갈무리하기";

    [Header("AI 및 타겟팅")]
    protected Dictionary<GameObject, float> attackers = new Dictionary<GameObject, float>();
    protected float attackerTimeoutDuration = 30f;
    [SerializeField] protected Transform currentTarget;
    public Transform CurrentTarget => currentTarget; // 외부 읽기용 프로퍼티 추가
    
    [Header("스폰 및 관리")]
    protected MonsterManager manager;
    public Vector3 spawnPosition { get; protected set; }
    protected Dictionary<MonsterState, MonsterBaseState> stateCache = new Dictionary<MonsterState, MonsterBaseState>();
    [Header("사체 설정")]
    [SerializeField] private float corpsePlayerCheckRadius = 15f;
    [SerializeField] private float corpseLingerDurationWithoutPlayer = 20f;
    public float CorpseLingerDurationWithoutPlayer => corpseLingerDurationWithoutPlayer;

    // 상태 전환용 임시 데이터 (더 나은 방법으로 개선 가능)
    [HideInInspector] public StateContext nextStateContext;
    protected float lastActivationTime = -Mathf.Infinity;
    // ★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★

    // --- 프로퍼티 ---
    // ★★★ 추가/확인: 외부 읽기용 프로퍼티 ★★★
    public virtual float LastActivationTime => lastActivationTime;

    // 기타 내부 변수
    private float accumulatedHealAmount = 0f;
    private Dictionary<string, Transform> namedTransformsCache = new Dictionary<string, Transform>(); // 스폰 포인트 등 캐싱용

    // --- 기본 능력치 프로퍼티 (MonsterData에서 직접 가져옴) ---
    public virtual float DetectionRange => monsterData?.detectionRange ?? 10f;
    public virtual float MoveSpeed => monsterData?.moveSpeed ?? 5f;
    public virtual float WanderRadius => monsterData?.WanderRadius ?? 5f;
    public virtual float EngagementDistance => monsterData?.engagementDistance ?? 3.0f;

    // --- 특수 행동 프로퍼티 (땅파기 등, MonsterData에서 직접 가져옴) ---
    public virtual float BurrowDuration => monsterData?.burrowDuration ?? 1.5f;
    public virtual float EmergeDuration => monsterData?.emergeDuration ?? 0f;
    public virtual float MinTimeBurrowed => monsterData?.minTimeBurrowed ?? 3.0f;
    public virtual float MaxTimeBurrowed => monsterData?.maxTimeBurrowed ?? 8.0f;
    public virtual float EmergeNearPlayerDistance => monsterData?.emergeNearPlayerDistance ?? 5.0f;
    public virtual float BurrowAllowedSpawnRadius => monsterData?.burrowAllowedSpawnRadius ?? 10f;
    public virtual float BehaviorRadius => monsterData?.behaviorRadius ?? 30f;

    // --- 초기화 ---
    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        monsterCollider = GetComponent<Collider>();
        if (agent != null)
        {
            agent.updateRotation = false; // ★★★ Agent의 자동 회전 기능 끄기 ★★★
        }
        agent = GetComponent<NavMeshAgent>(); // Awake에서 참조
        spawnPosition = transform.position;
        // Debug.Log($"<color=lime>[{gameObject.name}] Awake: spawnPosition 초기화됨: {spawnPosition}</color>");

        // 스폰 포인트 등 자주 찾을 Transform 미리 캐싱 (선택적 최적화)
        CacheNamedTransforms();
        // Debug.Log($"<color=cyan>[{gameObject.name}] Awake: 초기 스폰 위치 설정됨: {spawnPosition} (Instance ID: {gameObject.GetInstanceID()})</color>");
    }
    public Vector3 GetSpawnPosition()
    {
        return spawnPosition; // Awake에서 설정된 값을 반환
    }
    public virtual void Setup(MonsterManager ownerManager, Vector3 initialSpawnPosition)
    {
        this.manager = ownerManager;        
        // Debug.Log($"<color=green>[{gameObject.name}] Setup 완료. 설정된 spawnPosition: {this.spawnPosition}</color>");
    }

    /// <summary>
    /// 자식 트랜스폼 중 이름이 있는 것들을 캐싱합니다. (예: 스폰 포인트)
    /// </summary>
    protected virtual void CacheNamedTransforms()
    {
        namedTransformsCache.Clear();
        foreach (Transform child in transform.GetComponentsInChildren<Transform>(true)) // 비활성 자식 포함
        {
            if (!string.IsNullOrEmpty(child.name) && !namedTransformsCache.ContainsKey(child.name))
            {
                namedTransformsCache.Add(child.name, child);
            }
        }
        // ProjectileSpawnPoint, WebSpawnPoint 등이 Inspector에 할당되었다면 그것도 캐시에 추가
        // if (ProjectileSpawnPoint != null && !namedTransformsCache.ContainsKey(ProjectileSpawnPoint.name))
        //     namedTransformsCache.Add(ProjectileSpawnPoint.name, ProjectileSpawnPoint);
        // ... WebSpawnPoint ...
    }

    public virtual void Setup(MonsterManager ownerManager)
    {
        this.manager = ownerManager;
    }

    public virtual void InitializeMonster()
    {
        // Debug.Log($"<color=yellow>[{gameObject.name}] InitializeMonster 시작 시 spawnPosition: {spawnPosition} (Instance ID: {gameObject.GetInstanceID()})</color>");
        if (monsterData == null)
        {
            Debug.LogError($"[{gameObject.name}] MonsterData가 할당되지 않았습니다!", this);
            gameObject.SetActive(false);
            return;
        }
        currentHp = monsterData.maxHp;
        accumulatedHealAmount = 0f;
        attackers.Clear();
        currentTarget = null;
        canBeGathered = false;
        isPacified = false;
        SetInvulnerable(false);
        //ShowVisuals(); // 시각적 요소 보이기
        monsterCollider.enabled = true; // 콜라이더 활성화

        // 상태 캐시 초기화 (선택적)
        // stateCache.Clear();
        lastActivationTime = Time.time;
        transform.position = spawnPosition;
        transform.rotation = Quaternion.identity;
        gameObject.SetActive(true);

        if (agent != null)
        {
            if (!agent.enabled) agent.enabled = true;

            // ★★★ Warp 시 Awake에서 설정된 spawnPosition 사용 ★★★
            if (NavMesh.SamplePosition(spawnPosition, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
            {
                // NavMesh 상태와 관계없이 Warp 시도 (SamplePosition 성공 시)
                if (!agent.Warp(hit.position)) // SamplePosition 결과로 워프 시도
                {
                    Debug.LogError($"[{gameObject.name}] Agent.Warp 실패! 목표 위치: {hit.position}. Agent 비활성화 시도.");
                    // Warp 실패 시 Agent 비활성화 또는 다른 처리
                    // agent.enabled = false;
                }
                // else { Debug.Log($"[{gameObject.name}] Agent 워프 성공: {hit.position}"); }
            }
            else
            {
                Debug.LogError($"[{gameObject.name}] 스폰 위치 {spawnPosition} 근처에서 NavMesh 위치를 찾을 수 없습니다. Agent 비활성화.");
                agent.enabled = false;
            }

            if (agent.enabled) agent.isStopped = true;
        }        
        animator.Rebind();
        ChangeState(MonsterState.Idle);
    }

    public virtual void ResetMonster()
    {
        // InitializeMonster가 대부분의 리셋 로직 수행
        InitializeMonster();
    }

    // --- 상태 전환 (StateContext 사용) ---
    public virtual void ChangeState(MonsterState newStateEnum, StateContext context = null)
    {
        if (currentStateEnum == MonsterState.Dead && newStateEnum != MonsterState.Idle) return;

        // Debug.Log($"[{gameObject.name}] 상태 변경: {currentStateEnum} -> {newStateEnum}");
        currentStateObject?.ExitState();
        currentStateObject = GetStateInstance(newStateEnum, context); // ★ Context 전달
        currentStateEnum = newStateEnum;
        if (currentStateObject != null) currentStateObject.EnterState();
        else Debug.LogError($"[{gameObject.name}] 상태({newStateEnum}) 인스턴스 가져오기 실패!");
    }

    protected virtual MonsterBaseState GetStateInstance(MonsterState stateEnum, StateContext context)
    {
        // Emerging 상태는 항상 새로 생성 (Morven 등 자식에서 override 필요)
        if (stateEnum == MonsterState.Emerging)
        {
            // 자식 클래스에서 처리하도록 null 반환 또는 기본값 제공
            return null; // 또는 다른 자식 클래스 확인 로직
        }
        // ★★★ ReturnToSpawn 상태 객체 생성 로직 추가/확인 ★★★
        if (stateEnum == MonsterState.ReturnToSpawn)
        {
            // 캐시를 사용하지 않고 항상 새로 생성하는 것이 안전할 수 있음
            // (EnterState에서 목표 지점을 다시 설정해야 하므로)
            return new MonsterReturnToSpawnState(this);
        }
        // ★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★

        // 다른 상태들은 캐시 확인
        if (stateCache.TryGetValue(stateEnum, out var cachedState))
        {
            return cachedState;
        }

        // 캐시에 없으면 기본 상태 생성
        MonsterBaseState instance = stateEnum switch
        {
            MonsterState.Idle => new MonsterIdleState(this),
            MonsterState.Attack => new MonsterAttackState(this),
            MonsterState.Flee => new MonsterFleeState(this),
            MonsterState.FleeToSpawn => new FleeToSpawnState(this),
            MonsterState.Dead => new MonsterDeadState(this),
            // Burrowing, Burrowed는 Morven 등 자식 클래스에서 override하여 처리
            // Emerging, ReturnToSpawn은 위에서 처리됨
            _ => null // 처리되지 않은 다른 상태
        };

        if (instance != null)
        {
            stateCache[stateEnum] = instance;
        }
        else if (stateEnum != MonsterState.Burrowing && stateEnum != MonsterState.Burrowed && stateEnum != MonsterState.Emerging) // 자식 클래스 처리 상태 제외
        {
            // ReturnToSpawn이 아닌 다른 상태 생성 실패 시 로그
            Debug.LogError($"[{gameObject.name}] 기본 상태({stateEnum}) 인스턴스 생성 실패! 스위치 표현식 확인 필요.");
        }

        return instance;
    }

    // --- 업데이트 ---
    protected virtual void Update()
    {
        // --- 행동 반경 체크 ---
        // 죽었거나, 이미 복귀 중이거나, 스폰지점으로 도망 중이 아닐 때만 체크
        if (currentStateEnum != MonsterState.Dead &&
            currentStateEnum != MonsterState.ReturnToSpawn &&
            currentStateEnum != MonsterState.FleeToSpawn &&
            IsOutsideBehaviorRange()) // ★ 범위 벗어났는지 체크 ★
        {
            // Debug.Log($"[{gameObject.name}] 행동 반경(반경: {BehaviorRadius}) 벗어남! 현재 거리 제곱: {(transform.position - spawnPosition).sqrMagnitude:F1}. 복귀 시작.");
            ChangeState(MonsterState.ReturnToSpawn); // ★ 복귀 상태로 강제 전환 ★
            return; // 상태 변경 후 즉시 Update 종료 (새 상태는 다음 프레임부터 Update)
        }
        // --- 행동 반경 체크 끝 ---


        if (currentStateObject == null && currentStateEnum != MonsterState.Dead)
        {
            Debug.LogWarning($"[{gameObject.name}] 현재 상태 객체가 null! (상태: {currentStateEnum}). Idle로 강제 전환 시도.");
            ChangeState(MonsterState.Idle);
            return; // 상태 변경 후 업데이트 건너뛰기
        }

        currentStateObject?.UpdateState(); // 현재 상태 업데이트
        UpdateAnimatorParameters();
        CleanupAttackers();
    }
    protected virtual void UpdateAnimatorParameters()
    {
        if (animator == null) // Animator가 아예 없는 경우 먼저 체크
        {
            return;
        }

        // ▼▼▼ 파라미터 존재 여부 확인 로직 (선택적 방어 코드) ▼▼▼
        bool hasXVelocityParam = false;
        bool hasYVelocityParam = false;

        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == "xVelocity" && param.type == AnimatorControllerParameterType.Float)
            {
                hasXVelocityParam = true;
            }
            if (param.name == "yVelocity" && param.type == AnimatorControllerParameterType.Float)
            {
                hasYVelocityParam = true;
            }
        }
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
        {
            if (hasXVelocityParam) // 파라미터가 있을 때만 설정
                animator.SetFloat("xVelocity", 0f, 0.1f, Time.deltaTime);
            if (hasYVelocityParam) // 파라미터가 있을 때만 설정
                animator.SetFloat("yVelocity", 0f, 0.1f, Time.deltaTime);
            return;
        }

        Vector3 worldVelocity = agent.velocity;
        Vector3 localVelocity = transform.InverseTransformDirection(worldVelocity);

        float currentMaxMoveSpeed = monsterData?.moveSpeed ?? agent.speed;
        if (currentMaxMoveSpeed < 0.01f) currentMaxMoveSpeed = 0.01f;

        float targetXVelocity = localVelocity.x / currentMaxMoveSpeed;
        float targetYVelocity = localVelocity.z / currentMaxMoveSpeed;

        targetXVelocity = Mathf.Clamp(targetXVelocity, -1.0f, 1.0f);
        targetYVelocity = Mathf.Clamp(targetYVelocity, -1.0f, 1.0f);

        // ▼▼▼ SetFloat 호출 전 파라미터 존재 여부 확인 ▼▼▼
        if (hasXVelocityParam)
        {
            animator.SetFloat("xVelocity", targetXVelocity, 0.1f, Time.deltaTime);
        }
        // else Debug.LogWarning($"[{gameObject.name}] Animator에 'xVelocity' Float 파라미터가 없습니다."); // 필요시 경고

        if (hasYVelocityParam)
        {
            animator.SetFloat("yVelocity", targetYVelocity, 0.1f, Time.deltaTime);
        }
        // else Debug.LogWarning($"[{gameObject.name}] Animator에 'yVelocity' Float 파라미터가 없습니다."); // 필요시 경고
        // ▲▲▲ SetFloat 호출 전 파라미터 존재 여부 확인 끝 ▲▲▲
    }


    /// <summary>
    /// 몬스터가 현재 달려야 하는 상태인지 판단하는 예시 함수입니다.
    /// 실제 조건은 게임 로직에 따라 달라집니다.
    /// </summary>


    // --- 피격 처리 ---
    public virtual void TakeDamage(DamageInfo info)
    {
        if (isInvulnerable || currentStateEnum == MonsterState.Dead || info.damageAmount <= 0) return;

        PlayHitSound();

        int damageToApply = Mathf.FloorToInt(info.damageAmount);
        // TODO: 방어력, 속성 저항 등 계산

        currentHp -= damageToApply;
        // Debug.Log(...);

        if (currentHp <= 0)
        {
            currentHp = 0;
            ChangeState(MonsterState.Dead);
            animator.SetTrigger("Death_01");
        }
        else
        {
            if (info.attacker != null)
            {
                CheckAttacker(info.attacker);
                EvaluateNewAttacker(info.attacker);
            }
            currentStateObject?.OnTakeDamage(info); // 현재 상태에 피격 알림
            animator.SetTrigger("IsHit");
            animator.SetTrigger("Knockback");            
        }        
    }
    public virtual bool ProcessDropTable(GameObject gatherer = null)
    {
        Debug.Log($"[{gameObject.name}] Processing drop table...");
        if (monsterData == null || monsterData.dropTable == null || monsterData.dropTable.Count == 0)
        {
            Debug.LogWarning($"[{gameObject.name}] No drop table data found.");
            return false;
        }

        // 아이템 매니저/데이터베이스 참조 (예시)
        // ItemManager itemManager = ItemManager.Instance;
        // PlayerInventory inventory = gatherer?.GetComponent<PlayerInventory>();

        foreach (DropItemInfo dropInfo in monsterData.dropTable)
        {
            if (Random.Range(0f, 100f) <= dropInfo.dropChance)
            {
                int id = dropInfo.itemID;
                int amount = Random.Range(dropInfo.minQuantity, dropInfo.maxQuantity + 1);

                if (id != 0 && amount > 0)
                {
                    // 실제 아이템 이름 가져오기 (예시)
                    // string itemName = itemManager?.GetItemName(itemID) ?? $"Item ID [{itemID}]";
                    string itemName = $"Item ID [{id}]"; // 임시 이름
                    Debug.Log($"Dropping item: {itemName} x {amount}");

                    // 실제 인벤토리 추가 또는 아이템 생성 로직
                    bool success = GameManager.Instance.Inventory.TryAdd(id);
                    if (success) AnalyticsManager.TryGathering(monsterData.monsterID, id, amount);

                    return success;
                    
                    // if (inventory != null) inventory.AddItem(itemID, amount);
                    // else { /* 월드에 아이템 생성 */ }
                }
            }
        }

        return false;
    }

    // --- 시각/무적 제어 ---
    public virtual void SetInvulnerable(bool value) => isInvulnerable = value;
    public virtual void ShowVisuals() { }// => visualRoot?.SetActive(true);
    public virtual void HideVisuals() { }// => visualRoot?.SetActive(false);

    // --- AI 및 이동 헬퍼 ---
    public Transform GetCurrentTarget() => currentTarget;
    public void ClearTarget() => currentTarget = null;
    public virtual bool DetectPlayer()
    {
        if (isPacified) return false;
        // Player 레이어 마스크 캐싱하면 더 좋음
        int playerLayer = LayerMask.NameToLayer("Player");
        if (playerLayer == -1) { Debug.LogWarning("Player 레이어가 정의되지 않았습니다."); return false; }
        LayerMask playerMask = 1 << playerLayer;

        Collider[] hits = Physics.OverlapSphere(transform.position, DetectionRange, playerMask);
        if (hits.Length > 0)
        {
            // 가장 가까운 플레이어 또는 첫 번째 감지된 플레이어
            EvaluateNewAttacker(hits[0].gameObject);
            return true;
        }
        // 타겟이 감지 범위 밖에 나갔다면 ClearTarget (선택적)
        // if (currentTarget != null && !IsTargetInRange(currentTarget, DetectionRange * 1.1f)) ClearTarget();

        return false;
    }
    public virtual bool IsTargetInRange(Transform target, float range)
    {
        return target != null && Vector3.Distance(transform.position, target.position) <= range;
    }
    public virtual void EvaluateNewAttacker(GameObject newAttacker)
    {
        // 도망 중이 아닐 때만 타겟 설정
        if (newAttacker != null && currentStateEnum != MonsterState.Flee && currentStateEnum != MonsterState.FleeToSpawn)
        {
            currentTarget = newAttacker.transform;
        }
    }
    public virtual void StartMovement(Vector3 destination)
    {
        if (agent?.enabled == true && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.SetDestination(destination);
        }
    }
    public virtual void StopMovement()
    {
        if (agent?.enabled == true && agent.isOnNavMesh && !agent.isStopped)
        {
            agent.isStopped = true;
            // 선택적: 경로 즉시 취소
            // agent.ResetPath();
        }
    }
    public virtual bool HasReachedDestination()
    {
        if (agent == null || !agent.enabled || !agent.isOnNavMesh || agent.pathPending) return false;
        // 도착 거리에 충분히 가깝고, 속도가 거의 0일 때
        return agent.remainingDistance <= agent.stoppingDistance + 0.1f && agent.velocity.sqrMagnitude < 0.1f;
    }
    public virtual bool GetWanderPositionNavMesh(float radius, out Vector3 result)
    {
        // 스폰 위치 기준 배회
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * radius;
        randomDirection += spawnPosition; // 스폰 위치 중심
        randomDirection.y = transform.position.y; // 현재 높이 유지 시도

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, radius * 1.5f, NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }
        else
        {
            result = transform.position; // 실패 시 현재 위치 반환
            return false;
        }
    }

    // ==============================================================
    // ★★★ 공격 실행 헬퍼 메서드 ★★★
    // ==============================================================

    /// <summary>
    /// 캐싱된 자식 트랜스폼 중 지정된 이름의 트랜스폼을 찾습니다. 없으면 기본 transform 반환.
    /// </summary>
    public virtual Transform FindSpawnPoint(string name)
    {
        if (!string.IsNullOrEmpty(name) && namedTransformsCache.TryGetValue(name, out Transform point))
        {
            return point;
        }
        // Debug.LogWarning($"[{gameObject.name}] 스폰 포인트 '{name}'을(를) 찾을 수 없습니다. 기본 위치 사용.");
        // ProjectileSpawnPoint 등 Inspector 할당 변수도 확인 가능
        // if (name == "ProjectileSpawnPoint" && ProjectileSpawnPoint != null) return ProjectileSpawnPoint;

        return transform; // 기본값: 몬스터 자신의 위치
    }

    public void FireProjectileEventByType(int attackTypeEnumValue)
    {
        if (monsterData == null)
        {
            Debug.LogWarning($"[{gameObject.name}] MonsterData가 없습니다.");
            return;
        }
        Transform currentEventTarget = GetCurrentTarget(); // 현재 타겟 사용

        ProjectileAttackType typeToFire = (ProjectileAttackType)attackTypeEnumValue;
        ProjectileAttackData attackDataToUse = FindProjectileAttackDataByType(typeToFire);

        if (attackDataToUse != null)
        {
            SpawnProjectileFromData(attackDataToUse, currentEventTarget);
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] '{typeToFire}' 타입의 ProjectileAttackData를 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// 애니메이션 이벤트에서 호출되어 근접 공격 데미지를 적용합니다.
    /// 이벤트의 int 파라미터로 데미지 양을 전달받습니다.
    /// </summary>
    public void ApplyMeleeDamageFromAnimationEvent(int damageAmount)
    {
        if (damageAmount <= 0) return;
        DealDamageToTarget(damageAmount); // 기존 DealDamageToTarget 함수 사용
    }

    /// <summary>
    /// 애니메이션 이벤트에서 호출되어 특정 능력을 실행합니다.
    /// 이벤트의 string 파라미터로 능력 식별자를 전달받습니다.
    /// </summary>
    public void ExecuteAbilityFromAnimationEvent(string abilityIdentifier)
    {
        if (string.IsNullOrEmpty(abilityIdentifier))
        {
            Debug.LogWarning($"[{gameObject.name}] ExecuteAbilityFromAnimationEvent: abilityIdentifier가 비어있습니다.");
            return;
        }
        ExecuteAbility(abilityIdentifier); // 기존 ExecuteAbility 함수 사용
    }

    /// <summary>
    /// ProjectileAttackData 정보를 사용하여 투사체를 생성하고 발사합니다.
    /// </summary>
    public virtual void SpawnProjectileFromData(ProjectileAttackData data, Transform target)
    {
        if (data == null || data.projectilePrefab == null)
        {
            Debug.LogError($"[{gameObject.name}] 유효하지 않은 ProjectileAttackData 또는 프리팹!", this);
            return;
        }

        // 1. 스폰 위치/방향 결정
        Transform spawnPoint = FindSpawnPoint(data.spawnPointName); // 이름으로 스폰 포인트 찾기
        Vector3 spawnPos = spawnPoint.position;
        Quaternion spawnRot = transform.rotation; // 기본 회전
        if (target != null) // 타겟이 있으면 타겟 방향으로
        {
            Vector3 direction = (target.position - spawnPos).normalized;
            direction.y = 0; // 필요시 수평 조준
            if (direction != Vector3.zero) spawnRot = Quaternion.LookRotation(direction);
        }

        // TODO: 오브젝트 풀러 사용하도록 수정 필요
        // GameObject projectileGO = ObjectPooler.Instance.SpawnFromPool(data.projectilePrefab.name, spawnPos, spawnRot);
        GameObject projectileGO = Instantiate(data.projectilePrefab, spawnPos, spawnRot); // 임시 Instantiate

        if (projectileGO == null) { Debug.LogError("투사체 생성 실패!", this); return; }

        // 2. 투사체 컴포넌트 가져오기 및 초기화 (다양한 투사체 타입 고려)
        //    (Projectile, PoisonProjectile, WebProjectile 등)
        //    -> 투사체 스크립트들이 공통 인터페이스(예: IProjectile)를 구현하면 더 좋음
        if (projectileGO.TryGetComponent<Projectile>(out var proj)) // Projectile.cs (돌멩이 등)
        {
            int playerLayer = LayerMask.NameToLayer("Hurtbox");
            LayerMask playerMask = (playerLayer != -1) ? 1 << playerLayer : 0;
            proj.Initialize(this.gameObject, data.damage, data.projectileSpeed, playerMask);
        }
        else if (projectileGO.TryGetComponent<PoisonProjectile>(out var poisonProj)) // PoisonProjectile.cs (독침)
        {
            int playerLayer = LayerMask.NameToLayer("Hurtbox");
            LayerMask playerMask = (playerLayer != -1) ? 1 << playerLayer : 0;
            // 독침은 기본 공격력(data.damage)을 사용할지, 아니면 MonsterData의 다른 값을 쓸지 결정 필요
            poisonProj.Initialize(this.gameObject, data.projectileSpeed, playerMask, data.damage, false); // canCrit 임시 false
            //animator.SetTrigger("Throw_Venom_02");
        }
        else if (projectileGO.TryGetComponent<WebProjectile>(out var webProj)) // WebProjectile.cs (거미줄)
        {
            int playerLayer = LayerMask.NameToLayer("Hurtbox");
            LayerMask playerMask = (playerLayer != -1) ? 1 << playerLayer : 0;
            webProj.Initialize(this.gameObject, data.projectileSpeed, playerMask, data.damage); // 거미줄 데미지 사용
            //animator.SetTrigger("Throw_Venom_01");
        }
        else
        {
            Debug.LogWarning($"생성된 투사체 ({projectileGO.name})에 인식 가능한 투사체 스크립트가 없습니다.", projectileGO);
            // 기본 Rigidbody 속도 설정 등 공통 로직 추가 가능
            // Rigidbody rb = projectileGO.GetComponent<Rigidbody>();
            // if (rb != null) rb.velocity = projectileGO.transform.forward * data.projectileSpeed;
        }
    }

    /// <summary>
    /// 현재 타겟에게 근접 데미지를 입힙니다. (MeleeAttackData 사용 시 호출됨)
    /// </summary>
    /// <param name="damageAmount">입힐 데미지 양</param>
    public virtual void DealDamageToTarget(int damageAmount) // ★ 데미지 파라미터 받도록 수정 ★
    {
        if (currentTarget != null && IsTargetInRange(currentTarget, 2.0f)) // ★ 사거리 체크는 AttackData에서 하지만 여기서도 간단히 확인 ★
        {
            if (currentTarget.TryGetComponent<IDamageable>(out var playerDamageable))
            {
                // DamageInfo 생성 시 전달받은 데미지 사용
                DamageInfo damageInfo = new DamageInfo(
                    damageAmount,       // ★ 전달받은 데미지
                    0f, 0f, false,      // 나머지 데미지 정보 기본값
                    this.gameObject,    // 공격자
                    currentTarget.gameObject // 피격자
                );
                playerDamageable.TakeDamage(damageInfo);
            }
        }
    }

    /// <summary>
    /// AbilityAttackData에 명시된 식별자에 해당하는 능력을 실행합니다. (상태 클래스에서 호출)
    /// </summary>
    public virtual void ExecuteAbility(string abilityIdentifier)
    {
        // Debug.Log($"[{gameObject.name}] 능력 실행 시도: {abilityIdentifier}");
        switch (abilityIdentifier)
        {
            case "Burrow":
                // 땅파기 상태로 전환
                if (currentStateEnum != MonsterState.Burrowing && currentStateEnum != MonsterState.Burrowed)
                {
                    ChangeState(MonsterState.Burrowing);
                }
                break;
            case "Charge":
                // TODO: 돌진 상태(ChargeState) 또는 관련 로직 호출
                Debug.LogWarning("돌진 능력 미구현");
                break;
            // 다른 능력들 추가
            default:
                Debug.LogWarning($"[{gameObject.name}] 알 수 없는 능력 식별자: {abilityIdentifier}");
                break;
        }
    }
    protected virtual ProjectileAttackData FindProjectileAttackDataByType(ProjectileAttackType type)
    {
        if (monsterData == null || monsterData.availableAttacks == null)
        {
            // Debug.LogWarning($"[{gameObject.name}] FindProjectileAttackDataByType: MonsterData 또는 availableAttacks가 null입니다.");
            return null;
        }

        foreach (AttackData attackBaseData in monsterData.availableAttacks) // AttackData로 변경 (MonsterAttackBaseData가 AttackData를 상속한다고 가정)
        {
            // ProjectileAttackData 타입이고, attackType이 일치하는지 확인
            if (attackBaseData is ProjectileAttackData projData && projData.attackType == type)
            {
                return projData;
            }
        }
        // Debug.LogWarning($"[{gameObject.name}] FindProjectileAttackDataByType: '{type}' 타입의 ProjectileAttackData를 찾지 못했습니다.");
        return null; // 찾지 못한 경우
    }

    // ==============================================================

    // --- 기타 헬퍼 ---
    public virtual void LookAtTarget(Transform target)
    {
        if (target != null && agent != null && agent.enabled)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            direction.y = 0;
            if (direction.sqrMagnitude > 0.0001f) // magnitude 대신 sqrMagnitude 사용 (제곱근 계산 불필요)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction.normalized); // 정규화된 벡터 사용
                transform.rotation = targetRotation; // 즉시 회전
                // Debug.Log($"[{gameObject.name}] LookAtTarget: 회전 적용됨 -> {targetRotation.eulerAngles}");
            }
        }
    }
    public virtual void Pacify() { if (!isPacified) isPacified = true; }
    public virtual void ResetPacifiedState() { if (isPacified) isPacified = false; }
    public virtual bool CheckForNearbyPlayers()
    {
        int layer = LayerMask.NameToLayer("Player");
        if (layer == -1) return false;
        return Physics.CheckSphere(transform.position, corpsePlayerCheckRadius, 1 << layer);
    }
    public virtual void ForceDespawnCorpse() { if (gameObject.activeSelf) { gameObject.SetActive(false); manager?.RemoveFromDeadList(this); } }
    public void SetGatherable(bool value) { canBeGathered = value; }
    public void DisableCollider() { if (monsterCollider != null) monsterCollider.enabled = false; }
    public void ClearAttackers() { attackers.Clear(); }
    public void NotifyManagerOfDeath() { manager?.ReportDeath(this); }
    public void PlayHitSound() { /* TODO: 사운드 재생 */ }
    public void NotifyInteractionStart(GameObject interactor) { /* 필요시 구현 */ }
    protected void CheckAttacker(GameObject attacker) { if (attacker != null) attackers[attacker] = Time.time + attackerTimeoutDuration; }
    protected void CleanupAttackers()
    {
        // LINQ 사용 간소화 (GC Alloc 주의)
        var keysToRemove = attackers.Where(pair => Time.time > pair.Value).Select(pair => pair.Key).ToList();
        foreach (var key in keysToRemove) attackers.Remove(key);
    }
    public bool IsDead() => currentStateEnum == MonsterState.Dead;
    public virtual void RemoveFromManagerDeadList() => manager?.RemoveFromDeadList(this);
    public virtual void Heal(float amount)
    {
        if (currentStateEnum == MonsterState.Dead || amount <= 0 || currentHp >= monsterData.maxHp) return;
        accumulatedHealAmount += amount;
        if (accumulatedHealAmount >= 1f)
        {
            int healInt = Mathf.FloorToInt(accumulatedHealAmount);
            currentHp = Mathf.Clamp(currentHp + healInt, 0, monsterData.maxHp);
            accumulatedHealAmount -= healInt;
            // Debug.Log($"[{gameObject.name}] Healed {healInt} HP. Current: {currentHp}/{monsterData.maxHp}");
        }
    }
    public virtual void HandleSuccessfulFlee() { manager?.ReportSuccessfulFlee(this, monsterData.fleeReactivationTime, monsterData.fleeReactivationPointTag); gameObject.SetActive(false); }
    public bool IsOutsideBehaviorRange()
    {
        float radius = BehaviorRadius; // 프로퍼티 사용
        if (radius <= 0) return false; // 반경 0 이하면 체크 안 함
        return (transform.position - spawnPosition).sqrMagnitude > radius * radius;
    }
}