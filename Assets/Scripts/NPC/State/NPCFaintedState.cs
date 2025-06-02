using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCFaintedState : NPCBaseState
{
    private float _reviveTimer;
    private const float DEFAULT_REVIVE_TIME = 30.0f; // 기본 부활 시간

    public NPCFaintedState(NPCStateMachine stateMachine, NPCController npcController) : base(stateMachine, npcController) { }

    public override void Enter()
    {
        // Debug.Log($"[{NpcController.npcData.npcName}] Entering Fainted State.");
        StopMovement();
        TriggerAnimation("Fainted_Trigger"); // "Fainted" 상태 루프 또는 트리거
        NpcController.GetComponent<Collider>().enabled = false; // 다른 오브젝트와 충돌 방지

        // TODO: NPCData에 부활 시간 정의 또는 고정값 사용
        _reviveTimer = NpcController.npcData.reviveTime > 0 ? NpcController.npcData.reviveTime : DEFAULT_REVIVE_TIME; // NPCData에 reviveTime 필드 추가 가정
    }

    public override void Execute()
    {
        _reviveTimer -= Time.deltaTime;
        if (_reviveTimer <= 0)
        {
            // 부활 처리
            // NpcController.Revive(NpcController.npcData.baseMaxHp / 2); // 예: 절반 체력으로 부활
            // StateMachine.ChangeState(NPCState.Idle); // 부활 후 Idle 상태로 (NPCController.Revive 내부에서 처리 가능)
        }
    }

    public override void Exit()
    {
        NpcController.GetComponent<Collider>().enabled = true;
        // StopAnimation("IsFainted");
    }
}
