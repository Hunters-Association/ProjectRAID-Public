using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;



[Serializable]
public class MonsterSpawnInfo
{
    [Tooltip("생성할 몬스터의 프리팹 (Monster 또는 자식 클래스 컴포넌트 필수)")]
    public GameObject monsterPrefab;
    [Tooltip("이 종류의 몬스터를 생성할 개체 수")]
    [Min(0)] public int spawnCount = 1;
    [Tooltip("이 몬스터만 사용할 특정 스폰 지점들 (비워두면 매니저의 공용 스폰 지점 사용)")]
    public Transform[] specificSpawnPoints;
}

//===========================================================================
// 여러 종류의 몬스터 생성, 개별 리스폰 타이머 관리 등을 담당하는 메인 클래스
//===========================================================================
public class MonsterManager : MonoBehaviour
{
    // --- Inspector 설정 변수들 ---

    [Header("생성할 몬스터 목록")]
    [SerializeField]
    [Tooltip("여기에 생성할 각 몬스터 종류별 정보(프리팹, 수량, 특정 스폰 지점 등)를 추가합니다.")]
    private List<MonsterSpawnInfo> monsterSpawnList = new List<MonsterSpawnInfo>();

    [Header("스폰 설정")]
    [SerializeField]
    [Tooltip("모든 몬스터 또는 특정 스폰 지점이 없는 몬스터가 공통으로 사용할 스폰 포인트들입니다.")]
    private Transform[] commonSpawnPoints;

    [SerializeField]
    [Tooltip("선택된 스폰 포인트 주변 얼마의 반경 내에서 가중치 랜덤으로 실제 스폰 위치를 결정할지 설정합니다.")]
    private float spawnPointRadius = 5.0f;

    [SerializeField]
    [Tooltip("생성된 모든 몬스터 오브젝트가 하이라키에서 속하게 될 부모 오브젝트의 Transform입니다. (씬 정리용, 없어도 무방)")]
    private Transform monsterParent;

    [Header("공용 관리 설정")]
    [SerializeField]
    [Tooltip("몬스터 사망 후 사체가 비활성화되고 풀에 들어갈 때까지 걸리는 시간(초)입니다.")]
    private float corpseDespawnDelay = 10f; // 사체 소멸 및 풀링 시간

    [SerializeField]
    [Tooltip("몬스터 사망 후 개별적으로 리스폰될 때까지 걸리는 시간(초)입니다.")]
    private float respawnDelay = 30f;       // 개별 리스폰 대기 시간

    [Header("행동 반경 설정")]
    [SerializeField] private float behaviorRadius = 20f;
    [SerializeField] private Vector3 behaviorCenter;

    // --- 외부 접근용 프로퍼티 ---
    public Vector3 BehaviorCenter => behaviorCenter;
    public float BehaviorRadius => behaviorRadius;

    // --- 내부 관리용 변수들 ---
    private List<Monster> activeMonsters = new List<Monster>();          // 현재 활성화된 몬스터 목록
    private Queue<Monster> inactiveMonsterPool = new Queue<Monster>(); // ★ 비활성화된 재사용 대기 몬스터 풀
    private List<DeadMonsterInfo> deadMonsters = new List<DeadMonsterInfo>(); // ★ 사망 및 리스폰 대기 목록
    private List<FledMonsterInfo> fledMonsters = new List<FledMonsterInfo>(); // 도망간 몬스터 목록

    private int totalMonsterCap; // 관리할 총 몬스터 수

    // --- 내부 데이터 구조체/클래스 ---
    // DeadMonsterInfo: 리스폰 시간 추적 포함
    private class DeadMonsterInfo { public Monster monster; public float despawnTime; public float respawnTime; }
    // FledMonsterInfo: 변경 없음
    private class FledMonsterInfo { public Monster monster; public float reactivationTime; public string reactivationPointTag; }

    // --- Unity 메시지 메서드 ---

    private void Awake()
    {
        // 행동 반경 중심점 자동 설정
        if (behaviorCenter == Vector3.zero)
        {
            behaviorCenter = transform.position;
        }

        // 생성할 총 몬스터 수 계산 (Null 체크 강화)
        totalMonsterCap = monsterSpawnList?.Sum(info => info?.spawnCount ?? 0) ?? 0;
        if (totalMonsterCap == 0)
        {
            Debug.LogWarning("생성할 몬스터 수가 0입니다. Monster Spawn List 설정을 확인하세요.", this);
        }
    }

    private void Start()
    {
        InitializeSpawner(); // 초기 스폰 실행
        // 관리 코루틴 시작
        StartCoroutine(RespawnCheckCoroutine());     // ★ 개별 리스폰 타이머 체크 코루틴
        StartCoroutine(CorpseCleanupCoroutine());    // ★ 사체 비활성화 및 풀링 코루틴
        StartCoroutine(FleeReactivationCoroutine()); // 도망 재활성화 코루틴
    }

    // --- 초기 스폰 관련 메서드 ---

    /// <summary>
    /// Inspector에 설정된 `monsterSpawnList` 정보를 기반으로 초기 몬스터들을 생성합니다.
    /// 각 몬스터는 지정된 스폰 포인트 주변에서 가중치 랜덤 위치에 스폰됩니다.
    /// </summary>
    private void InitializeSpawner()
    {
        if (monsterSpawnList == null || monsterSpawnList.Count == 0)
        {
            Debug.LogWarning("몬스터 스폰 목록(Monster Spawn List)이 비어있습니다.", this);
            return;
        }

        // ★ 추가: InitializeSpawner 시작 로그 ★
        Debug.Log($"[MonsterManager] InitializeSpawner 시작. 스폰 목록 항목 수: {monsterSpawnList.Count}");

        int spawnedCount = 0;
        // ★ 수정: 인덱스를 사용하기 위해 for 루프로 변경 ★
        for (int listIndex = 0; listIndex < monsterSpawnList.Count; listIndex++)
        {
            MonsterSpawnInfo spawnInfo = monsterSpawnList[listIndex]; // 현재 항목 가져오기
            string prefabName = "할당되지 않음"; // 기본값
           

            // ★★★ 수정: 상세 조건 체크 및 로그 ★★★
            // 1. spawnInfo 자체가 null인지 체크
            if (spawnInfo == null)
            {
                // 이 로그는 일반적으로 발생하지 않지만, 만약을 대비
                Debug.LogError($"[MonsterManager] 스폰 목록 오류 (인덱스 {listIndex}): 항목(SpawnInfo) 자체가 null입니다. 건너뜁니다.");
                continue; // 다음 항목으로
            }

            // 2. monsterPrefab이 할당되었는지 체크
            if (spawnInfo.monsterPrefab == null)
            {
                // 이 경우가 가장 흔한 원인일 수 있음
                //Debug.LogError($"[MonsterManager] 스폰 목록 오류 (인덱스 {listIndex}): Monster Prefab 필드가 비어있습니다(None). 건너뜁니다.");
                continue; // 다음 항목으로
            }
            else // 프리팹이 할당된 경우
            {
                prefabName = spawnInfo.monsterPrefab.name; // 프리팹 이름 저장

                // 3. 할당된 프리팹에 Monster 또는 자식 스크립트가 있는지 체크
                Monster monsterComponent = spawnInfo.monsterPrefab.GetComponent<Monster>();
                if (monsterComponent == null)
                {
                    Debug.LogError($"[MonsterManager] 스폰 목록 오류 (인덱스 {listIndex}): 프리팹 '{prefabName}'에 Monster 또는 자식 스크립트가 없습니다. 건너뜁니다.", spawnInfo.monsterPrefab);
                    continue; // 다음 항목으로
                }
                
            }            

            // 모든 검사를 통과했다면 로그 출력 (정상 항목 확인용)
            Debug.Log($"[MonsterManager] 스폰 정보 유효 (인덱스 {listIndex}): Prefab='{prefabName}', Count={spawnInfo.spawnCount}");

            // --- 기존 스폰 로직 ---
            Transform[] targetSpawnPoints = (spawnInfo.specificSpawnPoints != null && spawnInfo.specificSpawnPoints.Length > 0)
                                           ? spawnInfo.specificSpawnPoints
                                           : commonSpawnPoints;

            if (targetSpawnPoints == null || targetSpawnPoints.Length == 0)
            {
                Debug.LogWarning($"몬스터 {spawnInfo.monsterPrefab.name} (인덱스 {listIndex})를 위한 스폰 포인트가 없습니다. 스폰 건너뜀.");
                continue;
            }

            for (int i = 0; i < spawnInfo.spawnCount; i++)
            {
                Transform baseSpawnPoint = GetRandomSpawnPoint(targetSpawnPoints);
                if (baseSpawnPoint == null) continue;

                if (GetWeightedRandomNavMeshPosition(baseSpawnPoint.position, spawnPointRadius, out Vector3 calculatedSpawnPosition))
                {
                    SpawnNewMonster(spawnInfo.monsterPrefab, calculatedSpawnPosition, Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0));
                    spawnedCount++;
                }
                else
                {
                    Debug.LogWarning($"스폰 포인트 {baseSpawnPoint.name} 주변에서 {spawnInfo.monsterPrefab.name} 스폰 위치 찾기 실패.");
                }
            }
            // --- 기존 스폰 로직 끝 ---
        }

        Debug.Log($"[MonsterManager] InitializeSpawner 완료. 총 {spawnedCount} 마리 스폰 시도 완료."); // 완료 로그 추가
    }

    /// <summary>
    /// 지정된 프리팹을 사용하여 특정 위치와 회전값으로 새 몬스터를 생성하고 초기화합니다.
    /// </summary>
    private void SpawnNewMonster(GameObject prefabToSpawn, Vector3 initialPosition, Quaternion initialRotation)
    {
        //  Instantiate 시 계산된 위치 사용 
        GameObject monsterObj = Instantiate(prefabToSpawn, initialPosition, initialRotation, monsterParent);
        Monster monster = monsterObj.GetComponent<Monster>();

        if (monster != null)
        {
            //  Setup 호출 시 계산된 초기 위치 전달 
            monster.Setup(this);
            monster.InitializeMonster(); // 내부 초기화
            activeMonsters.Add(monster);
        }
        else
        {
            Debug.LogError("생성된 몬스터 프리팹에 Monster 컴포넌트가 없습니다!", monsterObj);
            Destroy(monsterObj);
        }
    }

    // --- 가중치 랜덤 위치 계산 관련 메서드 ---

    /// <summary>
    /// 중심점에서 멀어질수록 확률이 낮아지는 2D 위치(XZ 평면)를 반환합니다.
    /// Sin 함수를 이용하여 중심 근처에 높은 가중치를 둡니다.
    /// </summary>
    private Vector2 GetWeightedRandomXZ(Vector3 center, float range)
    {
        Vector2 center2D = new Vector2(center.x, center.z); // 3D 중심점을 2D로 변환
        const int maxAttempts = 100; // 최대 시도 횟수

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 offset = UnityEngine.Random.insideUnitCircle * range;
            Vector2 candidate = center2D + offset;
            float distanceNormalized = (range > 0.001f) ? offset.magnitude / range : 0f; // 0 나누기 방지
            float probability = Mathf.Sin((1f - distanceNormalized) * Mathf.PI * 0.5f); // 중심에서 1, 끝에서 0

            if (UnityEngine.Random.value < probability) return candidate; // 확률 통과 시 반환
        }
        // 실패 시 중심 근처 랜덤 위치 반환
        // Debug.LogWarning($"가중치 랜덤 위치 계산 {maxAttempts}회 실패. 중심 근처({range * 0.5f}) 랜덤 위치 반환.");
        return center2D + UnityEngine.Random.insideUnitCircle * (range * 0.5f);
    }

    /// <summary>
    /// 가중치 랜덤으로 계산된 2D 위치 근처의 유효한 NavMesh 위 3D 위치를 찾습니다.
    /// </summary>
    private bool GetWeightedRandomNavMeshPosition(Vector3 center, float range, out Vector3 result)
    {
        Vector2 randomXZ = GetWeightedRandomXZ(center, range); // 가중치 랜덤 XZ 계산
        Vector3 randomPos3D = new Vector3(randomXZ.x, center.y, randomXZ.y); // 3D 좌표로 (Y는 우선 중심값)
        float searchRadius = 2.0f; // NavMesh 탐색 반경

        if (NavMesh.SamplePosition(randomPos3D, out NavMeshHit hit, searchRadius, NavMesh.AllAreas)) // 유효 지점 탐색
        {
            result = hit.position; // 찾은 위치 반환
            return true;
        }
        else // 실패 시
        {
            // 선택적: Raycast로 지면 찾고 다시 SamplePosition 시도
            result = center; // 최종 실패 시 중심점 반환
            return false;
        }
    }

    // --- 몬스터 상태 보고 메서드 (Monster 클래스에서 호출) ---

    /// <summary> ★ 몬스터 사망 시 호출. 사체 및 리스폰 대기 목록에 추가하고 리스폰 시간 설정 ★ </summary>
    public void ReportDeath(Monster deadMonster)
    {
        if (activeMonsters.Remove(deadMonster)) // 활성 목록에서 제거 성공 시
        {
            fledMonsters.RemoveAll(fm => fm.monster == deadMonster); // 도망 목록에서도 제거
            if (!deadMonsters.Exists(dm => dm.monster == deadMonster)) // 사망 목록 중복 방지
            {
                float despawnAt = Time.time + corpseDespawnDelay;
                float respawnAt = Time.time + respawnDelay; // ★ 개별 리스폰 시간 계산
                deadMonsters.Add(new DeadMonsterInfo { monster = deadMonster, despawnTime = despawnAt, respawnTime = respawnAt }); // ★ 저장
                                                                                                                                   // Debug.Log($"{deadMonster.monsterData?.monsterName} 사망 보고. 사체 소멸: {despawnAt:F1}s, 리스폰: {respawnAt:F1}s");
            }
        }
    }

    /// <summary> 몬스터 도망 성공 시 호출 </summary>
    public void ReportSuccessfulFlee(Monster fledMonster, float reactivationDelay, string pointTag)
    {
        if (activeMonsters.Remove(fledMonster)) // 활성 목록에서 제거 성공 시
        {
            if (!fledMonsters.Exists(fm => fm.monster == fledMonster)) // 도망 목록 중복 방지
                fledMonsters.Add(new FledMonsterInfo { monster = fledMonster, reactivationTime = Time.time + reactivationDelay, reactivationPointTag = pointTag });
        }
    }

    // --- 관리 코루틴 ---

    /// <summary> ★ 주기적으로 사체를 정리: 시간이 되면 비활성화하고 오브젝트 풀에 추가 ★ </summary>
    private IEnumerator CorpseCleanupCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f); // 1초마다 체크
            for (int i = deadMonsters.Count - 1; i >= 0; i--) // 뒤에서부터 제거 고려
            {
                DeadMonsterInfo deadInfo = deadMonsters[i];
                if (deadInfo.monster == null) { deadMonsters.RemoveAt(i); continue; } // Null 참조 정리

                // ★ 아직 활성화 상태이고 사체 소멸 시간이 지났다면 처리 ★
                if (deadInfo.monster.gameObject.activeSelf && Time.time >= deadInfo.despawnTime)
                {
                    Monster monsterToDespawn = deadInfo.monster;
                    // Debug.Log($"[CorpseCleanup] {monsterToDespawn.name} 사체 소멸 시간 도달. 비활성화 및 풀 추가.");

                    monsterToDespawn.gameObject.SetActive(false); // 비활성화

                    // ★ 풀에 추가 (중복 방지) ★
                    if (!inactiveMonsterPool.Contains(monsterToDespawn))
                    {
                        inactiveMonsterPool.Enqueue(monsterToDespawn);
                        // Debug.Log($"[CorpseCleanup] {monsterToDespawn.name} 풀에 추가됨. 현재 풀 크기: {inactiveMonsterPool.Count}");
                    }
                    // ★ 여기서 deadMonsters 리스트에서 제거하지 않음! 리스폰 타이머 위해 ★
                }
            }
        }
    }

    /// <summary> ★ 주기적으로 개별 몬스터 리스폰 확인 및 실행 (풀링 연동) ★ </summary>
    private IEnumerator RespawnCheckCoroutine()
    {
        List<DeadMonsterInfo> toRespawn = new List<DeadMonsterInfo>(); // 리스폰 대상 임시 저장

        while (true)
        {
            yield return new WaitForSeconds(1f); // 1초 간격 체크
            toRespawn.Clear(); // 매번 초기화

            // 죽은 몬스터 목록(리스폰 대기 목록) 확인
            for (int i = deadMonsters.Count - 1; i >= 0; i--)
            {
                DeadMonsterInfo deadInfo = deadMonsters[i];
                if (deadInfo.monster == null) { deadMonsters.RemoveAt(i); continue; } // Null 참조 제거

                // 리스폰 시간이 되었는지 확인
                if (Time.time >= deadInfo.respawnTime)
                {
                    // 필드 여유 공간 확인 (현재 활성+도망 < 최대치)
                    if (activeMonsters.Count + fledMonsters.Count < totalMonsterCap)
                    {
                        toRespawn.Add(deadInfo); // 리스폰 대상에 추가
                        deadMonsters.RemoveAt(i); // ★ 리스폰 처리할 것이므로 사망 대기 목록에서 제거
                    }
                    // else { Debug.Log($"{deadInfo.monster.name} 리스폰 대기 중 (필드 꽉 참)"); }
                }
            }

            // 리스폰 대상들 처리
            foreach (var info in toRespawn)
            {
                RespawnDeadMonster(info); // ★ 수정된 리스폰 함수 호출
            }
        }
    }

    /// <summary> 주기적으로 도망간 몬스터의 재활성화 조건 확인 및 실행 </summary>
    private IEnumerator FleeReactivationCoroutine()
    {
        while (true)
        {
            // ★ yield return null; 또는 짧은 시간으로 변경하여 더 자주 체크 가능
            yield return new WaitForSeconds(1f); // 예: 1초마다 체크

            for (int i = fledMonsters.Count - 1; i >= 0; i--) // 뒤에서부터 제거
            {
                FledMonsterInfo info = fledMonsters[i];
                if (info.monster == null) { fledMonsters.RemoveAt(i); continue; } // Null 체크

                if (Time.time >= info.reactivationTime) // 재활성화 시간 확인
                {
                    // 필드 몬스터 수 확인 (재활성화 시 +1 고려)
                    if (activeMonsters.Count + fledMonsters.Count - 1 < totalMonsterCap)
                    {
                        ReactivateFledMonster(info); // 재활성화 실행
                        fledMonsters.RemoveAt(i);    // 목록에서 제거
                    }
                    // else { Debug.Log($"{info.monster.monsterData?.name} 재활성화 대기 (필드 꽉 참)"); }
                }
            }
        }
    }

    // --- 리스폰/재활성화 실행 메서드 ---

    /// <summary> ★ 죽은 몬스터 정보를 받아 리스폰시킵니다. (오브젝트 풀링 연동) ★ </summary>
    private void RespawnDeadMonster(DeadMonsterInfo deadInfo)
    {
        Monster monsterToRespawn = deadInfo.monster;
        if (monsterToRespawn == null) return;

        // --- 리스폰 위치 계산 (Specific > Common -> 가중치 랜덤) ---
        MonsterData respawnMonsterData = monsterToRespawn.monsterData;
        MonsterSpawnInfo spawnInfo = null;
        if (respawnMonsterData != null)
            spawnInfo = monsterSpawnList.FirstOrDefault(info => info?.monsterPrefab?.GetComponent<Monster>()?.monsterData?.monsterID == respawnMonsterData.monsterID);
        Transform[] targetSpawnPoints = (spawnInfo?.specificSpawnPoints?.Length > 0) ? spawnInfo.specificSpawnPoints : commonSpawnPoints;
        if (targetSpawnPoints == null || targetSpawnPoints.Length == 0)
        { Debug.LogError($"몬스터 {respawnMonsterData?.monsterName ?? monsterToRespawn.name} 리스폰 포인트 없음! 리스폰 실패."); return; }
        Transform baseSpawnPoint = GetRandomSpawnPoint(targetSpawnPoints);
        if (baseSpawnPoint == null)
        {
            Debug.LogError("선택된 리스폰 포인트 null! 리스폰 실패.");
            return;
        }
        if (!GetWeightedRandomNavMeshPosition(baseSpawnPoint.position, spawnPointRadius, out Vector3 respawnPosition))
        {
            Debug.LogError($"리스폰 위치 찾기 실패({baseSpawnPoint.name} 주변)! 리스폰 실패.");
            return;
        }
        // --- 위치 계산 끝 ---

        // ★ 오브젝트 풀에서 해당 객체 제거 시도 (선택적이지만 권장 - 효율적 방법 필요) ★
        // Queue는 직접 제거가 어려우므로, 만약 풀 관리가 중요하다면 List<Monster>로 변경 고려
        // 여기서는 CorpseCleanup에서 풀에 넣고, 여기서 풀 상태와 관계없이 활성화하는 것으로 가정
        bool wasInPool = inactiveMonsterPool.Contains(monsterToRespawn); // 임시 확인 (비효율적)
        if (wasInPool)
        {
            // Queue에서 특정 요소 제거는 번거로우므로 새 Queue를 만들거나 List 사용 권장
            // 예: inactiveMonsterPool = new Queue<Monster>(inactiveMonsterPool.Where(m => m != monsterToRespawn));
            // 여기서는 로그만 남기고 그냥 진행
            // Debug.Log($"{monsterToRespawn.name} 리스폰 시 풀에 존재했음.");
        }


        // ★ 몬스터 활성화 및 초기화 ★
        monsterToRespawn.gameObject.SetActive(true);        

        monsterToRespawn.ResetMonster(); // 상태 및 HP 초기화 (내부에서 Warp 등 처리)
        activeMonsters.Add(monsterToRespawn);
    }

    /// <summary> 도망간 몬스터를 지정된 태그 위치 또는 원래 스폰 위치에서 재활성화 </summary>
    private void ReactivateFledMonster(FledMonsterInfo info)
    {
        if (info?.monster == null) return;

        Transform reactivatePoint = null;
        GameObject[] reactivationPoints = null;
        if (!string.IsNullOrEmpty(info.reactivationPointTag))
        {
            try { reactivationPoints = GameObject.FindGameObjectsWithTag(info.reactivationPointTag); }
            catch (UnityException) { Debug.LogWarning($"태그 '{info.reactivationPointTag}' 미정의."); }
        }

        if (reactivationPoints != null && reactivationPoints.Length > 0) // 지정 태그 위치 사용
            reactivatePoint = reactivationPoints[UnityEngine.Random.Range(0, reactivationPoints.Length)].transform;
        else // 태그 없거나 못 찾으면 원래 스폰 위치 사용
        {
            Debug.LogWarning($"재활성화 지점 태그 '{info.reactivationPointTag}' 없음/미발견. 원래 스폰 위치 사용.");
            reactivatePoint = info.monster.transform;
            reactivatePoint.position = info.monster.spawnPosition;
            reactivatePoint.rotation = Quaternion.identity;
        }

        info.monster.transform.position = reactivatePoint.position;
        info.monster.transform.rotation = reactivatePoint.rotation;
        info.monster.gameObject.SetActive(true);
        info.monster.ResetMonster();
        activeMonsters.Add(info.monster);
    }

    // --- 헬퍼 메서드 ---

    /// <summary> 스폰 포인트 배열에서 랜덤하게 하나 선택 </summary>
    private Transform GetRandomSpawnPoint(Transform[] points)
    {
        if (points != null && points.Length > 0)
        {
            // 비활성화된 스폰 포인트 제외 
            // var activePoints = points.Where(p => p != null && p.gameObject.activeInHierarchy).ToArray();
            // if (activePoints.Length > 0) return activePoints[UnityEngine.Random.Range(0, activePoints.Length)];
            // else return null; // 활성 스폰 포인트 없음

            // 간단히 배열에서 랜덤 선택
            return points[UnityEngine.Random.Range(0, points.Length)];
        }
        // Debug.LogWarning("GetRandomSpawnPoint: 사용할 수 있는 스폰 포인트가 없습니다.");
        return null; // ★ 스폰 포인트 없으면 null 반환하도록 수정
    }

    /// <summary>  Monster.ForceDespawnCorpse에서 호출 시 deadMonsters 리스트에서 즉시 제거 </summary>
    public void RemoveFromDeadList(Monster monster)
    {
        deadMonsters.RemoveAll(dm => dm.monster == monster);
    }
}
