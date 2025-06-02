using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCReturningToPostState : NPCBaseState
{
    public NPCReturningToPostState(NPCStateMachine stateMachine, NPCController npcController) : base(stateMachine, npcController) { }

    public override void Enter()
    {
        NpcController.RequestSheatheWeapon();
        Debug.Log($"[{NpcController.npcData.npcName}] Entering ReturningToPost State.");
        // NPCController에 저장된 초기 위치(_initialPosition)로 이동
        StartMovementTo(NpcController.InitialPosition, NpcController.npcData.baseMoveSpeed);
        Animator.SetFloat("Speed", NpcController.npcData.baseMoveSpeed); // 걷기 애니메이션
    }

    public override void Execute()
    {
        if (HasReachedDestination())
        {
            NpcController.transform.rotation = Quaternion.Slerp(NpcController.transform.rotation, NpcController.InitialRotation, Time.deltaTime * 5f);
            if (Quaternion.Angle(NpcController.transform.rotation, NpcController.InitialRotation) < 1.0f)
            {
                StateMachine.ChangeState(NPCState.Idle); // 원래 위치 도착 후 Idle 상태로
            }
        }
    }

    public override void Exit()
    {
        StopMovement();
        Animator.SetFloat("Speed", 0f);
    }
}
