using System.Collections.Generic;
using UnityEngine;

public class NPCStateMachine : MonoBehaviour
{
    private NPCController _npcController;
    //[Header("Targeting Settings")] // Inspector에서 잘 보이도록 헤더 추가 
    //[SerializeField] private LayerMask playerLayerMask;
    public NPCBaseState CurrentState { get; private set; }
    private Dictionary<NPCState, NPCBaseState> _states;

    [Header("Debug")]
    [SerializeField] // 인스펙터에서 현재 상태 확인용
    private NPCState _currentStateEnumForDebug;

    private bool _canEngageInCombat = false; // 외부(NPCController, NPCAffinity)에서 제어

    public SkillData NextSkillToUse { get; private set; }
    public GameObject NextSkillTarget { get; private set; }


    public void SetNextSkill(SkillData skill, GameObject target)
    {
        NextSkillToUse = skill;
        NextSkillTarget = target;
    }

    // NPCUsingSkillState에서 호출하기 위한 getter 메서드 (선택적, 프로퍼티 직접 접근도 가능)
    public SkillData GetNextSkillToUse() => NextSkillToUse;
    public GameObject GetNextSkillTarget() => NextSkillTarget;

    public void Initialize(NPCController controller)
    {
        _npcController = controller;
        if (_npcController == null)
        {
            Debug.LogError("NPCStateMachine requires a valid NPCController to initialize.", this);
            enabled = false;
            return;
        }

        _states = new Dictionary<NPCState, NPCBaseState>
        {
            // 각 상태 객체 생성 및 등록
            { NPCState.Idle, new NPCIdleState(this, _npcController) },
            { NPCState.FollowPlayer, new NPCFollowPlayerState(this, _npcController) },
            { NPCState.CombatIdle, new NPCCombatIdleState(this, _npcController) },
            { NPCState.UsingSkill, new NPCUsingSkillState(this, _npcController) },
            { NPCState.Fainted, new NPCFaintedState(this, _npcController) },
            { NPCState.ReturningToPost, new NPCReturningToPostState(this, _npcController) },
        };

        // 초기 전투 참여 가능 여부 설정 (NPCAffinity의 초기값에 따라)
        SetCombatEngagement(_npcController.affinityComponent.CurrentAffinityLevelData?.canJoinCombat ?? false);

        // 초기 상태 결정
        if (_canEngageInCombat && IsPlayerNearbyAndInCombat()) // 예시: 전투 참여 가능하고 주변에 적이 있다면
        {
            ChangeState(NPCState.CombatIdle);
        }
        else
        {
            ChangeState(NPCState.Idle);
        }
    }

    void Update()
    {
        CurrentState?.Execute();
    }

    public void ChangeState(NPCState newStateKey)
    {
        CurrentState?.Exit(); // 이전 상태 Exit 호출

        if (_states.TryGetValue(newStateKey, out NPCBaseState newStateInstance))
        {
            CurrentState = newStateInstance;
            CurrentState.Enter(); // 새 상태 Enter 호출
            _currentStateEnumForDebug = newStateKey; // 디버깅용 업데이트
            // Debug.Log($"NPC [{_npcController.npcData.npcName}] changed state to: {newStateKey}");
        }
        else
        {
            Debug.LogError($"NPC [{_npcController.npcData.npcName}] State [{newStateKey}] not found in state machine! Reverting to Idle.", _npcController);
            // 안전장치: 정의되지 않은 상태로 가려고 하면 Idle로
            if (_states.TryGetValue(NPCState.Idle, out NPCBaseState idleState))
            {
                CurrentState = idleState;
                CurrentState.Enter();
                _currentStateEnumForDebug = NPCState.Idle;
            }
        }
    }

    /// <summary>
    /// NPCController가 외부 요인(예: 호감도)에 의해 전투 참여 가능 여부를 설정합니다.
    /// </summary>
    public void SetCombatEngagement(bool canEngage)
    {
        _canEngageInCombat = canEngage;
        // Debug.Log($"NPC [{_npcController.npcData.npcName}] Combat Engagement set to: {canEngage}");

        // 상태 전환 로직은 각 상태의 Execute 내부에서 처리하거나,
        // 여기서 특정 조건 만족 시 강제 전환할 수 있음.
        // 예를 들어, 참여 가능해졌는데 현재 Idle이고 플레이어가 전투 중이면 CombatIdle로.
        if (_canEngageInCombat && CurrentState is NPCIdleState && IsPlayerNearbyAndInCombat())
        {
            ChangeState(NPCState.CombatIdle);
        }
        else if (!_canEngageInCombat && (CurrentState is NPCCombatIdleState || CurrentState is NPCUsingSkillState /*등 전투 관련 상태*/))
        {
            // 전투 참여 불가능해졌는데 전투 중이면 비전투 상태로 전환 (예: FollowPlayer 또는 Idle)
            ChangeState(NPCState.FollowPlayer); // 또는 Idle
        }
    }

    public bool CanEngageInCombat()
    {
        return _canEngageInCombat;
    }

    /// <summary>
    /// NPC가 데미지를 받았을 때 현재 상태에 알립니다.
    /// </summary>
    public void OnDamaged(DamageInfo info)
    {
        CurrentState?.OnDamaged(info);
    }

    // 플레이어가 근처에 있고 전투 중인지 확인하는 헬퍼 (구현 필요)
    public bool IsPlayerNearbyAndInCombat()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null || _npcController == null || _npcController.npcData == null) return false;

        float distanceToPlayer = Vector3.Distance(_npcController.transform.position, playerObj.transform.position);
        // baseFollowRangeMax가 null일 수 있으므로 npcData null 체크 후 접근
        float followRange = _npcController.npcData.baseFollowRangeMax > 0 ? _npcController.npcData.baseFollowRangeMax : 10f; // 기본값 설정
        bool isPlayerNearby = distanceToPlayer < followRange;

        PlayerController playerController = playerObj.GetComponentInParent<PlayerController>();
        bool isPlayerInCombat = false;
        if (playerController != null && playerController.CurrentState != null) // CurrentState null 체크 추가
        {
            isPlayerInCombat = playerController.CurrentState is CombatState ||
                               playerController.CurrentState is AttackState ||
                               playerController.CurrentState is AimingState;
            //Debug.Log($"[NPCStateMachine] IsPlayerNearbyAndInCombat: PlayerNearby={isPlayerNearby} (Dist:{distanceToPlayer:F1} < Range:{followRange:F1}), PlayerInCombat={isPlayerInCombat}, PlayerState={playerController.CurrentState?.GetType().Name ?? "null"}");
        }
        else
        {
            // Debug.LogWarning("qweqweqewqewqweqweqweqew");
        }
        

        return isPlayerNearby && isPlayerInCombat;
    }
}
