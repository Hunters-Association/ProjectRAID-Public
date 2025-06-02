using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MonsterIdleState : MonsterBaseState
{
    private enum IdlePattern { Breathe = 1, Wander = 2, Jump = 3 } // ID 3을 점프/특수동작으로 가정
    private IdlePattern currentPatternID;
    private Coroutine patternCoroutine;
    private float pacifiedCooldownTimer = 0f; // ★ 온순화 쿨다운 타이머

    private float currentWanderDropChance;
    private GameObject currentWanderItemPrefab;
    private float currentWanderDropCheckInterval;
    public MonsterIdleState(Monster contextMonster) : base(contextMonster) { }

    public override void EnterState()
    {
        monster.StopMovement();
        if (patternCoroutine != null)
        {
            monster.StopCoroutine(patternCoroutine);
            patternCoroutine = null;
        }
        if (monster.monsterData != null)
        {
            currentWanderDropChance = monster.monsterData.wanderDropChance;
            currentWanderItemPrefab = monster.monsterData.wanderItemPrefab;
            currentWanderDropCheckInterval = monster.monsterData.wanderDropCheckInterval;
            // 체크 주기가 0 이하면 오류 방지 위해 기본값 설정
            if (currentWanderDropCheckInterval <= 0) currentWanderDropCheckInterval = 2.0f;
        }
        else // MonsterData 없으면 기본값 사용 또는 비활성화
        {
            currentWanderDropChance = 0f; // 드랍 안 함
            currentWanderItemPrefab = null;
            currentWanderDropCheckInterval = 999f; // 체크 안 함
            Debug.LogWarning("MonsterData가 없어 배회 중 아이템 드랍 비활성화됨.");
        }
        
        if (monster.IsPacified) // ★ 온순화 상태 진입
        {
            float cooldownDuration = monster.monsterData?.fleeReactivationTime ?? 60f;
            pacifiedCooldownTimer = cooldownDuration;
            Debug.Log($"[Idle EnterState] 온순화 상태 감지. 쿨다운 시작: {cooldownDuration}초");
        }
        else // 일반 Idle 상태 진입
        {
            pacifiedCooldownTimer = 0f;
            SelectAndExecuteNewIdlePattern();
        }
    }

    public override void UpdateState()
    {
        if (monster.IsPacified) // ★ 온순화 상태일 때 처리
        {
            if (pacifiedCooldownTimer > 0)
            {
                pacifiedCooldownTimer -= Time.deltaTime;

                // --- ★ 점진적 HP 회복 ---
                float regenRate = monster.monsterData?.pacifiedHealthRegenRate ?? 1.0f;
                monster.Heal(regenRate * Time.deltaTime); // 초당 회복량 적용
                

                // 쿨다운 종료 체크
                if (pacifiedCooldownTimer <= 0)
                {
                    Debug.Log($"[Idle Update] 온순화 쿨다운 종료. ResetPacifiedState 호출.");
                    monster.ResetPacifiedState(); // 온순화 상태만 해제 (HP는 이미 회복됨)
                    // 즉시 일반 Idle 패턴 시작
                    SelectAndExecuteNewIdlePattern();
                }
            }
            // 쿨다운 중에는 다른 행동 안 하고 HP만 회복
            return; // 아래 일반 Idle 로직 실행 방지
        }

        // --- 일반 Idle 상태 로직 (isPacified == false) ---
        if (monster.monsterData?.monsterType != MonsterBehaviorType.Passive && // Passive 제외
            !(monster is Squirrel) && // ★ Squirrel 타입 제외 ★
            monster.DetectPlayer())   // 플레이어 감지
        {
            // Debug.Log($"{monster.name}(Type:{monster.monsterData.monsterType}) 플레이어 감지! Attack 상태로 전환.");
            monster.ChangeState(MonsterState.Attack); // Attack 상태로 전환
            return; // 상태 변경 후 업데이트 종료
        }
        // 패턴 코루틴이 끝나면 자동으로 다음 패턴 시작 (코루틴 내에서 처리)
    }

    public override void ExitState()
    {
        if (patternCoroutine != null)
        {
            monster.StopCoroutine(patternCoroutine);
            patternCoroutine = null;
        }
    }

    private void SelectAndExecuteNewIdlePattern()
    {
        if (monster.IsPacified) return; // 안전 코드
        if (patternCoroutine != null) monster.StopCoroutine(patternCoroutine);
        // ... (확률 기반 패턴 ID 선택: Breathe=1, Wander=2, Jump=3 가정) ...
        float p1 = 50f, p2 = 40f, p3 = 10f;
        if (monster.monsterData != null)
        {
            p1 = monster.monsterData.idlePattern1_Chance;
            p2 = monster.monsterData.idlePattern2_Chance;
            p3 = monster.monsterData.idlePattern3_Chance;
        }
        float total = p1 + p2 + p3; if (total <= 0)
        {
            currentPatternID = (IdlePattern)1;
        }
        else
        {
            float rand = Random.Range(0f, total);
            if (rand < p1) currentPatternID = IdlePattern.Breathe;
            else if (rand < p1 + p2) currentPatternID = IdlePattern.Wander;
            else currentPatternID = IdlePattern.Jump;
        }

        patternCoroutine = monster.StartCoroutine(ExecutePattern(currentPatternID));
    }

    private IEnumerator ExecutePattern(IdlePattern patternId)
    {
        if (monster.IsPacified) yield break; // 시작 시 온순화 체크

        float duration;
        switch (patternId)
        {
            case IdlePattern.Breathe:
                duration = Random.Range(3f, 6f);
                monster.StopMovement();
                yield return new WaitForSeconds(duration);
                break;
            case IdlePattern.Wander:
                duration = Random.Range(5f, 10f); float timer = 0f;
                float nextDropCheckTime = Time.time + currentWanderDropCheckInterval;
                if (monster.GetWanderPositionNavMesh(monster.WanderRadius, out Vector3 wanderPos))
                {
                    monster.StartMovement(wanderPos);
                    while (timer < duration && !monster.HasReachedDestination())
                    {
                        if (monster.IsPacified) yield break;
                        timer += Time.deltaTime;
                        if (Time.time >= nextDropCheckTime)
                        {
                            // ★ 상태 변수에 저장된 값 사용 ★
                            TryDropItemDuringWander(currentWanderDropChance, currentWanderItemPrefab);
                            nextDropCheckTime = Time.time + currentWanderDropCheckInterval; // 다음 체크 시간 갱신
                        }
                        yield return null;
                    }
                    monster.StopMovement();
                }
                else yield return new WaitForSeconds(1f); break;
            case IdlePattern.Jump: // ★ ID 3: 점프 (애니메이션 없이 시간만)
                duration = Random.Range(1f, 2f);
                monster.StopMovement();
                // TODO: 실제 점프 이동 로직 (선택적)
                yield return new WaitForSeconds(duration);
                break;
            default:
                yield return new WaitForSeconds(1f);
                break;
        }
        patternCoroutine = null;
        if (monster.CurrentStateEnum == MonsterState.Idle && !monster.IsPacified) // ★ 완료 후 온순화 아닐 때만 다음 패턴
            SelectAndExecuteNewIdlePattern();
    }
    /// 배회 중 설정된 확률과 아이템 프리팹으로 아이템을 드랍하는 함수
    /// </summary>
    private void TryDropItemDuringWander(float dropChance, GameObject itemPrefab) // ★ 파라미터 받도록 수정됨
    {
        if (Random.Range(0f, 100f) <= dropChance) // 확률 체크
        {
            if (itemPrefab != null) // 프리팹 유효성 확인
            {
                // 스폰 위치 결정 (몬스터 발밑 근처)
                Vector3 spawnPos = monster.transform.position + Vector3.down * 0.1f + (Vector3)Random.insideUnitCircle * 0.3f;
                if (NavMesh.SamplePosition(spawnPos, out NavMeshHit hit, 1.0f, NavMesh.AllAreas)) spawnPos = hit.position; // NavMesh 보정

                // 아이템 생성
                GameObject droppedItem = Object.Instantiate(itemPrefab, spawnPos, Quaternion.identity);
                // Debug.Log($"{monster.monsterData?.monsterName ?? "몬스터"} 배회 중 아이템 드랍! ({itemPrefab.name})");

            }

        }
    }

    public override void OnTakeDamage(DamageInfo info)
    {
        Debug.Log($"{monster.gameObject.name} took damage while Idle. Attacker: {info.attacker?.name}. Changing state to Chase.");
        monster.EvaluateNewAttacker(info.attacker); // 공격자를 타겟으로 설정
        monster.ChangeState(MonsterState.Attack);
    }
}
