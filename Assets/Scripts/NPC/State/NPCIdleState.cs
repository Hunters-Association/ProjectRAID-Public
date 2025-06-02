using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCIdleState : NPCBaseState
{
    private float _idleTimer;
    private float _timeToNextAction;

    public NPCIdleState(NPCStateMachine stateMachine, NPCController npcController) : base(stateMachine, npcController) { }

    public override void Enter()
    {
        // Debug.Log($"[{NpcController.npcData.npcName}] Entering Idle State.");
        StopMovement();
        // TODO: "Idle" 또는 "Breathe" 같은 기본 대기 애니메이션 재생
        // TriggerAnimation("Idle_Trigger"); 또는 StartAnimation("IsIdling");
        // Animator.Play("Idle"); // 예시: "Idle" 상태 직접 재생 (루프 애니메이션 가정)
        SetRandomNextActionTime();
    }

    public override void Execute()
    {
        if (NpcController.IsActivelyFollowingPlayer && NpcController.IsInCombatParty)
        {
            // 플레이어가 '함께하기'를 눌렀고, NPC가 전투 참여 자격이 있다면 따라가기 상태로 전환
            Debug.Log($"[{NpcController.npcData.npcName}] Idle 중, 플레이어 추종 시작 조건 충족. FollowPlayer 상태로 전환.");
            StateMachine.ChangeState(NPCState.FollowPlayer);
            return; // 상태 변경 후 즉시 현재 Execute 종료
        }
        // 1. 전투 참여 조건 확인
        if (StateMachine.CanEngageInCombat())
        {
            // 플레이어가 근처에 있고, 주변에 적이 감지되면 CombatIdle 상태로 전환
            // TODO: 주변 적 감지 로직 필요
            // if (IsPlayerNearby() && AreEnemiesNearby()) { StateMachine.ChangeState(NPCState.CombatIdle); return; }
            // 기획서: 캐릭터 근처에서 정지 상태 유지 -> FollowPlayer로 전환하여 따라다니게 할 수 있음
            
        }
        else // 전투 참여 불가능 (호감도 부족 등)
        {
            // 플레이어가 너무 멀어지면 따라가지 않거나, 특정 지점으로 이동할 수 있음 (선택적)
        }


        // 2. 주기적인 행동 (예: 주변 둘러보기)
        _idleTimer += Time.deltaTime;
        if (_idleTimer >= _timeToNextAction)
        {
            // TODO: 랜덤한 Idle 애니메이션 재생 (예: "LookAround", "Stretch")
            // int randomAction = Random.Range(0, 3);
            // Animator.SetInteger("IdleActionID", randomAction);
            // TriggerAnimation("DoIdleAction");
            // Debug.Log($"[{NpcController.npcData.npcName}] Performing random idle action.");
            SetRandomNextActionTime();
        }
    }

    public override void Exit()
    {
        // StopAnimation("IsIdling");
    }

    private void SetRandomNextActionTime()
    {
        _idleTimer = 0f;
        _timeToNextAction = Random.Range(3f, 7f); // 3~7초마다 다음 행동
    }

    public override void OnDamaged(DamageInfo info)
    {
        // 이제 info.attacker, info.damageAmount 등을 사용할 수 있습니다.
        if (StateMachine.CanEngageInCombat())
        {
            // NpcController.SetCurrentTarget(info.attacker); // 필요하다면 공격자를 타겟으로 설정
            Debug.Log($"[{NpcController.npcData?.npcName}] Idle 중 피격! 공격자: {info.attacker?.name}. CombatIdle 상태로 전환.");
            StateMachine.ChangeState(NPCState.CombatIdle);
        }
        // else: 비전투 상태에서 피격 시 다른 행동
    }
}

