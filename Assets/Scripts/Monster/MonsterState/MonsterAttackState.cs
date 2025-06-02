using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MonsterAttackState : MonsterBaseState
{
    private Transform currentTarget;
    private bool isPerformingAttackAction = false;
    private Coroutine currentAttackCoroutine = null;

    // ★ 각 AttackData 별 마지막 사용 시간 추적 (쿨다운 관리용) ★
    private Dictionary<AttackData, float> attackCooldownTimers = new Dictionary<AttackData, float>();

    private AttackData selectedAttackData = null; // 현재 실행하기로 결정된 공격 데이터

    public MonsterAttackState(Monster contextMonster) : base(contextMonster) { }

    public override void EnterState()
    {
        // Debug.Log($"[{monster.gameObject.name}] 공격 상태 진입");
        StopCurrentAttackCoroutine(); // 진행 중인 공격 코루틴 중지
        isPerformingAttackAction = false;
        selectedAttackData = null; // 선택된 공격 초기화
        currentTarget = monster.GetCurrentTarget();

        if (currentTarget == null)
        {
            // Debug.LogWarning($"[{monster.gameObject.name}] 공격 상태 진입 시 타겟 없음. Idle로 전환.");
            monster.ChangeState(MonsterState.Idle);
            return;
        }

        // 상태 진입 시 쿨다운 초기화 (선택적)
        // InitializeCooldowns();
    }

    // 쿨다운 딕셔너리 초기화 (필요 시 EnterState에서 호출)
    private void InitializeCooldowns()
    {
        attackCooldownTimers.Clear();
        if (monster.monsterData?.availableAttacks != null)
        {
            foreach (var attackData in monster.monsterData.availableAttacks)
            {
                if (attackData != null)
                {
                    attackCooldownTimers[attackData] = -Mathf.Infinity; // 즉시 사용 가능하도록 초기화
                }
            }
        }
    }

    public override void UpdateState()
    {
        // 1. 타겟 유효성 검사
        if (currentTarget == null || !monster.IsTargetInRange(currentTarget, monster.DetectionRange * 1.1f)) // 감지 범위 약간 벗어나도 추적 유지
        {
            // Debug.Log($"[{monster.gameObject.name}] 공격 상태 중 타겟 잃음. Idle로 전환.");
            monster.ClearTarget();
            monster.ChangeState(MonsterState.Idle);
            return;
        }

        // 2. 현재 공격 액션 실행 중이면 대기
        if (isPerformingAttackAction)
        {
            return;
        }

        // 3. 실행할 공격 결정 (selectedAttackData가 null일 때만)
        if (selectedAttackData == null)
        {
            selectedAttackData = DecideNextAttack(); // ★ 공격 결정 로직 호출 ★
        }

        // 4. 결정된 공격 실행 또는 추적
        if (selectedAttackData != null)
        {
            // 공격 범위 내에 있는지 다시 확인 (그 사이에 타겟이 움직였을 수 있음)
            float distance = Vector3.Distance(monster.transform.position, currentTarget.position);
            if (distance >= selectedAttackData.minRange && distance <= selectedAttackData.maxRange)
            {
                ExecuteAttack(selectedAttackData); // ★ 공격 실행 ★
            }
            else
            {
                // 공격 범위 벗어남 -> 추적 또는 다른 행동 고려
                // Debug.Log($"[{monster.gameObject.name}] 선택된 공격({selectedAttackData.attackName}) 범위 벗어남. 추적 시작.");
                monster.StartMovement(currentTarget.position);
                selectedAttackData = null; // 공격 선택 취소하고 다음 프레임에 다시 결정
            }
        }
        else
        {
            // 사용 가능한 공격 없음 -> 추적
            float distance = Vector3.Distance(monster.transform.position, currentTarget.position);
            if (distance > monster.EngagementDistance) // 교전 거리보다 멀면 추적
            {
                monster.StartMovement(currentTarget.position);
            }
            else // 교전 거리 내인데 공격할 게 없으면 대기 또는 특수 행동?
            {
                monster.StopMovement();
                monster.LookAtTarget(currentTarget);
                // TODO: 여기서 특수 행동(땅파기 등 AbilityAttackData) 결정 로직 추가 가능
            }
        }
    }

    /// <summary>
    /// 사용 가능한 공격 데이터 목록에서 현재 상황에 가장 적합한 공격을 선택합니다.
    /// </summary>
    /// <returns>선택된 AttackData 또는 null</returns>
    private AttackData DecideNextAttack()
    {
        if (monster.monsterData?.availableAttacks == null || monster.monsterData.availableAttacks.Count == 0)
        {
            return null; // 사용할 공격 없음
        }

        float distance = Vector3.Distance(monster.transform.position, currentTarget.position);
        List<AttackData> possibleAttacks = new List<AttackData>();
        float totalWeight = 0;

        // 사용 가능한 공격 필터링
        foreach (var attackData in monster.monsterData.availableAttacks)
        {
            if (attackData == null) continue;

            // 쿨다운 확인
            if (attackCooldownTimers.TryGetValue(attackData, out float lastUsedTime) && Time.time < lastUsedTime + attackData.cooldown)
            {
                continue; // 쿨다운 중
            }

            // 사거리 확인
            if (distance >= attackData.minRange && distance <= attackData.maxRange)
            {
                possibleAttacks.Add(attackData);
                totalWeight += attackData.decisionWeight; // 가중치 합산
            }
        }

        // 가중치 기반 랜덤 선택
        if (possibleAttacks.Count > 0)
        {
            float randomPoint = Random.Range(0, totalWeight);
            float currentWeightSum = 0;
            foreach (var attack in possibleAttacks)
            {
                currentWeightSum += attack.decisionWeight;
                if (randomPoint <= currentWeightSum)
                {
                    // Debug.Log($"[{monster.gameObject.name}] 다음 공격 결정: {attack.attackName}");
                    return attack; // 공격 선택
                }
            }
            // 이론상 여기에 도달하면 안 되지만, 안전하게 첫 번째 공격 반환
            return possibleAttacks[0];
        }

        return null; // 조건 맞는 공격 없음
    }

    /// <summary>
    /// 선택된 AttackData를 기반으로 공격을 실행합니다.
    /// </summary>
    private void ExecuteAttack(AttackData attackData)
    {
        if (isPerformingAttackAction) return;
        // Debug.Log($"[{monster.gameObject.name}] 공격 실행: {attackData.attackName}");

        isPerformingAttackAction = true;
        monster.StopMovement();
        monster.LookAtTarget(currentTarget);

        if (!string.IsNullOrEmpty(attackData.animationTriggerName))
        {
            monster.animator?.SetTrigger(attackData.animationTriggerName); // Animator의 SetTrigger 호출
        }
        else
        {
            Debug.LogWarning($"[{monster.gameObject.name}] 공격 ({attackData.attackName})에 애니메이션 트리거 이름이 없습니다. 공격 애니메이션이 실행되지 않을 수 있습니다.");
            // 애니메이션 트리거가 없으면 isPerformingAttackAction을 다시 false로 하거나,
            // PerformAttackAction 코루틴이 즉시 완료되도록 처리해야 할 수 있습니다.
            // (현재 코드는 attackDuration이 0이면 코루틴이 빠르게 종료됩니다.)
        }

        // 공격 실행 코루틴 시작
        StopCurrentAttackCoroutine();
        currentAttackCoroutine = monster.StartCoroutine(PerformAttackAction(attackData));
    }

    /// <summary>
    /// 실제 공격 로직 실행 및 후딜레이 처리 코루틴
    /// </summary>
    private IEnumerator PerformAttackAction(AttackData attackData)
    {
        currentTarget = monster.GetCurrentTarget(); // 최신 타겟 정보 가져오기
        bool isSpiderWebShot = monster is Spider &&
                           attackData is ProjectileAttackData projDataForSpiderCheck && // 타입 캐스팅과 함께 변수 선언
                           projDataForSpiderCheck.attackType == ProjectileAttackType.Web;         

        if (isSpiderWebShot)
        {
            // 거미 타입으로 캐스팅 후 LookAwayFromTarget 호출
            if (currentTarget != null && monster is Spider spider) // is 연산자로 타입 확인 및 캐스팅 동시 수행
            {
                
                //spider.LookAwayFromTarget(currentTarget); // ★ Spider의 메서드 호출 ★
                                                          // Debug.Log($"<color=magenta>Spider.LookAwayFromTarget 호출 직후 Rotation: {monster.transform.rotation.eulerAngles}</color>");
            }
        }
        else
            if (currentTarget != null)
        {
            monster.LookAtTarget(currentTarget); // 행동 시작 시 타겟 방향으로 회전
            Debug.Log($"<color=yellow>LookAtTarget 호출 직후 Rotation: {monster.transform.rotation.eulerAngles}</color>");
        }
        // 1. 데미지/효과 적용 딜레이 대기
        if (attackData.damageApplicationDelay > 0)
        {
            yield return new WaitForSeconds(attackData.damageApplicationDelay);
            Debug.Log($"<color=orange>Delay 후 Rotation: {monster.transform.rotation.eulerAngles}</color>");
        }

        // 2. 공격 타입에 따른 실제 로직 실행
        //if (attackData is MeleeAttackData meleeData)
        //{
        //    monster.DealDamageToTarget(meleeData.damage); // ★ Monster의 헬퍼 호출
        //    monster.animator.SetTrigger("Attack_02");
        //}
        //else if (attackData is ProjectileAttackData projData)
        //{
        //    monster.SpawnProjectileFromData(projData, currentTarget); // ★ Monster의 헬퍼 호출            
        //}
        //else if (attackData is AbilityAttackData abilityData)
        //{
        //    monster.ExecuteAbility(abilityData.abilityIdentifier); // ★ Monster의 헬퍼 호출
        //}
        // 다른 AttackData 타입 추가 가능

        // 3. 남은 공격 지속 시간(후딜레이) 대기
        float remainingDuration = attackData.attackDuration - attackData.damageApplicationDelay;
        if (remainingDuration > 0)
        {
            yield return new WaitForSeconds(remainingDuration);
        }

        // 4. 공격 완료 처리
        // Debug.Log($"[{monster.gameObject.name}] 공격 완료: {attackData.attackName}");
        attackCooldownTimers[attackData] = Time.time; // ★ 쿨다운 타이머 업데이트 ★
        isPerformingAttackAction = false;
        selectedAttackData = null; // 다음 행동 결정을 위해 초기화
        currentAttackCoroutine = null;
    }

    

    public override void ExitState()
    {
        // Debug.Log($"[{monster.gameObject.name}] 공격 상태 종료");
        StopCurrentAttackCoroutine();
        isPerformingAttackAction = false;
        selectedAttackData = null;
        monster.StopMovement(); // 상태 나갈 때 확실히 멈춤
    }

    public override void OnTakeDamage(DamageInfo info)
    {
        // 공격 중 피격 시 타겟은 부모가 갱신
        // Debug.Log($"[{monster.gameObject.name}] 공격 중 피격!");

        // 선택적: 특정 공격은 피격 시 취소될 수 있도록 구현
        // if (isPerformingAttackAction && selectedAttackData != null && selectedAttackData.canBeInterrupted)
        // {
        //     StopCurrentAttackCoroutine();
        //     isPerformingAttackAction = false;
        //     selectedAttackData = null;
        //     monster.animator?.SetTrigger("Hit"); // 피격 애니메이션 등
        //     // 상태를 Idle로 바꾸거나 잠시 경직 상태 추가 가능
        // }
    }

    private void StopCurrentAttackCoroutine()
    {
        if (currentAttackCoroutine != null)
        {
            monster.StopCoroutine(currentAttackCoroutine);
            currentAttackCoroutine = null;
        }
    }
}
