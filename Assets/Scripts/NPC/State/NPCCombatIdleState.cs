using UnityEngine;
using System.Linq;
using ProjectRaid.Data;
using System.Collections.Generic;

public class NPCCombatIdleState : NPCBaseState
{
    private float _decisionTimer;
    private const float DECISION_INTERVAL = 1.0f;
    private Transform _currentTargetEnemy;

    // GameManager에 있는 NPCSkillUser 인스턴스를 참조하기 위한 변수
    private NPCSkillUser _centralSkillUser;

    public NPCCombatIdleState(NPCStateMachine stateMachine, NPCController npcController) : base(stateMachine, npcController) { }

    public override void Enter()
    {
        NpcController.RequestDrawWeapon();
        StopMovement();
        // if (Animator != null) // Animator는 NPCBaseState에 이미 null 체크된 참조가 있을 수 있음
        // {
        //     // Animator.Play("CombatIdle"); // 또는 Animator.SetBool("IsCombatIdling", true);
        //     Debug.Log($"[{NpcController.npcData.npcName}] CombatIdle 애니메이션 호출 시도 (주석 처리됨)");
        // }
        _decisionTimer = DECISION_INTERVAL;

        // ▼▼▼ GameManager에서 NPCSkillUser 인스턴스 가져오기 ▼▼▼
        if (GameManager.Instance != null)
        {
            _centralSkillUser = GameManager.Instance.GetComponent<NPCSkillUser>();
            if (_centralSkillUser == null)
            {
                Debug.LogError($"[{NpcController.npcData.npcName}] GameManager에 NPCSkillUser 컴포넌트를 찾을 수 없습니다!");
            }
        }
        else
        {
            Debug.LogError($"[{NpcController.npcData.npcName}] GameManager 인스턴스를 찾을 수 없습니다!");
        }
        // ▲▲▲ NPCSkillUser 인스턴스 가져오기 끝 ▲▲▲

        FindBestTarget();
    }

    public override void Execute()
    {
        if (NpcController.IsFainted)
        {
            StateMachine.ChangeState(NPCState.Fainted);
            return;
        }

        if (_currentTargetEnemy == null || !_currentTargetEnemy.gameObject.activeInHierarchy)
        {
            FindBestTarget();
            if (_currentTargetEnemy == null && !StateMachine.IsPlayerNearbyAndInCombat())
            {
                StateMachine.ChangeState(NPCState.FollowPlayer);
                Animator.SetTrigger("Sheathe");
                Debug.Log("Sheathe켜짐");
                return;
            }
        }

        if (_currentTargetEnemy != null)
        {
            LookAt(_currentTargetEnemy);
        }
        else
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player"); // PlayerController.Instance 등으로 변경 권장
            if (playerObj != null) LookAt(playerObj.transform);
            // Debug.Log("플레이어 바ㅏㅏㅏㅏㅏㅏㅏㅏㅏㅏㅏㅏㅏ"); // 이 로그는 불필요하면 제거
        }

        _decisionTimer -= Time.deltaTime;
        if (_decisionTimer <= 0f)
        {
            DecideNextAction();
            _decisionTimer = DECISION_INTERVAL * Random.Range(0.8f, 1.2f);
        }
    }

    private void DecideNextAction()
    {
        // Debug.Log("플레이어 찾기"); // 행동 결정 시작 로그

        if (_centralSkillUser == null) // NPCSkillUser 참조가 없으면 스킬 사용 불가
        {
            Debug.LogError($"[{NpcController.npcData.npcName}] NPCSkillUser 참조가 없어 행동 결정을 제대로 수행할 수 없습니다.");
            // 스킬 없이 기본 행동만 하도록 유도하거나, 에러 상태로 처리
            ProceedWithBasicActions(Object.FindObjectOfType<PlayerController>()); // PlayerController 참조 방식 개선 필요
            return;
        }

        // 1. 플레이어 힐 (조건은 프로젝트에 맞게 상세화 필요)
        PlayerController player = Object.FindObjectOfType<PlayerController>(); // PlayerController.Instance 등으로 변경 권장
        if (player != null && player.Stats.Runtime.CurrentHealth < player.Stats.Runtime.MaxHealth * 0.5f) // 예: HP 50% 미만
        {
            SkillData healSkill = GameManager.Instance.GetSkillByID("SK_Heal"); // 스킬 ID는 정확해야 함
            if (healSkill != null && _centralSkillUser.CanUseSkill(NpcController, healSkill, player.gameObject)) // ★★★ NpcController 전달 ★★★
            {
                StateMachine.SetNextSkill(healSkill, player.gameObject); // NPCStateMachine의 메서드
                StateMachine.ChangeState(NPCState.UsingSkill);
                 Debug.Log($"[{NpcController.npcData.npcName}] 플레이어 힐 사용: {healSkill.skillName}");
                return;
            }
        }

        // 2. 적 공격 (스킬 우선)
        if (_currentTargetEnemy != null)
        {
            SkillData attackSkill = SelectBestOffensiveSkill_Random(); // 이 함수 내부에서도 _centralSkillUser 사용

            if (attackSkill != null) // ★★★ 추가된 null 체크 ★★★
            {
                float npcColliderRadius = 0f;
                Collider npcCol = NpcController.GetComponent<Collider>(); // NPC 자신의 콜라이더
                if (npcCol != null) npcColliderRadius = NPCController.GetColliderRadius(npcCol); // 헬퍼 함수 사용

                float targetColliderRadius = 0f;
                Collider targetCol = _currentTargetEnemy.GetComponentInChildren<Collider>(true); // 타겟의 콜라이더 (자식 포함)
                if (targetCol == null) targetCol = _currentTargetEnemy.GetComponent<Collider>(); // 타겟 자체 콜라이더
                if (targetCol != null) targetColliderRadius = NPCController.GetColliderRadius(targetCol);

                float baseSkillRange = attackSkill.range; // SkillData에 정의된 기본 사거리
                float effectiveAttackRange = npcColliderRadius + targetColliderRadius + baseSkillRange;
                if (attackSkill != null && _centralSkillUser.CanUseSkill(NpcController, attackSkill, _currentTargetEnemy.gameObject)) // ★★★ NpcController 전달 ★★★
                {
                    float distanceToEnemy = Vector3.Distance(NpcController.transform.position, _currentTargetEnemy.position);
                    if (attackSkill.range > 0 && distanceToEnemy > effectiveAttackRange)
                    {
                        // ApproachingTarget 상태가 있다면 그 상태로 전환하고, 접근 후 사용할 스킬 정보 전달
                        // StateMachine.SetTargetToApproach(_currentTargetEnemy.position, attackSkill.range, attackSkill);
                        // StateMachine.ChangeState(NPCState.ApproachingTarget);
                        StartMovementTo(_currentTargetEnemy.position, NpcController.npcData.baseRunSpeed); // 임시: 직접 이동
                                                                                                           // Debug.Log($"[{NpcController.npcData.npcName}] 적에게 스킬 [{attackSkill.skillName}] 사용 위해 접근.");
                        return;
                    }
                    else
                    {
                        StateMachine.SetNextSkill(attackSkill, _currentTargetEnemy.gameObject);
                        StateMachine.ChangeState(NPCState.UsingSkill);
                        // Debug.Log($"[{NpcController.npcData.npcName}] 적에게 스킬 사용: {attackSkill.skillName}");
                        return;
                    }
                }
            }
            else // 사용할 공격 스킬이 없거나 쿨다운 중이면
            {
                ProceedWithBasicActions(player); // 기본 공격 또는 접근
                return;
            }
        }

        // 3. 할 일 없으면 플레이어 근처로 (기본 행동)
        ProceedWithBasicActions(player);
    }

    /// <summary>
    /// 사용할 공격 스킬이 없거나 쿨다운일 때 수행할 기본 행동 (적 접근 또는 플레이어에게 이동)
    /// </summary>
    private void ProceedWithBasicActions(PlayerController player)
    {
        if (_currentTargetEnemy != null) // 적이 있다면
        {
            float distanceToEnemy = Vector3.Distance(NpcController.transform.position, _currentTargetEnemy.position);
            float engageRange = NpcController.npcData.baseFollowRangeMin; // 또는 기본 공격 사거리
            if (distanceToEnemy > engageRange)
            {
                StartMovementTo(_currentTargetEnemy.position, NpcController.npcData.baseRunSpeed);
                // Debug.Log($"[{NpcController.npcData.npcName}] 스킬 없이 적에게 접근.");
            }
            else
            {
                StopMovement(); // 사거리 내면 다음 결정까지 대기
                // Debug.Log($"[{NpcController.npcData.npcName}] 적절한 교전 거리, 다음 행동 대기.");
            }
        }
        else if (player != null) // 적이 없고 플레이어가 있다면
        {
            float distanceToPlayer = GetDistanceToPlayer(); // NPCBaseState의 헬퍼 함수
            if (distanceToPlayer > NpcController.npcData.baseFollowRangeMax * 0.7f) // 플레이어와 너무 멀어지지 않도록
            {
                StartMovementTo(player.transform.position, NpcController.npcData.baseMoveSpeed);
                // Debug.Log($"[{NpcController.npcData.npcName}] 전투 중 할 일 없어 플레이어에게 이동.");
            }
            else
            {
                StopMovement(); // 플레이어 근처면 대기
            }
        }
    }


    /// <summary>
    /// 사용 가능한 공격 스킬 중 최적의 것을 선택합니다.
    /// </summary>
    private SkillData SelectBestOffensiveSkill_Random()
    {
        NPCSkillUser centralSkillUser = GameManager.Instance?.GetComponent<NPCSkillUser>();
        if (centralSkillUser == null || _currentTargetEnemy == null) return null;

        List<SkillData> usableOffensiveSkills = new List<SkillData>();
        // NPCController가 자신의 사용 가능 스킬 목록(해금된 것 포함)을 가지고 있다고 가정 (GetAvailableSkills() 등)
        // List<SkillData> allAvailableSkills = NpcController.GetAvailableSkills();
        // 여기서는 defaultSkillIDs만 사용한다고 가정
        if (NpcController.npcData.defaultSkillIDs != null)
        {
            foreach (var skillID in NpcController.npcData.defaultSkillIDs)
            {
                SkillData skill = GameManager.Instance.GetSkillByID(skillID);
                if (skill != null && skill.skillType == SkillType.Attack &&
                    centralSkillUser.CanUseSkill(NpcController, skill, _currentTargetEnemy.gameObject))
                {
                    usableOffensiveSkills.Add(skill);
                }
            }
        }
        // TODO: NPCAffinity 등으로 해금된 스킬도 usableOffensiveSkills에 추가

        if (usableOffensiveSkills.Count > 0)
        {
            return usableOffensiveSkills[Random.Range(0, usableOffensiveSkills.Count)];
        }
        return null;
    }

    // DetermineSkillTarget 함수는 이전과 거의 동일하게 유지하되, NPCController 컨텍스트가 필요하면 전달받도록 수정 가능.
    // 여기서는 _currentTargetEnemy를 직접 사용하므로 큰 변경은 필요 없어 보입니다.
    private GameObject DetermineSkillTarget(SkillData skill)
    {
        // ... (이전 코드와 거의 동일) ...
        if (skill == null) return null;
        switch (skill.targetType)
        {
            case SkillTargetType.Self: return NpcController.gameObject;
            case SkillTargetType.AllySingle: return GameObject.FindGameObjectWithTag("Player");
            case SkillTargetType.EnemySingle: return _currentTargetEnemy?.gameObject;
            default: return _currentTargetEnemy?.gameObject; // 기본적으로 현재 적 타겟
        }
    }


    private void FindBestTarget()
    {
        _currentTargetEnemy = null; // 타겟 초기화
        GameObject closestDamageableTarget = null;
        float minDistance = float.MaxValue;        
        int enemyLayerMask = LayerMask.GetMask("Enemy"); // "Enemy" 레이어 이름은 프로젝트에 맞게 수정
        if (enemyLayerMask == 0) // 레이어가 없거나 잘못된 경우
        {
            Debug.LogWarning($"[{NpcController.npcData.npcName}] 'Enemy' 레이어를 찾을 수 없습니다. FindBestTarget이 제대로 작동하지 않을 수 있습니다.");
            // 필요하다면 모든 콜라이더를 대상으로 하거나, 다른 방식으로 적을 찾아야 합니다.
            // 여기서는 일단 리턴하여 타겟을 찾지 않도록 합니다.
            return;
        }

        // NPCData에 정의된 감지 범위 사용
        float detectionRadius = NpcController.npcData.DetectionRange;
        Collider[] hitColliders = Physics.OverlapSphere(NpcController.transform.position, detectionRadius, enemyLayerMask);

        if (hitColliders == null || hitColliders.Length == 0)
        {
            //Debug.Log($"[{NpcController.npcData.npcName}] 감지 범위 내에 적('Enemy' 레이어) 없음.");
            return;
        }

        foreach (Collider hitCollider in hitColliders)
        {
            // 자기 자신은 타겟에서 제외
            if (hitCollider.gameObject == NpcController.gameObject)
            {
                continue;
            }

            // IDamageable 인터페이스를 가지고 있는지 확인
            IDamageable damageableEntity = hitCollider.GetComponentInChildren<IDamageable>();
            //IDamageable damageablEntity = hitCollider.GetComponentInParent<IDamageable>();
            // 또는 GetComponentInParent, GetComponentInChildren 등 상황에 맞게 사용

            if (damageableEntity != null) //&& damageablEntity!= null)
            {                
                Monster monsterTarget = hitCollider.GetComponent<Monster>(); // Monster 컴포넌트를 다시 가져와서 HP 확인
                Boss bossTarget = hitCollider.GetComponent<Boss>();
                if ((monsterTarget != null && monsterTarget.CurrentHp <= 0)||( bossTarget != null && bossTarget.bossHealth.hp <= 0))
                {
                    continue; // 이미 죽은 몬스터는 제외
                }                

                // 플레이어는 적으로 간주하지 않음 (만약 플레이어도 IDamageable을 구현하고 "Enemy" 레이어에 있다면)
                if (hitCollider.CompareTag("Player")) // "Player" 태그를 가진 오브젝트는 제외
                {
                    continue;
                }


                float distanceToTarget = Vector3.Distance(NpcController.transform.position, hitCollider.transform.position);

                if (distanceToTarget < minDistance)
                {
                    minDistance = distanceToTarget;
                    closestDamageableTarget = hitCollider.gameObject;
                }
            }
        }

        if (closestDamageableTarget != null)
        {
            _currentTargetEnemy = closestDamageableTarget.transform;
            Debug.Log($"[{NpcController.npcData.npcName}] 새로운 IDamageable 타겟 설정: {_currentTargetEnemy.name}");
        }
        else
        {
            Debug.Log($"[{NpcController.npcData.npcName}] 주변에 감지된 IDamageable 적 없음.");
        }

    }

    public override void Exit()
    {
        // StopAnimation("IsCombatIdling");
    }


    public override void OnDamaged(DamageInfo info)
    {        

        GameObject attacker = info.attacker;
        if (attacker != null && (_currentTargetEnemy == null || attacker.transform != _currentTargetEnemy))
        {
            // 현재 타겟보다 공격자가 더 가깝거나 특정 조건 만족 시 변경
            // 여기서는 간단히 공격자를 새 타겟 후보로 설정하고 FindBestTarget 호출
            // _currentTargetEnemy = attacker.transform; // 이렇게 직접 설정하거나,
            FindBestTarget(); // 또는 타겟 선정 로직을 다시 돌림 (공격자를 우선 고려하도록 수정 가능)
            Debug.Log($"[{NpcController.npcData.npcName}] 피격으로 인해 타겟 재탐색 시도. 공격자: {attacker.name}");
        }
    }
}
