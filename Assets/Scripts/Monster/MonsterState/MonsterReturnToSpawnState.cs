using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 몬스터가 설정된 행동 반경을 벗어났을 때 스폰 지점으로 복귀하는 상태입니다.
/// </summary>
public class MonsterReturnToSpawnState : MonsterBaseState
{
    private Vector3 targetDestination; // 실제 NavMesh 위의 목표 지점
    private bool destinationSet = false; // 목표 지점 설정 완료 여부

    public MonsterReturnToSpawnState(Monster contextMonster) : base(contextMonster) { }

    public override void EnterState()
    {
        // Debug.Log($"[{monster.gameObject.name}] ReturnToSpawn 상태 진입");
        monster.ClearTarget(); // 플레이어 타겟 해제

        Vector3 spawnPos = monster.GetSpawnPosition(); // 저장된 초기 스폰 위치 가져오기

        // 스폰 위치 근처의 유효한 NavMesh 지점 찾기
        if (NavMesh.SamplePosition(spawnPos, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
        {
            targetDestination = hit.position; // NavMesh 위의 점을 목표로 설정
            monster.StartMovement(targetDestination); // 목표 지점으로 이동 시작
            destinationSet = true;
            // Debug.Log($"[{monster.gameObject.name}] 스폰 위치({targetDestination})로 복귀 시작.");
        }
        else
        {
            // 스폰 위치 근처에 NavMesh가 없으면 이동 불가. Idle 상태로 전환 시도.
            Debug.LogError($"[{monster.gameObject.name}] 복귀 목표 지점 ({spawnPos}) 근처 NavMesh 없음! Idle 상태로 전환.", monster);
            monster.ChangeState(MonsterState.Idle);
        }
    }

    public override void UpdateState()
    {
        // 목표 지점이 설정되었고, 목적지에 도착했다면 Idle 상태로 전환
        if (destinationSet && monster.HasReachedDestination())
        {
            // Debug.Log($"[{monster.gameObject.name}] 스폰 지점 복귀 완료. Idle 상태로 전환.");
            monster.ChangeState(MonsterState.Idle);
        }
        // 선택적: 목적지에 도달하기 전에 행동 반경 안으로 다시 들어오면 즉시 Idle 상태로 전환?
        // else if (destinationSet && !monster.IsOutsideBehaviorRange())
        // {
        //    Debug.Log($"[{monster.gameObject.name}] 행동 반경 내 진입. 복귀 중단하고 Idle 상태로 전환.");
        //    monster.ChangeState(MonsterState.Idle);
        // }
    }

    public override void ExitState()
    {
        monster.StopMovement(); // 상태를 빠져나갈 때 이동 중지
        destinationSet = false; // 목표 설정 플래그 리셋
        // Debug.Log($"[{monster.gameObject.name}] ReturnToSpawn 상태 종료");
    }

    public override void OnTakeDamage(DamageInfo info)
    {
        // 스폰 지점으로 복귀하는 도중 피격당하면 즉시 공격 상태로 전환
        // Debug.Log($"[{monster.gameObject.name}] 복귀 중 피격! 공격자로 타겟 변경 후 Attack 상태 전환.");
        if (info.attacker != null)
        {
            monster.EvaluateNewAttacker(info.attacker); // 공격자를 새 타겟으로 설정
        }
        monster.ChangeState(MonsterState.Attack); // 공격 상태로 전환
    }
}
