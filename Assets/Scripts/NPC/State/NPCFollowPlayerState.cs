using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCFollowPlayerState : NPCBaseState
{
    private Transform _playerTransform;
    private const float CHECK_TARGET_INTERVAL = 0.5f; // 플레이어 위치 업데이트 주기
    private float _checkTimer;
    

    public NPCFollowPlayerState(NPCStateMachine stateMachine, NPCController npcController) : base(stateMachine, npcController) { }

    public override void Enter()
    {
        NpcController.RequestSheatheWeapon();
        // Debug.Log($"[{NpcController.npcData.npcName}] Entering FollowPlayer State.");
        // 플레이어 Transform 찾기 (실제 프로젝트에서는 GameManager 등에서 관리하는 참조 사용)
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) _playerTransform = playerObj.transform;

        if (_playerTransform == null)
        {
            Debug.LogError($"[{NpcController.npcData.npcName}] Player not found! Cannot follow. Reverting to Idle.", NpcController);
            StateMachine.ChangeState(NPCState.Idle);
            return;
        }        
    }

    public override void Execute()
    {
        if (NpcController != null && NpcController.npcData != null)
        {
            bool playerNearbyAndInCombat = StateMachine.IsPlayerNearbyAndInCombat(); // 미리 호출해서 결과 저장
            //Debug.Log($"[NPCFollowPlayerState Execute] NPC: {NpcController.npcData.npcName}, IsActivelyFollowing: {NpcController.IsActivelyFollowingPlayer}, IsInCombatParty: {NpcController.IsInCombatParty}, IsPlayerNearbyAndInCombatResult: {playerNearbyAndInCombat}");
        }
        if (!NpcController.IsActivelyFollowingPlayer || !NpcController.IsInCombatParty)
        {
            // 플레이어가 '해제하기'를 눌렀거나, NPC가 전투 참여 자격을 잃었다면
            Debug.Log($"[{NpcController.npcData.npcName}] 플레이어 추종 중단 조건 충족. ReturningToPost 상태로 전환.");
            StateMachine.ChangeState(NPCState.ReturningToPost);
            return;
        }
        if (_playerTransform == null)
        {
            StateMachine.ChangeState(NPCState.Idle);
            return;
        }

        // 주기적으로 플레이어 위치 업데이트하여 따라가기
        _checkTimer -= Time.deltaTime;
        if (_checkTimer <= 0f)
        {
            StartMovementTo(_playerTransform.position, NpcController.npcData.baseMoveSpeed);            
            _checkTimer = CHECK_TARGET_INTERVAL;
        }

        float distanceToPlayer = Vector3.Distance(NpcController.transform.position, _playerTransform.position);

        // 기획서: "캐릭터를 따라 다님 (3~6m 거리 유지)"
        // 너무 가까우면 멈춤
        if (distanceToPlayer <= NpcController.npcData.baseFollowRangeMin)
        {
            StopMovement();            
            LookAt(_playerTransform);
        }
        // 너무 멀어지면 다시 따라가기 시작 (StartMovementTo가 isStopped를 false로 하므로 자동으로 다시 움직임)
        else if (Agent.isStopped && distanceToPlayer > NpcController.npcData.baseFollowRangeMin * 1.2f) // 약간의 버퍼
        {
            StartMovementTo(_playerTransform.position, NpcController.npcData.baseMoveSpeed);            
        }


        // 전투 참여 조건 확인
        if (StateMachine.CanEngageInCombat())
        {
            // TODO: 주변 적 감지 및 CombatIdle로 전환 로직
            // if (AreEnemiesNearby()) { StateMachine.ChangeState(NPCState.CombatIdle); return; }
        }
        if (StateMachine.IsPlayerNearbyAndInCombat()) // IsPlayerNearbyAndInCombat()는 NPCStateMachine에 구현된 헬퍼
        {
            Debug.Log($"[{NpcController.npcData.npcName}] FollowPlayer 중, 플레이어 전투 감지! CombatIdle 상태로 전환.");
            StateMachine.ChangeState(NPCState.CombatIdle);
            return;
        }
    }

    public override void Exit()
    {
        // Debug.Log($"[{NpcController.npcData.npcName}] Exiting FollowPlayer State.");
        StopMovement();
        
    }
}
