using ProjectRaid.EditorTools;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum BossMainState
{
    Init,           // 초기 생성 상태
    NoneCombat,     // 비전투 상태
    Combat,         // 전투 상태
    Destruct,       // 부위 파괴 상태
    Cut,            // 부위 절단 상태
    Retreat,        // 후퇴 상태
    Rest,           // 휴식 상태
    Dead,           // 죽음 상태
    Count
}

public class Boss : MonoBehaviour
{
    //=================== [Component] ===================
    [HideInInspector] public BossStateMachine stateMachine;
    [HideInInspector] public NavMeshAgent navAgent;
    [HideInInspector] public Collider bodyCollider;
    [HideInInspector] public Animator animator;
    [HideInInspector] public BossHealth bossHealth;

    //=================== [Point] ===================
    [HideInInspector] public Transform target;        // 몬스터의 적
    [HideInInspector] public PlayerStatsRuntime targetStat;
    public Transform nest;                            // 몬스터의 둥지

    //=================== [Bool] ===================
    [HideInInspector] public bool isChangeRetreatState = false;          // 휴식 상태로 변환해야 되는가?
    [HideInInspector] public bool isChangePhase = false;                 // 페이즈가 변환이 되어서 포효를 해야되는가?

    //=================== [Data] ===================
    // 보스마다 조정될 수치들
    public BossSO bossData;
    public float detectDistance;    // 적 감지 거리
    [HideInInspector] public float attackDistacne;    // 공격 거리
    [HideInInspector] public readonly float enableTime = 60f;        // 죽고 유지되는 시간

    public float recoveryInterval;  // 휴식시 회복 간격
    public float restTime;          // 최대 휴식 시간

    public MonsterBehaviorType monsterType;

    [HideInInspector] public float startStunTime = 0f;
    [HideInInspector] public float stunTime = 5f;

    //=================== [Phase] ===================
    [FoldoutGroup("페이즈", ExtendedColor.White)]
    [HideInInspector] public List<BossPhaseData> phases;      // 페이즈 별 정보
    public BossPhaseData currentPhase;      // 현재 페이즈
    [HideInInspector] public int currentPhaseIndex;           // 현재 페이즈 인덱스

    //=================== [Hitboxes] ===================
    [FoldoutGroup("페이즈", ExtendedColor.Green)]
    public BossHitbox[] hitBoxes;

    //=================== [State] ===================
    [HideInInspector] public Dictionary<BossMainState, BossBaseState> states = new Dictionary<BossMainState, BossBaseState>();    // 보스들 상태
    [ShowNonSerializedField] public BossBaseState currentState;                  // 디버깅 용

    //=================== [BodyParts] ===================
    [HideInInspector] public BossBodyPartBase[] bossBodyParts;        // 보스 부위 콜라이더들
    [HideInInspector] public List<Collider> bossBodyPartsColliders = new();
    public BossBodyDestructionPart headPart;        // 머리 부위

    //=================== [Interactable] ===================
    [HideInInspector] public BossInteratable interactable;            // 상호 작용할 오브젝트

    private void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
        bodyCollider = GetComponent<Collider>();
        animator = GetComponentInChildren<Animator>();
        bossHealth = GetComponent<BossHealth>();
        interactable = GetComponentInChildren<BossInteratable>(true);

        if (hitBoxes.Length == 0)
            hitBoxes = GetComponentsInChildren<BossHitbox>();

        SetMonsterType();

        bossHealth.OnHit += UpdatePhase;
        bossHealth.InitHP(bossData.bossHP);

        Init();
    }

    public virtual void Init()
    {
    }

    private void SetMonsterType()
    {
        monsterType = (MonsterBehaviorType)bossData.bossType;

        switch (monsterType)
        {
            // 온순한 몬스터이면 데미지를 입었을 때 타겟을 설정
            case MonsterBehaviorType.Passive:
                bossHealth.OnFirstHit += SetTarget;
                break;
            // 선공 가능 또는 선공 몬스터이면
            case MonsterBehaviorType.Territorial:
            case MonsterBehaviorType.Aggressive:
                SetTarget();
                break;
        }
    }

    protected virtual void OnEnable()
    {
        OffInteractableObject();
        OffHitBoxes();

        currentPhaseIndex = 0;
        currentPhase = phases[currentPhaseIndex];

        isChangeRetreatState = false;
        bodyCollider.enabled = true;

        stateMachine.ChangeState(states[BossMainState.Init]);
    }

    private void Start()
    {
        InitPartColliders();
    }

    public virtual void UpdatePhase()
    {
        float percentHP = bossHealth.hp / bossHealth.maxHP;

        if (currentPhaseIndex == phases.Count - 1)
            return;

        BossPhaseData nextPhase = phases[currentPhaseIndex + 1];

        if (percentHP < nextPhase.changeHPPercent)
        {
            currentPhase = nextPhase;
            currentPhaseIndex = currentPhase.phaseIndex;

            // 현재 페이즈에 맞는 정보들 세팅
            currentPhase.Init();

            // 페이즈가 마지막 페이즈가 아니라면 포효 상태로 넘어갈 수 있도록 변수 설정
            if(currentPhase.phaseIndex < phases.Count -1)
            {
                isChangePhase = true;
            }
        }
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        stateMachine.Update();

        if(Input.GetKeyDown(KeyCode.H))
        {
            bossHealth.TakeDamage(100f);
        }
    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistacne);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectDistance);
    }

    public void SetTarget()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform;

        if (target != null)
            targetStat = target.GetComponentInParent<StatController>().Runtime;
        else
            Debug.Log("Scene에 Player 오브젝트가 없습니다!");
    }

    public virtual void InitPartColliders()
    {
        if (bossBodyPartsColliders.Count != 0) return;

        bossBodyParts = GetComponentsInChildren<BossBodyPartBase>();

        // 파츠들의 모든 콜라이더를 가져온다.
        for (int i = 0; i < bossBodyParts.Length; i++)
        {
            Collider partsCollider = null;
            BossBodyPartBase partsObject = null;

            // 하위 파츠가 있는 상위 파츠
            if (bossBodyParts[i] is BossHighParts)
            {
                partsObject = bossBodyParts[i];

                List<BossLowParts> bossPartsList = (partsObject as BossHighParts).partsList;
                for (int j = 0; j < bossPartsList.Count; j++)
                {
                    if (bossPartsList[j].TryGetComponent(out partsCollider))
                    {
                        bossBodyPartsColliders.Add(partsCollider);
                    }
                }
            }
            else
            {
                if (bossBodyParts[i].TryGetComponent(out partsCollider))
                    bossBodyPartsColliders.Add(partsCollider);
            }

            if(partsCollider == null)
            {
                Debug.LogAssertion($"{gameObject.name} {partsObject.name} 파츠에 콜라이더가 없습니다.");
            }
        }

        for (int i = 0; i < bossBodyParts.Length; i++)
        {
            if (bossBodyParts[i] is BossBodyDestructionPart)
            {
                bossBodyParts[i].onDestPart += stateMachine.OnDestruct;
                //bossBodyParts[i].onDestPart += OffHitBoxes;
                //bossHealth.OnDead += OffHitBoxes;
            }
            else if (bossBodyParts[i] is BossBodyDestructionListPart)
            {
                bossBodyParts[i].onDestPart += stateMachine.OnDestruct;
                //bossBodyParts[i].onDestPart += OffHitBoxes;
                //bossHealth.OnDead += OffHitBoxes;
            }
            else if(bossBodyParts[i] is BossBodyCuttingPart)
            {
                bossBodyParts[i].onCutPart += stateMachine.OnDestruct;
                //bossBodyParts[i].onCutPart += OffHitBoxes;
                //bossHealth.OnDead += OffHitBoxes;
            }
            else if (bossBodyParts[i] is BossBodyCuttingListParts)
            {
                bossBodyParts[i].onCutPart += stateMachine.OnDestruct;
                //bossBodyParts[i].onCutPart += OffHitBoxes;
                //bossHealth.OnDead += OffHitBoxes;
            }
        }
    }

    // 부위파괴 이벤트 구독
    public void SubscribeDestructionPartsEvent()
    {
        for (int i = 0; i < bossBodyParts.Length; i++)
        {
            if (bossBodyParts[i] is BossBodyDestructionPart)
            {
                bossBodyParts[i].onDestPart += stateMachine.OnDestruct;
            }
            else if (bossBodyParts[i] is BossBodyDestructionListPart)
            {
                bossBodyParts[i].onDestPart += stateMachine.OnDestruct;
            }
            else if (bossBodyParts[i] is BossBodyCuttingPart)
            {
                bossBodyParts[i].onCutPart += stateMachine.OnDestruct;
            }
            else if (bossBodyParts[i] is BossBodyCuttingListParts)
            {
                bossBodyParts[i].onCutPart += stateMachine.OnDestruct;
            }
        }
    }

    // 부위파괴 이벤트 구독 해제
    public void UnSubscribeDestructionPartsEvent()
    {
        for (int i = 0; i < bossBodyParts.Length; i++)
        {
            if (bossBodyParts[i] is BossBodyDestructionPart)
            {
                bossBodyParts[i].onDestPart -= stateMachine.OnDestruct;
            }
            else if (bossBodyParts[i] is BossBodyDestructionListPart)
            {
                bossBodyParts[i].onDestPart -= stateMachine.OnDestruct;
            }
            else if (bossBodyParts[i] is BossBodyCuttingPart)
            {
                bossBodyParts[i].onCutPart -= stateMachine.OnDestruct;
            }
            else if (bossBodyParts[i] is BossBodyCuttingListParts)
            {
                bossBodyParts[i].onCutPart -= stateMachine.OnDestruct;
            }
        }
    }

    // 모든 부위 파츠들의 콜라이더를 꺼줌
    public void OffPartColliders()
    {
        for (int i = 0; i < bossBodyPartsColliders.Count; i++)
        {
            bossBodyPartsColliders[i].enabled = false;
        }
    }

    public void OnPartColliders()
    {
        for (int i = 0; i < bossBodyPartsColliders.Count; i++)
        {
            if (bossBodyPartsColliders[i].gameObject.activeSelf == false)
                bossBodyPartsColliders[i].gameObject.SetActive(true);

            bossBodyPartsColliders[i].enabled = true;
        }

        for (int i = 0; i < bossBodyParts.Length; i++)
        {
            bossBodyParts[i].gameObject.SetActive(true);
        }
    }

    public virtual void OnDestructionEffects() { }

    public virtual void OffHitEffects() { }

    public void OnHitBoxes()
    {
        for (int i = 0; i < hitBoxes.Length; i++)
        {
            if (hitBoxes[i].HasLowPartsHitbox())
            {
                hitBoxes[i].OnLowHitBoxes();
            }
            else
                hitBoxes[i].gameObject.SetActive(true);
        }
    }

    public void OffHitBoxes()
    {
        for (int i = 0; i < hitBoxes.Length; i++)
        {
            if (hitBoxes[i].HasLowPartsHitbox())
            {
                hitBoxes[i].OffLowHitBoxes();
            }
            else
                hitBoxes[i].gameObject.SetActive(false);
        }
    }

    public void OnInteractableObject()
    {
        interactable.gameObject.SetActive(true);
    }

    public void OffInteractableObject()
    {
        interactable.gameObject.SetActive(false);
    }

    public float GetWalkSpeed()
    {
        return bossData.bossSpeed / 2f;
    }

    public float GetRunSpeed()
    {
        return bossData.bossSpeed;
    }

    public bool IsHeadDestruct()
    {
        if(headPart != null)
        {
            return headPart.IsDestruction();
        }

        return false;
    }

    // 타겟과의 방향을 반환
    public Vector3 GetTargetDir()
    {
        if(target != null)
        {
            Vector3 dir = (target.transform.position - transform.position);

            dir = new Vector3(dir.x, 0, dir.z);

            return dir.normalized;
        }
        return Vector3.zero;
    }

    public Vector3 GetBossDir()
    {
        Vector3 lookDir = transform.forward;

        lookDir = new Vector3(lookDir.x, 0, lookDir.z);

        return lookDir;
    }
}