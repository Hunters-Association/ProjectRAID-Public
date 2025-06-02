using UnityEngine;

public class MonsterDeadState : MonsterBaseState
{
    private float noPlayerNearbyTimer = 0f;
    private GameEventInt monsterKilledEvent; // ★ 이벤트 참조 저장용 변수
    private int monsterID;
    private CorpseInteractionTrigger interactionTrigger;
    public MonsterDeadState(Monster contextMonster) : base(contextMonster)
    {
        if (contextMonster.monsterData != null)
        {
            // MonsterData에서 이벤트와 ID를 가져와 내부 변수에 저장
            this.monsterKilledEvent = contextMonster.monsterData.monsterKilledEvent;
            this.monsterID = contextMonster.monsterData.monsterID;
        }
        else
        {
            Debug.LogError($"Monster {contextMonster.gameObject.name} is missing MonsterData! Cannot get ID or KilledEvent.");
            this.monsterID = -1; // 오류 값
        }
        interactionTrigger = contextMonster.GetComponentInChildren<CorpseInteractionTrigger>(true);
        if (interactionTrigger == null)
        {
            // 자식 트리거가 없는 몬스터 타입도 있을 수 있으므로 Warning 정도로 처리
            Debug.LogWarning($"Monster {contextMonster.gameObject.name} does not have a CorpseInteractionTrigger child object.", contextMonster);
        }
    }

    public override void EnterState()
    {
        monster.animator.SetTrigger("IsDead");
        monster.animator.SetTrigger("Is_D_Dead");
        monster.animator.SetTrigger("Is_N_Dead");
        noPlayerNearbyTimer = 0f;
        monster.DisableCollider();
        monster.StopMovement();
        monster.ClearTarget();
        monster.ClearAttackers();
        monster.SetGatherable(true); // ★ 즉시 갈무리 가능        
        monster.NotifyManagerOfDeath();
        if (interactionTrigger != null)
        {
            // 자식 트리거의 상태를 업데이트하여 콜라이더 등을 활성화하도록 요청
            interactionTrigger.UpdateInteractableState();
            // 또는 필요하다면 GameObject 자체를 활성화할 수도 있음
            // interactionTrigger.gameObject.SetActive(true);
            Debug.Log($"Corpse Interaction Trigger processing activated for {monster.gameObject.name}");
        }
        if (monsterKilledEvent != null && monsterID != -1) // 유효한 ID와 이벤트가 있을 때만
        {
            monsterKilledEvent.Raise(monsterID); // 저장된 ID로 이벤트 발행            
        }
        else if (monsterID != -1) // ID는 있는데 이벤트가 없는 경우 경고
        {
            Debug.LogWarning($"Monster {monster.gameObject.name} (ID: {monsterID}) has no MonsterKilledEvent assigned in its MonsterData!");
        }
    }

    public override void UpdateState()
    {
        // ★ 즉시 플레이어 체크 및 자동 소멸 로직 시작 ★
        if (monster.CheckForNearbyPlayers()) { noPlayerNearbyTimer = 0f; }
        else
        {
            noPlayerNearbyTimer += Time.deltaTime;
            if (noPlayerNearbyTimer >= monster.CorpseLingerDurationWithoutPlayer)
            {
                monster.ForceDespawnCorpse(); return;
            }
        }
    }

    public override void ExitState()
    {
        if (interactionTrigger != null)
        {
            // GameObject를 비활성화하거나 Collider를 끄거나 UpdateInteractableState 호출
            interactionTrigger.UpdateInteractableState(); // 내부에서 CanInteractNow가 false가 되어 Collider 꺼짐
            interactionTrigger.HideHighlight(); // 하이라이트 끄기
            Debug.Log($"Corpse Interaction Trigger deactivated for {monster.gameObject.name} on state exit.");
        }
    }

    public override void OnTakeDamage(DamageInfo info)
    {

    }
}