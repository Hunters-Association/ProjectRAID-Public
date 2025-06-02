using UnityEngine;
using UnityEngine.AI;

public class MorvenBurrowedState : MonsterBaseState
{
    private Morven morven;
    private float timeSpentBurrowed;
    private float emergeTime;

    public MorvenBurrowedState(Monster contextMonster) : base(contextMonster)
    {
        morven = contextMonster as Morven;
        if (morven == null) Debug.LogError("MorvenBurrowedState created with non-Morven monster!");
    }

    public override void EnterState()
    {
        // Debug.Log($"[{morven.gameObject.name}] Entering Burrowed State.");
        // 이미 BurrowState에서 모습 숨기고 NavMeshAgent 비활성화 함
        morven.SetInvulnerable(true); // 확실히 무적 상태 확인

        // 땅 속에 머무를 시간 랜덤 설정
        emergeTime = Time.time + Random.Range(morven.MinTimeBurrowed, morven.MaxTimeBurrowed);
        timeSpentBurrowed = 0f;
    }

    public override void UpdateState()
    {
        timeSpentBurrowed += Time.deltaTime;

        // 설정된 시간이 지나면 나타나기 시도
        if (Time.time >= emergeTime)
        {
            TryEmerge();
        }
        // TODO: 땅 속에서 플레이어 위치 추적 로직 (선택적)
        // 주기적으로 플레이어 위치 확인하여 너무 멀어지면 근처로 순간이동?
    }

    private void TryEmerge()
    {
        Vector3 emergeTargetPos = CalculateEmergePosition(); // 플레이어 근처 위치 계산
        //Debug.Log($"[{morven.gameObject.name}] CalculateEmergePosition 결과: {emergeTargetPos}");

        NavMeshHit hit;
        // ★★★ 검색 반경 수정 ★★★
        // emergeNearPlayerDistance * 1.5f 대신 훨씬 작은 값 사용 (예: 2.0f 또는 5.0f)
        // 이 값은 emergeTargetPos가 NavMesh에서 약간 벗어났을 때 보정해주는 역할
        float searchRadius = 5.0f; // 예시 값, 맵 구조에 따라 조절 필요
        // ★★★★★★★★★★★★★★★★
        //Debug.Log($"[{morven.gameObject.name}] NavMesh.SamplePosition 검색 시작. 중심: {emergeTargetPos}, 반경: {searchRadius}");

        if (NavMesh.SamplePosition(emergeTargetPos, out hit, searchRadius, NavMesh.AllAreas))
        {
            // NavMesh 위 유효 위치 찾음
            //Debug.Log($"[{morven.gameObject.name}] NavMesh.SamplePosition 성공. 찾은 위치(hit.position): {hit.position}");

            // ★★★ 이제 hit.position은 플레이어 근처의 유효한 NavMesh 위치여야 함 ★★★
            StateContext emergeContext = new StateContext
            {
                TargetPosition = hit.position
            };
            //Debug.Log($"[{morven.gameObject.name}] 나타날 최종 위치: {hit.position}, Context 생성됨.");

            morven.ChangeState(MonsterState.Emerging, emergeContext);
            monster.animator.SetTrigger("IsBD");
        }
        else
        {
            // ★ 플레이어 근처에 유효한 NavMesh 지점을 못 찾은 경우 ★
            Debug.LogWarning($"[{morven.gameObject.name}] 플레이어 근처({emergeTargetPos})에서 유효한 NavMesh 위치를 반경 {searchRadius} 내에서 찾지 못했습니다! 스폰 지점 근처에서 나타납니다.");
            // 이 경우, 플레이어 근처가 아닌 스폰 지점 근처에서 나타나도록 명시적으로 처리하거나,
            // emergeTime을 늘려 다음 프레임에 다시 시도하도록 둘 수 있습니다.
            // (아래는 스폰 지점 근처에서 나타나도록 처리하는 예시)
            /*
            if (NavMesh.SamplePosition(morven.spawnPosition, out hit, 5.0f, NavMesh.AllAreas)) {
                 StateContext emergeContext = new StateContext { TargetPosition = hit.position };
                 morven.ChangeState(MonsterState.Emerging, emergeContext);
            } else {
                 // 스폰 지점 근처도 못찾으면 정말 문제...
                 emergeTime = Time.time + 0.5f; // 일단 다음 프레임 시도
            }
            */
            // 또는 그냥 다음 프레임에 재시도하도록 둠 (기존 로직)
            emergeTime = Time.time + 0f;
        }
    }


    // 나타날 위치 계산 (플레이어 근처)
    private Vector3 CalculateEmergePosition()
    {
        // 플레이어 위치 가져오기 (싱글톤 또는 Find 사용)
        Transform playerTransform = null;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player"); // 또는 PlayerController.Instance.transform
        if (playerObj != null) playerTransform = playerObj.transform;

        if (playerTransform != null)
        {
            // 플레이어 주변 랜덤 위치 (EmergeNearPlayerDistance 사용)
            Vector3 randomOffset = Random.insideUnitSphere * morven.EmergeNearPlayerDistance;
            randomOffset.y = 0; // y축은 0으로
            //Debug.Log($"[{morven.gameObject.name}] 플레이어({playerTransform.name}) 근처 {playerTransform.position + randomOffset} 계산됨.");
            return playerTransform.position + randomOffset;
        }
        else
        {
            // 플레이어 못 찾으면 원래 스폰 위치 근처에서 나타나기
            Debug.LogWarning($"[{morven.gameObject.name}] 플레이어를 찾지 못하여 스폰 위치 근처에서 나타납니다!");
            return morven.spawnPosition + Random.insideUnitSphere * 5f;
        }
    }

    public override void ExitState()
    {
        // 특별히 정리할 내용 없음
    }

    public override void OnTakeDamage(DamageInfo info)
    {
        // 땅 속에 있을 때는 무적이므로 호출되지 않음
    }
}
