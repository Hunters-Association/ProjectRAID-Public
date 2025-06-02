using UnityEngine;
using UnityEngine.AI;

public class FleeToSpawnState : MonsterBaseState
{
    private Vector3 spawnNavMeshPosition;
    private bool destinationSet = false;

    public FleeToSpawnState(Monster contextMonster) : base(contextMonster) { }

    public override void EnterState()
    {
        destinationSet = false; Vector3 spawnPoint = monster.spawnPosition;
        if (NavMesh.SamplePosition(spawnPoint, out NavMeshHit hit, 5.0f, NavMesh.AllAreas))
        {
            spawnNavMeshPosition = hit.position;
            monster.StartMovement(spawnNavMeshPosition);
            destinationSet = true;
        }
        else
        {
            Debug.LogError($"스폰 위치({spawnPoint}) 근처 NavMesh 없음! Idle 복귀.", monster);
            monster.ChangeState(MonsterState.Idle);
        }
    }

    public override void UpdateState()
    {
        // 목적지 설정이 성공했고, 목적지에 도착했다면
        if (destinationSet && monster.HasReachedDestination())
        {
            monster.StopMovement(); // 이동 중지
            Debug.Log($"[FleeToSpawn] 목적지 도착! Pacify 호출 및 Idle 전환 시작.");
            monster.Pacify(); // 몬스터의 public 메서드를 호출하여 온순화 상태로 설정
            monster.ChangeState(MonsterState.Idle); // Idle 상태로 전환 (IdleState에서 쿨다운 시작)
        }

    }

    public override void ExitState() { }    

    public override void OnTakeDamage(DamageInfo info)
    {
       
    }
}
