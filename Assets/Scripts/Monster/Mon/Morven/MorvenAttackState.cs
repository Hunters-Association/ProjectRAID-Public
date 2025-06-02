using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MorvenAttackState : MonsterBaseState
{
    private Morven morven; // Morven 타입으로 캐스팅
    private Transform currentTarget;
    private bool isPerformingAttackAction = false;
    private Coroutine currentAttackCoroutine = null;

    // 각 AttackData 별 마지막 사용 시간 추적 (쿨다운 관리용)
    private Dictionary<AttackData, float> attackCooldownTimers = new Dictionary<AttackData, float>();

    private AttackData selectedAttackData = null; // 현재 실행하기로 결정된 공격 데이터
    private float burrowDelayAfterSpawn = 5.0f;
    private ProjectileAttackData projectileAttackData = null;
    private MeleeAttackData meleeAttackData = null;
    public MorvenAttackState(Monster contextMonster) : base(contextMonster)
    {
        // Morven 타입으로 캐스팅, 실패 시 오류 로깅
        morven = contextMonster as Morven;
        if (morven == null)
        {
            Debug.LogError("MorvenAttackState가 Morven 타입이 아닌 몬스터로 생성되었습니다!", contextMonster);
        }
    }

    public override void EnterState()
    {
        // Debug.Log($"[{morven?.gameObject.name}] 공격 상태 진입");
        StopCurrentAttackCoroutine();
        isPerformingAttackAction = false;
        selectedAttackData = null;
        currentTarget = morven?.GetCurrentTarget(); // Null-conditional operator 사용

        if (currentTarget == null)
        {
            // Debug.LogWarning($"[{morven?.gameObject.name}] 공격 상태 진입 시 타겟 없음. Idle로 전환.");
            morven?.ChangeState(MonsterState.Idle); // Null-conditional operator 사용
            return;
        }
        // 쿨다운 초기화 (상태 진입 시마다 할 수도 있고, 한 번만 할 수도 있음)
        InitializeCooldowns();
    }

    // 쿨다운 딕셔너리 초기화
    private void InitializeCooldowns()
    {
        attackCooldownTimers.Clear();
        // MonsterData 및 availableAttacks null 체크 강화
        if (morven?.monsterData?.availableAttacks != null)
        {
            foreach (var attackData in morven.monsterData.availableAttacks)
            {
                if (attackData != null && !attackCooldownTimers.ContainsKey(attackData)) // 중복 추가 방지
                {
                    // 처음에는 즉시 사용 가능하도록 과거 시간으로 설정
                    attackCooldownTimers[attackData] = -Mathf.Infinity;
                }
            }
        }
    }

    public override void UpdateState()
    {
        if (morven == null) return; // 안전 코드

        // 1. 타겟 유효성 검사
        if (currentTarget == null || !morven.IsTargetInRange(currentTarget, morven.DetectionRange * 1.1f))
        {
            // Debug.Log($"[{morven.gameObject.name}] 공격 상태 중 타겟 잃음. Idle로 전환.");
            morven.ClearTarget();
            morven.ChangeState(MonsterState.Idle);
            return;
        }

        // 2. 현재 공격 액션 실행 중이면 대기
        if (isPerformingAttackAction)
        {
            return;
        }

        // 3. 실행할 공격 결정 (아직 결정되지 않았다면)
        if (selectedAttackData == null)
        {
            selectedAttackData = DecideNextAttack(); // ★ AttackData 기반 결정 로직 ★
        }

        // 4. 결정된 공격 실행 또는 추적/대기
        if (selectedAttackData != null)
        {
            float distance = Vector3.Distance(morven.transform.position, currentTarget.position);
            // 선택된 공격의 범위 재확인
            if (distance >= selectedAttackData.minRange && distance <= selectedAttackData.maxRange)
            {
                ExecuteAttack(selectedAttackData); // ★ AttackData 기반 실행 로직 ★
            }
            else
            {
                // 범위 벗어남: 추적
                // Debug.Log($"[{morven.gameObject.name}] 선택된 공격({selectedAttackData.attackName}) 범위 벗어남 ({distance:F1}m). 추적 시작.");
                morven.StartMovement(currentTarget.position);
                selectedAttackData = null; // 공격 취소, 다음 프레임 재결정
            }
        }
        else
        {
            // 사용 가능한 공격 없음: 추적 또는 대기
            float distance = Vector3.Distance(morven.transform.position, currentTarget.position);
            // engagementDistance는 MonsterData에서 가져옴
            if (distance > morven.EngagementDistance)
            {
                morven.StartMovement(currentTarget.position); // 교전 거리 밖이면 추적
            }
            else
            {
                morven.StopMovement(); // 교전 거리 안이면 대기
                morven.LookAtTarget(currentTarget);
                // TODO: 여기서 땅파기(Burrow) Ability 사용 결정 로직 추가 가능
                // 예: if (CanUseBurrowAbility()) { ExecuteBurrowAbility(); }
            }
        }
    }

    /// <summary>
    /// 사용 가능한 AttackData 목록에서 현재 상황에 맞는 최적의 공격/능력을 선택합니다.
    /// </summary>
    private AttackData DecideNextAttack()
    {
        if (morven?.monsterData?.availableAttacks == null || morven.monsterData.availableAttacks.Count == 0 || currentTarget == null)
        {
            return null;
        }

        float distance = Vector3.Distance(morven.transform.position, currentTarget.position);
        List<AttackData> possibleActions = new List<AttackData>(); // 이제 일반 공격과 능력을 모두 담음
        float totalWeight = 0;

        // 사용 가능한 모든 공격/능력 데이터 순회
        foreach (var attackData in morven.monsterData.availableAttacks)
        {
            if (attackData == null) continue;

            // 쿨다운 체크
            if (attackCooldownTimers.TryGetValue(attackData, out float lastUsedTime) && Time.time < lastUsedTime + attackData.cooldown)
            {
                continue; // 쿨다운 중
            }

            // ★★★ 땅파기(Ability) 인지 일반 공격인지 구분하여 조건 체크 ★★★
            if (attackData is AbilityAttackData abilityData && abilityData.abilityIdentifier == "Burrow")
            {
                // --- 땅파기 조건 체크 ---
                // 1. 리스폰 후 시간 지연 체크
                float timeSinceActive = Time.time - monster.LastActivationTime;
                if (timeSinceActive < burrowDelayAfterSpawn)
                {
                    continue; // 지연 시간 안 지났으면 땅파기 불가
                }

                // 2. 스폰 지점 거리 체크
                float allowedRadius = monster.BurrowAllowedSpawnRadius;
                bool canBurrowNearSpawn = (allowedRadius <= 0) ||
                                          ((monster.transform.position - monster.GetSpawnPosition()).sqrMagnitude <= allowedRadius * allowedRadius);

                if (canBurrowNearSpawn) // 거리 제한 통과
                {
                    // 3. 추가 조건 (선택적): 거리가 멀 때만 땅 파기?
                    // if (distance > morven.EngagementDistance) // 예: 교전 거리 밖일 때만
                    // {
                    possibleActions.Add(attackData); // 땅파기 가능 목록에 추가
                    totalWeight += attackData.decisionWeight;
                    // }
                }
                // --- 땅파기 조건 체크 끝 ---
                
            }
            else // 일반 공격 (Melee, Projectile 등)
            {
                // 사거리 체크
                if (distance >= attackData.minRange && distance <= attackData.maxRange)
                {                    
                    possibleActions.Add(attackData);
                    totalWeight += attackData.decisionWeight;                    
                }
            }
            // ★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★
        }

        // 가능한 행동 중 가중치 기반 랜덤 선택
        if (possibleActions.Count > 0)
        {
            if (totalWeight <= 0) return possibleActions[0];
            float randomPoint = Random.Range(0, totalWeight);
            float currentWeightSum = 0;
            foreach (var action in possibleActions)
            {
                currentWeightSum += action.decisionWeight;
                if (randomPoint <= currentWeightSum)
                {
                    // Debug.Log($"[{morven.gameObject.name}] 다음 행동 결정: {action.attackName}");
                    return action;
                }
            }
            return possibleActions[Random.Range(0, possibleActions.Count)]; // 안전 코드
        }

        return null; // 조건 맞는 행동 없음
    }

    /// <summary>
    /// 선택된 AttackData를 기반으로 실제 공격/능력을 실행합니다.
    /// </summary>
    private void ExecuteAttack(AttackData attackData)
    {
        if (isPerformingAttackAction || morven == null) return; // 실행 중이거나 morven 참조 없으면 중단
        // Debug.Log($"[{morven.gameObject.name}] 행동 실행: {attackData.attackName}");

        isPerformingAttackAction = true;
        morven.StopMovement(); // 행동 전 이동 멈춤
        morven.LookAtTarget(currentTarget); // 타겟 바라보기

        // 애니메이션 트리거 (AttackData에 이름이 지정된 경우)
        if (!string.IsNullOrEmpty(attackData.animationTriggerName))
        {
            morven.animator?.SetTrigger(attackData.animationTriggerName);
        }

        // 공격/능력 실행 코루틴 시작
        StopCurrentAttackCoroutine();
        currentAttackCoroutine = morven.StartCoroutine(PerformAttackAction(attackData));
    }

    /// <summary>
    /// 실제 공격/능력 로직 실행 및 후딜레이(지속 시간) 처리 코루틴
    /// </summary>
    private IEnumerator PerformAttackAction(AttackData attackData)
    {
        // 1. 실제 효과 적용까지의 딜레이 (선딜레이)
        if (attackData.damageApplicationDelay > 0)
        {
            yield return new WaitForSeconds(attackData.damageApplicationDelay);
        }

        // 2. AttackData 타입에 따라 적절한 Monster 헬퍼 메서드 호출
        if (attackData is MeleeAttackData meleeData)
        {
            monster.animator.SetTrigger("IsAttacking");
            //morven.DealDamageToTarget(meleeData.damage);
            // 근접 공격 데미지 전달
            meleeAttackData = meleeData;
                        
        }
        else if (attackData is ProjectileAttackData projData)
        {
            monster.animator.SetTrigger("IsMAttacking");
            //
            // 투사체 생성 요청
            projectileAttackData = projData;
        }
        else if (attackData is AbilityAttackData abilityData)
        {
            morven.ExecuteAbility(abilityData.abilityIdentifier);
            monster.animator.SetTrigger("IsB");// 특정 능력 실행 요청
        }
        // ... 다른 AttackData 타입 처리 ...

        // 3. 남은 공격 지속 시간 (후딜레이) 대기
        float remainingDuration = attackData.attackDuration - attackData.damageApplicationDelay;
        if (remainingDuration > 0)
        {
            yield return new WaitForSeconds(remainingDuration);
        }

        // 4. 행동 완료 처리
        // Debug.Log($"[{morven.gameObject.name}] 행동 완료: {attackData.attackName}");
        attackCooldownTimers[attackData] = Time.time; // ★ 쿨다운 타이머 업데이트 ★
        isPerformingAttackAction = false;
        selectedAttackData = null; // 다음 행동 결정을 위해 초기화
        currentAttackCoroutine = null;
    }

    public override void ExitState()
    {
        // Debug.Log($"[{morven?.gameObject.name}] 공격 상태 종료");
        StopCurrentAttackCoroutine();
        isPerformingAttackAction = false;
        selectedAttackData = null;
        morven?.StopMovement(); // 상태 나갈 때 멈춤
    }

    public override void OnTakeDamage(DamageInfo info)
    {
        if (morven == null) return;
        // 공격 중 피격 시 특별한 반응 로직 (예: 경직, 패턴 취소) 추가 가능
        // Debug.Log($"[{morven.gameObject.name}] 공격 중 피격!");

        // 예시: 피격 시 25% 확률로 땅파기 시도 (Burrow 능력이 AttackData에 포함되어 있어야 함)
        /*
        var burrowAbility = morven.monsterData?.availableAttacks
                                .OfType<AbilityAttackData>()
                                .FirstOrDefault(a => a.abilityIdentifier == "Burrow");
        if (burrowAbility != null && Random.Range(0f, 1f) < 0.25f)
        {
            // 쿨다운 체크 등 추가 조건 확인 후
            if (!attackCooldownTimers.TryGetValue(burrowAbility, out float lastUsed) || Time.time >= lastUsed + burrowAbility.cooldown)
            {
                Debug.Log($"[{morven.gameObject.name}] 피격 후 땅파기 시도!");
                StopCurrentAttackCoroutine(); // 현재 공격 중단
                isPerformingAttackAction = false;
                selectedAttackData = null;
                ExecuteAttack(burrowAbility); // 땅파기 능력 즉시 실행
                return; // 추가 처리 방지
            }
        }
        */
        // 타겟 갱신은 Monster.TakeDamage에서 처리됨
    }

    // 코루틴 중지 헬퍼
    private void StopCurrentAttackCoroutine()
    {
        if (currentAttackCoroutine != null && morven != null)
        {
            morven.StopCoroutine(currentAttackCoroutine);
            currentAttackCoroutine = null;
        }
    }
    public void OnProjectileHook()
    {
        if(projectileAttackData != null)
        {
            morven.SpawnProjectileFromData(projectileAttackData, currentTarget);
        }
    }
    public void OnMeleeAttackHook()
    {
        if (meleeAttackData != null)
        {
            morven.DealDamageToTarget(meleeAttackData.damage);
        }
    }
}
