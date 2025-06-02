using UnityEngine;
using UnityEngine.AI;

public abstract class NPCBaseState
{
    protected NPCStateMachine StateMachine { get; private set; }
    protected NPCController NpcController { get; private set; }
    protected NavMeshAgent Agent { get; private set; } // 편의를 위해 NavMeshAgent 직접 참조
    protected Animator Animator { get; private set; } // 편의를 위해 Animator 직접 참조

    public NPCBaseState(NPCStateMachine stateMachine, NPCController npcController)
    {
        this.StateMachine = stateMachine;
        this.NpcController = npcController;
        if (npcController != null)
        {
            this.Agent = npcController.navMeshAgent;
            this.Animator = npcController.npcAnimator;
        }
    }

    /// <summary>
    /// 상태에 진입할 때 호출됩니다.
    /// </summary>
    public abstract void Enter();

    /// <summary>
    /// 매 프레임 상태 로직을 실행합니다.
    /// </summary>
    public abstract void Execute();

    /// <summary>
    /// 상태에서 빠져나갈 때 호출됩니다.
    /// </summary>
    public abstract void Exit();

    /// <summary>
    /// NPC가 데미지를 입었을 때 호출될 수 있는 가상 메서드입니다.
    /// 상태별로 피격 시 반응을 다르게 하고 싶을 때 재정의합니다.
    /// </summary>
    
    public virtual void OnDamaged(DamageInfo info)
    {
        // 기본적으로는 아무것도 하지 않음.
        // 예: 특정 상태에서는 피격 시 다른 상태로 즉시 전환 등
    }

    // --- 공통 헬퍼 메서드 ---

    protected void StartAnimation(string animationNameBool)
    {
        if (Animator != null)
        {
            Animator.SetBool(animationNameBool, true);
        }
    }

    protected void StopAnimation(string animationNameBool)
    {
        if (Animator != null)
        {
            Animator.SetBool(animationNameBool, false);
        }
    }

    protected void TriggerAnimation(string triggerName)
    {
        //if (Animator != null)
        //{
        //    Animator.SetTrigger(triggerName);
        //}
    }

    protected void StartMovementTo(Vector3 destination, float speed)
    {
        if (Agent != null && Agent.isOnNavMesh)
        {
            Agent.speed = speed;
            Agent.SetDestination(destination);
            Agent.isStopped = false;
            // 보통 "Walk" 또는 "Run" 애니메이션 파라미터도 여기서 설정
            // StartAnimation("IsMoving"); // 또는 Animator.SetFloat("Speed", speed);
        }
    }

    protected void StopMovement()
    {
        if (Agent != null && Agent.isOnNavMesh && !Agent.isStopped)
        {
            Agent.isStopped = true;
            Agent.velocity = Vector3.zero; // 관성 제거
            // StopAnimation("IsMoving"); // 또는 Animator.SetFloat("Speed", 0f);
        }
    }

    protected bool HasReachedDestination(float stoppingDistanceOffset = 0.1f)
    {
        if (Agent == null || !Agent.isOnNavMesh || Agent.pathPending)
        {
            return false;
        }
        // isStopped가 true여도 remainingDistance는 이전 경로 기준일 수 있으므로,
        // isStopped가 false일 때만 경로 완료로 간주하거나,
        // 경로가 없거나(isStopped=true) 도착했을 때를 모두 포함.
        return Agent.isStopped || (Agent.remainingDistance <= Agent.stoppingDistance + stoppingDistanceOffset);
    }

    protected void LookAt(Transform target)
    {
        if (target == null || NpcController == null) return;
        Vector3 direction = (target.position - NpcController.transform.position).normalized;
        direction.y = 0; // 수평으로만 바라보도록
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            NpcController.transform.rotation = Quaternion.Slerp(NpcController.transform.rotation, lookRotation, Time.deltaTime * (Agent != null ? Agent.angularSpeed / 120f : 5f)); // NavMeshAgent의 angularSpeed 활용 또는 고정값
        }
    }

    protected float GetDistanceToPlayer()
    {
        // 플레이어 참조 방식은 프로젝트에 맞게 수정 (예: GameManager.Instance.PlayerTransform)
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null && NpcController != null)
        {
            return Vector3.Distance(NpcController.transform.position, playerObject.transform.position);
        }
        return float.MaxValue;
    }
}
