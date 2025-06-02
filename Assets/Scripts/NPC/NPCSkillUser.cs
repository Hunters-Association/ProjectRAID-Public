using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ProjectRaid.Extensions;

public class NPCSkillUser : MonoBehaviour
{
    // ▼▼▼ 딕셔너리 키를 NPC ID (int)로 변경 ▼▼▼
    private Dictionary<int, Dictionary<SkillData, float>> _npcIdSkillCooldowns = new();
    private Dictionary<int, List<SkillData>> _npcIdAvailableSkills = new();
    // ▲▲▲ 변경 끝 ▲▲▲

    public void RegisterAndInitializeNpc(NPCController npc)
    {
        if (npc == null || npc.npcData == null)
        {
            Debug.LogError("[NPCSkillUser Central] RegisterAndInitializeNpc: NPC 또는 NPCData가 null입니다.");
            return;
        }
        int npcID = npc.npcData.npcID; // ★★★ NPC ID 사용 ★★★

        // Debug.LogWarning($"[NPCSkillUser Central] !!!!! RegisterAndInitializeNpc 호출 시작 !!!!! NPC: {npc.npcData.npcName}, NPC_ID: {npcID}");

        if (!_npcIdSkillCooldowns.TryGetValue(npcID, out Dictionary<SkillData, float> cooldownsForThisNpc)) // ★★★ npcID로 조회 ★★★
        {
            cooldownsForThisNpc = new Dictionary<SkillData, float>();
            _npcIdSkillCooldowns.Add(npcID, cooldownsForThisNpc); // ★★★ npcID로 추가 ★★★
            // Debug.LogWarning($"[NPCSkillUser Central] NPC_ID [{npcID}] ({npc.npcData.npcName})의 쿨다운 딕셔너리 새로 생성됨.");
        }
        else
        {
            // Debug.LogWarning($"[NPCSkillUser Central] NPC_ID [{npcID}] ({npc.npcData.npcName})의 기존 쿨다운 딕셔너리 사용. 현재 스킬 수: {cooldownsForThisNpc.Count}");
        }

        if (!_npcIdAvailableSkills.TryGetValue(npcID, out List<SkillData> availableSkillsForThisNpc)) // ★★★ npcID로 조회 ★★★
        {
            availableSkillsForThisNpc = new List<SkillData>();
            _npcIdAvailableSkills.Add(npcID, availableSkillsForThisNpc); // ★★★ npcID로 추가 ★★★
            // Debug.LogWarning($"[NPCSkillUser Central] NPC_ID [{npcID}] ({npc.npcData.npcName})의 사용 가능 스킬 목록 새로 생성.");
        }
        bool hadExistingAvailableSkills = availableSkillsForThisNpc.Count > 0;
        // availableSkillsForThisNpc.Clear();
        // if (hadExistingAvailableSkills) Debug.LogWarning($"[NPCSkillUser Central] NPC_ID [{npcID}] ({npc.npcData.npcName})의 기존 사용 가능 스킬 목록 초기화됨 (Clear 호출).");


        if (npc.npcData.defaultSkillIDs != null && npc.npcData.defaultSkillIDs.Count > 0)
        {
            // Debug.LogWarning($"[NPCSkillUser Central RegisterAndInitializeNpc] NPC_ID [{npcID}] ({npc.npcData.npcName}) defaultSkillIDs 개수: {npc.npcData.defaultSkillIDs.Count}");
            foreach (string skillDataID_from_NPCData in npc.npcData.defaultSkillIDs) // 변수명 명확히
            {
                SkillData skill = GameManager.Instance.GetSkillByID(skillDataID_from_NPCData);
                if (skill != null)
                {
                    if (!availableSkillsForThisNpc.Contains(skill))
                    {
                        availableSkillsForThisNpc.Add(skill);
                    }
                    if (!cooldownsForThisNpc.ContainsKey(skill))
                    {
                        cooldownsForThisNpc.Add(skill, 0f);
                        // Debug.LogWarning($"[NPCSkillUser Central RegisterAndInitializeNpc] NPC_ID [{npcID}] 스킬 [{skill.skillName}] cooldownsForThisNpc에 *새로 추가됨* (쿨다운 ).");
                    }
                    else
                    {
                        // Debug.LogWarning($"[NPCSkillUser Central RegisterAndInitializeNpc] NPC_ID [{npcID}] 스킬 [{skill.skillName}]은 이미 cooldownsForThisNpc에 존재. 기존 쿨다운 값 [{cooldownsForThisNpc[skill]:F2}] 유지.");
                    }
                }
                else
                {
                    Debug.LogWarning($"[NPCSkillUser Central RegisterAndInitializeNpc] Default Skill ID [{skillDataID_from_NPCData}]를 SkillDatabase에서 찾을 수 없습니다.");
                }
            }
        }
        else
        {
            Debug.LogWarning($"[NPCSkillUser Central RegisterAndInitializeNpc] NPC_ID [{npcID}] ({npc.npcData.npcName})의 defaultSkillIDs가 비어있거나 null입니다.");
        }
        // Debug.LogWarning($"[NPCSkillUser Central] !!!!! RegisterAndInitializeNpc 호출 완료 !!!!! NPC_ID: {npcID} ({npc.npcData.npcName}), 최종 해당 NPC의 스킬 수: {cooldownsForThisNpc.Count}, 사용 가능 스킬 수: {availableSkillsForThisNpc.Count}");
    }

    public void UnregisterNpc(NPCController npc)
    {
        if (npc == null || npc.npcData == null) return;
        int npcID = npc.npcData.npcID;
        // Debug.LogError($"[NPCSkillUser Central] !!!!! UnregisterNpc 호출 !!!!! NPC_ID: {npcID} ({npc.npcData.npcName}), 호출 스택: {new System.Diagnostics.StackTrace(true)}");
        _npcIdSkillCooldowns.Remove(npcID);
        _npcIdAvailableSkills.Remove(npcID);
    }
    public void ClearAllNpcCooldownsForDebug() // 이런 함수가 있다면 호출 지점 확인
    {
        // Debug.LogWarning($"[NPCSkillUser Central] !!!!! ClearAllNpcCooldownsForDebug 호출 !!!!! 호출 스택: {new System.Diagnostics.StackTrace(true)}");
        _npcIdSkillCooldowns.Clear();
        _npcIdAvailableSkills.Clear(); // 스킬 목록도 함께?
    }

    void Update()
    {
        if (_npcIdSkillCooldowns == null || _npcIdSkillCooldowns.Count == 0) return;

        foreach (int npcID in _npcIdSkillCooldowns.Keys.ToList()) // ★★★ npcID로 순회 ★★★
        {
            if (!_npcIdSkillCooldowns.ContainsKey(npcID)) continue; // 방어 코드

            Dictionary<SkillData, float> cooldownsForThisNpc = _npcIdSkillCooldowns[npcID];
            List<SkillData> skillsToUpdate = new(cooldownsForThisNpc.Keys);
            // bool cooldownLoggedForThisNpcThisFrame = false; // 필요시 디버그 로그용 플래그

            foreach (SkillData skill in skillsToUpdate)
            {
                if (skill == null) continue;
                if (cooldownsForThisNpc.TryGetValue(skill, out float currentCooldown))
                {
                    if (currentCooldown > 0)
                    {
                        float newCooldown = currentCooldown - Time.deltaTime;
                        cooldownsForThisNpc[skill] = Mathf.Max(0f, newCooldown);

                        // 특정 NPC의 특정 스킬 로그 (예: "리첼"의 "공격스킬")
                        // if (GameManager.Instance.GetNPCByID(npcID)?.npcData.npcName == "리첼" && skill.skillName == "공격스킬" && !cooldownLoggedForThisNpcThisFrame)
                        // {
                        //    Debug.Log($"[NPCSkillUser Update Central] NPC_ID [{npcID}] 스킬 [{skill.skillName}] 쿨다운 감소: {currentCooldown:F2} -> {cooldownsForThisNpc[skill]:F2}");
                        //    cooldownLoggedForThisNpcThisFrame = true;
                        // }
                    }
                }
            }
        }
    }

    public bool CanUseSkill(NPCController npc, SkillData skill, GameObject target = null)
    {
        if (npc == null || npc.npcData == null || skill == null || npc.IsFainted) return false;
        int npcID = npc.npcData.npcID;
        if (!_npcIdAvailableSkills.TryGetValue(npcID, out var availableSkills) || !availableSkills.Contains(skill)) return false;
        if (_npcIdSkillCooldowns.TryGetValue(npcID, out var cooldownsForThisNpc))
        {
            if (cooldownsForThisNpc.TryGetValue(skill, out float currentCooldownValue) && currentCooldownValue > 0) return false;
        }
        return true;
    }

    public bool TryUseSkill(NPCController npc, SkillData skill, GameObject target = null)
    {
        if (!CanUseSkill(npc, skill, target)) return false;
        if (npc == null) return false;

        npc.PrepareForSkillAnimationEvent(skill, target);

        // ▼▼▼ 애니메이션 파라미터 설정 로직 ▼▼▼
        if (npc.npcAnimator != null && skill.animatorParametersToSet != null)
        {
            Debug.Log($"[NPCSkillUser Central] NPC [{npc.npcData.npcName}] 스킬 [{skill.skillName}] 사용. 애니메이션 파라미터 설정 시작...");
            foreach (var paramSetter in skill.animatorParametersToSet)
            {
                if (string.IsNullOrEmpty(paramSetter.parameterName)) continue;

                try // Animator에 해당 파라미터가 없을 경우를 대비한 예외 처리
                {
                    switch (paramSetter.parameterType)
                    {
                        case AnimatorParameterSetter.ParamType.Bool:
                            npc.npcAnimator.SetBool(paramSetter.parameterName, paramSetter.boolValue);
                            Debug.Log($"  - Bool Param: {paramSetter.parameterName} = {paramSetter.boolValue}");
                            break;
                        case AnimatorParameterSetter.ParamType.Int:
                            npc.npcAnimator.SetInteger(paramSetter.parameterName, paramSetter.intValue);
                            Debug.Log($"  - Int Param: {paramSetter.parameterName} = {paramSetter.intValue}");
                            if (paramSetter.parameterName == "SkillNum")
                            {
                                // 다음 프레임에 반영될 수 있으므로, 현재 프레임에서는 Set한 값이 바로 Get으로 안 나올 수 있습니다.
                                // 하지만 디버깅 목적으로 현재 상태를 확인합니다.
                                // 더 확실한 확인은 Animator 창을 직접 보거나, 다음 프레임에 Get 하는 것입니다.
                                int actualSkillNum = npc.npcAnimator.GetInteger("SkillNum");
                                // Debug.LogError($"  - Int Param 'SkillNum' GET AFTER SET: Animator 현재 값 = {actualSkillNum} (설정 시도 값: {paramSetter.intValue})");
                            }
                            break;
                        case AnimatorParameterSetter.ParamType.Float:
                            npc.npcAnimator.SetFloat(paramSetter.parameterName, paramSetter.floatValue);
                            Debug.Log($"  - Float Param: {paramSetter.parameterName} = {paramSetter.floatValue}");
                            break;
                        case AnimatorParameterSetter.ParamType.Trigger:
                            npc.npcAnimator.SetTrigger(paramSetter.parameterName);
                            Debug.Log($"  - Trigger Param: {paramSetter.parameterName} 발동");
                            break;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[NPCSkillUser Central] Animator 파라미터 [{paramSetter.parameterName}] 설정 중 오류: {e.Message}. Animator Controller에 해당 파라미터가 정의되어 있는지 확인하세요.");
                }
            }
        }
        // ▲▲▲ 애니메이션 파라미터 설정 로직 끝 ▲▲▲
        else if (npc.npcAnimator == null) Debug.LogWarning($"...");
        else if (skill.animatorParametersToSet == null) Debug.LogWarning($"스킬 [{skill.skillName}]에 animatorParametersToSet이 null입니다.");


        // 쿨다운 설정 (기존과 동일)
        int npcID = npc.npcData.npcID;
        if (!_npcIdSkillCooldowns.ContainsKey(npcID))
        {
            _npcIdSkillCooldowns.Add(npcID, new Dictionary<SkillData, float>());
        }
        _npcIdSkillCooldowns[npcID][skill] = skill.cooldown;
        // Debug.LogError($"[NPCSkillUser Central TRY_USE_SKILL] NPC [{npc.npcData.npcName}] 스킬 [{skill.skillName}] 애니메이션 시작 및 쿨다운 [{skill.cooldown}]초 설정됨.");
        return true;
    }

    //public void ExecuteSkillEffectInternal(NPCController caster, SkillData skill, GameObject target)
    //{
    //    if (skill == null || caster == null || caster.IsFainted) return;
    //    Debug.Log($"[NPCSkillUser Central EXECUTE_EFFECT_INTERNAL] NPC [{caster.npcData.npcName}] applying effect of skill: {skill.skillName} on target: {target?.name ?? "Self/Area"}");
    //}

    //private IEnumerator PerformSkillCast(NPCController npc, SkillData skill, GameObject target)
    //{
    //    yield return new WaitForSeconds(skill.castTime);
    //    ApplySkillEffect(npc, skill, target);
    //}

    //private void ApplySkillEffect(NPCController npc, SkillData skill, GameObject target)
    //{
    //    ExecuteSkillEffectInternal(npc, skill, target);
    //}

    public void ExecuteSkillEffectInternal(NPCController caster, SkillData skill, GameObject target)
    {
        if (skill == null || caster == null || caster.IsFainted)
        {
            Debug.LogWarning($"[NPCSkillUser Central EXECUTE_EFFECT_INTERNAL] 필수 파라미터가 null이거나 시전자가 기절 상태입니다. Skill: {skill?.skillName}, Caster: {caster?.name}");
            return;
        }

        Debug.Log($"[NPCSkillUser Central EXECUTE_EFFECT_INTERNAL] NPC [{caster.npcData.npcName}] applying effect of skill: [{skill.skillName}] on target: [{target?.name ?? "Self/Area"}]");

        // VFX/SFX 재생
        if (skill.vfxPrefab != null)
        {
            Vector3 vfxPosition = caster.transform.position;
            if (target != null && (skill.targetType == SkillTargetType.EnemySingle || skill.targetType == SkillTargetType.Point || skill.targetType == SkillTargetType.EnemyArea))
            {
                vfxPosition = target.transform.position;
            }
            Instantiate(skill.vfxPrefab, vfxPosition, Quaternion.identity);
        }
        AudioSource casterAudioSource = caster.GetComponent<AudioSource>();
        if (skill.sfxClip != null && casterAudioSource != null)
        {
            casterAudioSource.PlayOneShot(skill.sfxClip);
        }

        // 실제 스킬 효과 적용 로직
        else if (skill.skillType == SkillType.Attack && skill.attackDamageAmount > 0)
        {
            Debug.Log($"★★★ NPC [{caster.npcData.npcName}] 스킬 [{skill.skillName}]은 공격 타입이고 데미지 양({skill.attackDamageAmount})이 있습니다.");
            GameObject attackTarget = DetermineActualTarget(caster, skill.targetType, target);

            if (attackTarget != null)
            {
                // ▼▼▼ 자식에서 IDamageable 컴포넌트 검색으로 수정 ▼▼▼
                IDamageable damageableEntity = attackTarget.GetComponentInChildren<IDamageable>(true);

                if (damageableEntity != null)
                {
                    // IDamageable을 찾은 GameObject (자식일 수도 있음)를 실제 피격 대상으로 간주할 수 있습니다.
                    // 또는, 데미지 정보에는 여전히 부모인 attackTarget을 명시할 수도 있습니다.
                    // 여기서는 damageableEntity가 붙어있는 GameObject를 기준으로 합니다.
                    // (하지만 DamageInfo에는 원래 타겟인 attackTarget을 넘기는 것이 일반적일 수 있습니다.)
                    GameObject actualHitObject = null;
                    if (damageableEntity is MonoBehaviour mb) // IDamageable이 MonoBehaviour를 상속했다면
                    {
                        actualHitObject = mb.gameObject;
                    }
                    else if (damageableEntity is Component comp) // Component를 상속했다면
                    {
                        actualHitObject = comp.gameObject;
                    }
                    // 위의 코드는 damageableEntity에서 GameObject를 가져오는 더 안전한 방법입니다.
                    // Component는 항상 gameObject 프로퍼티를 가집니다.

                    float actualDamage = Random.Range(skill.attackDamageAmount * 0.9f, skill.attackDamageAmount * 1.1f);
                    DamageInfo damageInfo = new(actualDamage, 0f, 0f, false, caster.gameObject, actualHitObject != null ? actualHitObject : attackTarget); // 실제 맞은 오브젝트 또는 원래 타겟
                    // Debug.Log($"★★★ NPC [{caster.npcData.npcName}]가 [{(actualHitObject ?? attackTarget).name}]에게 TakeDamage 호출 직전. 데미지: {skill.attackDamageAmount}");
                    damageableEntity.TakeDamage(damageInfo);

                    GameManager.Instance.DamagePopup.ShowPopup(actualDamage, false, Random.onUnitSphere + attackTarget.transform.position + Vector3.up);
                }
                else
                {
                    Debug.LogWarning($"[NPCSkillUser] Attack skill [{skill.skillName}] target [{attackTarget.name}] 또는 그 자식들에서 IDamageable 컴포넌트를 찾을 수 없습니다.");
                }
            }
            else Debug.LogWarning($"[NPCSkillUser] Attack skill [{skill.skillName}] has no valid target.");
        
    }
        else if (skill.skillType == SkillType.Heal || skill.isHealEffect) // 힐 로직 추가 (예시)
        {
            Debug.Log("힐");
            GameObject healTarget = DetermineActualTarget(caster, skill.targetType, target);
            if (healTarget != null)
            {
                var playerToHeal = healTarget.GetComponentInParent<PlayerController>();
                if (playerToHeal != null)
                {
                    int healAmount = skill.healAmount;
                    if (skill.healPercent > 0 && playerToHeal.Stats != null && playerToHeal.Stats.Runtime != null)
                    {
                        healAmount += Mathf.FloorToInt(playerToHeal.Stats.Runtime.MaxHealth * skill.healPercent);
                    }
                    if (playerToHeal.Stats != null) playerToHeal.Stats.Heal(healAmount);
                    Debug.Log($"NPC [{caster.npcData.npcName}] healed [{playerToHeal.name}] for {healAmount} HP with skill [{skill.skillName}].");
                }
            }
        }
        // ... 다른 스킬 효과 로직 ...
    }
    public void SetSkillCooldown(NPCController npc, SkillData skill)
    {
        if (npc == null || npc.npcData == null || skill == null)
        {
            Debug.LogError("[NPCSkillUser Central] SetSkillCooldown: NPC, NPCData, 또는 SkillData가 null입니다.");
            return;
        }
        int npcID = npc.npcData.npcID;

        if (!_npcIdSkillCooldowns.ContainsKey(npcID)) // 해당 NPC의 쿨다운 딕셔너리가 없으면 (이론상 Register에서 생성됨)
        {
            _npcIdSkillCooldowns.Add(npcID, new Dictionary<SkillData, float>());
            Debug.LogWarning($"[NPCSkillUser Central SetSkillCooldown] NPC_ID [{npcID}]에 대한 쿨다운 딕셔너리가 없어 새로 생성 후 쿨다운 설정.");
        }
        _npcIdSkillCooldowns[npcID][skill] = skill.cooldown; // SkillData에 정의된 cooldown 값 사용
        // Debug.LogError($"[NPCSkillUser Central SET_COOLDOWN] NPC_ID [{npcID}] ({npc.npcData.npcName}) 스킬 [{skill.skillName}] (ID: {skill.GetInstanceID()}) 쿨다운을 [{skill.cooldown}]초로 설정! 현재 값: {_npcIdSkillCooldowns[npcID][skill]:F2}");
    }

    private GameObject DetermineActualTarget(NPCController caster, SkillTargetType skillTargetType, GameObject initiallyPassedTarget)
    {
        // ... (이 함수는 이전과 동일하게 caster를 사용하므로 변경 없음) ...
        return skillTargetType switch
        {
            SkillTargetType.Self => caster.gameObject,
            SkillTargetType.AllySingle => GameObject.FindGameObjectWithTag("Player"),
            SkillTargetType.EnemySingle => initiallyPassedTarget,
            _ => initiallyPassedTarget,
        };
    }

    public void UnlockSkillForNpc(NPCController npc, string skillIDToUnlock) // 변수명 명확히
    {
        if (npc == null || npc.npcData == null || string.IsNullOrEmpty(skillIDToUnlock)) return;
        int npcID = npc.npcData.npcID; // ★★★ npcID 사용 ★★★

        if (!_npcIdAvailableSkills.TryGetValue(npcID, out var availableSkills))
        {
            availableSkills = new List<SkillData>();
            _npcIdAvailableSkills.Add(npcID, availableSkills); // ★★★ npcID로 추가 ★★★
        }

        if (availableSkills.Any(s => s.skillID == skillIDToUnlock)) return;

        SkillData skill = GameManager.Instance.GetSkillByID(skillIDToUnlock);
        if (skill != null)
        {
            availableSkills.Add(skill);
            if (!_npcIdSkillCooldowns.ContainsKey(npcID))
            {
                _npcIdSkillCooldowns.Add(npcID, new Dictionary<SkillData, float>()); // ★★★ npcID로 추가 ★★★
            }
            if (!_npcIdSkillCooldowns[npcID].ContainsKey(skill))
            {
                _npcIdSkillCooldowns[npcID].Add(skill, 0f); // ★★★ npcID로 접근 ★★★
            }
            Debug.Log($"NPC_ID [{npcID}] ({npc.npcData.npcName}) new skill unlocked: {skill.skillName}");
        }
    }
}
