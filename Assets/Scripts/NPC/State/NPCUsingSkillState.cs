using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCUsingSkillState : NPCBaseState
{
    private SkillData _skillBeingUsed;
    private GameObject _skillTarget;
    private float _castTimer;
    private bool _isCasting;
    private NPCSkillUser _centralSkillUser; 

    public NPCUsingSkillState(NPCStateMachine stateMachine, NPCController npcController) : base(stateMachine, npcController) { }

    public override void Enter()
    {
        NpcController.RequestDrawWeapon();
        StopMovement(); // 스킬 사용 중 이동 불가

        // GameManager에서 NPCSkillUser 인스턴스 가져오기
        if (GameManager.Instance != null)
        {
            _centralSkillUser = GameManager.Instance.GetComponent<NPCSkillUser>();
        }
        if (_centralSkillUser == null)
        {
            Debug.LogError($"[{NpcController.npcData?.npcName}] UsingSkillState: GameManager에 NPCSkillUser가 없습니다! CombatIdle로 복귀.");
            StateMachine.ChangeState(NPCState.CombatIdle);
            return;
        }

        _skillBeingUsed = StateMachine.GetNextSkillToUse(); // NPCStateMachine에서 현재 사용할 스킬 가져오기
        _skillTarget = StateMachine.GetNextSkillTarget();   // NPCStateMachine에서 현재 스킬 타겟 가져오기

        if (_skillBeingUsed == null)
        {
            Debug.LogError($"[{NpcController.npcData?.npcName}] UsingSkillState: 사용할 스킬 정보가 없습니다! CombatIdle로 복귀.");
            StateMachine.ChangeState(NPCState.CombatIdle);
            return;
        }

        Debug.Log($"[{NpcController.npcData.npcName}] Entering UsingSkill State for skill: {_skillBeingUsed.skillName}");

        // 타겟 바라보기 (선택적)
        Transform lookTarget = _skillTarget != null ? _skillTarget.transform : GameObject.FindGameObjectWithTag("Player")?.transform;
        if (lookTarget != null) LookAt(lookTarget);

        // ▼▼▼ NPCSkillUser.TryUseSkill 호출하여 애니메이션 시작, NPCController에 정보 전달, 쿨다운 설정 ▼▼▼
        bool skillUseInitiated = _centralSkillUser.TryUseSkill(this.NpcController, _skillBeingUsed, _skillTarget);
        // ▲▲▲ 호출 끝 ▲▲▲

        if (!skillUseInitiated) // 스킬 사용 시작 실패 (예: CanUseSkill에서 false 반환)
        {
            Debug.LogWarning($"[{NpcController.npcData?.npcName}] 스킬 [{_skillBeingUsed.skillName}] 사용 시작 실패 (TryUseSkill 반환 false). CombatIdle로 복귀.");
            StateMachine.ChangeState(NPCState.CombatIdle);
            return;
        }
        // 성공적으로 TryUseSkill이 호출되면 애니메이션이 시작되고 쿨다운이 NPCSkillUser에 의해 적용됨.
        // 실제 효과 적용은 애니메이션 이벤트가 NPCController.TriggerSkillEffect를 호출하여 이루어짐.

        // 캐스팅 시간은 이제 애니메이션 클립의 길이 또는 애니메이션 이벤트 발생까지의 시간으로 간주.
        // 이 상태에서 별도의 타이머를 돌릴 필요가 없음.
        // 애니메이션이 끝나면 Animator Controller의 트랜지션에 의해 다음 상태로 자동 전환되도록 설정하는 것이 이상적.
    }

    public override void Execute()
    {
        
    }

    //private void ApplyEffectAndSetCooldownAndFinish()
    //{
    //    if (_skillBeingUsed == null || _centralSkillUser == null) // _centralSkillUser는 Enter()에서 할당
    //    {
    //        StateMachine.ChangeState(NPCState.CombatIdle);
    //        return;
    //    }

    //    // 1. 실제 스킬 효과 적용 요청
    //    _centralSkillUser.ExecuteSkillEffectInternal(this.NpcController, _skillBeingUsed, _skillTarget);

    //    // 2. 쿨다운 설정 요청
    //    _centralSkillUser.SetSkillCooldown(this.NpcController, _skillBeingUsed); // ★★★ 이 호출이 쿨다운을 설정합니다 ★★★

    //    StateMachine.ChangeState(NPCState.CombatIdle);
    //}

    public override void Exit()
    {
        _skillBeingUsed = null; // 다음 스킬 사용을 위해 정리
        _skillTarget = null;
    }

    public override void OnDamaged(DamageInfo info)
    {
        if (_skillBeingUsed != null /* && _skillBeingUsed.isCancelableOnHit */) // SkillData에 취소 가능 여부 플래그가 있다면 활용
        {
            // Debug.Log($"[{NpcController.npcData?.npcName}] Skill [{_skillBeingUsed.skillName}] animation/casting interrupted by damage from {info.attacker?.name}.");
            // 현재 재생 중인 스킬 애니메이션을 중단하는 로직 (예: 특정 트리거를 발동하여 Idle로 즉시 전환)
            // if (Animator != null) Animator.SetTrigger("CancelSkill"); // "CancelSkill" 같은 트리거가 Animator Controller에 있다고 가정
            StateMachine.ChangeState(NPCState.CombatIdle); // 또는 피격 반응 상태로 전환
        }
    }
}
