using UnityEngine;

[RequireComponent(typeof(Collider))]
public class QuestLocationTrigger : MonoBehaviour
{
    [Tooltip("이 지역의 고유 숫자 ID (ReachLocationObjectiveDefinition의 ID와 일치해야 함)")]
    public int locationID;

    [Tooltip("플레이어 도달 시 발행할 이벤트 (GameEventInt 타입 SO 연결)")]
    public GameEventInt locationReachedEvent;

    [Tooltip("플레이어로 인식할 태그")]
    public string playerTag = "Player";

    private Collider triggerCollider;
    // private bool eventRaised = false; // 이제 매번 체크하므로 이 플래그는 필요 없을 수 있음

    void Awake()
    {
        triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null) { triggerCollider.isTrigger = true; }
        else { Debug.LogError($"QuestLocationTrigger on {gameObject.name} requires a Collider component!"); }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 이벤트 SO가 없거나 들어온 대상이 플레이어가 아니면 무시
        if (locationReachedEvent == null || !other.CompareTag(playerTag))
        {
            return;
        }

        // locationID가 유효한지 확인
        if (locationID == 0)
        {
            Debug.LogWarning($"QuestLocationTrigger on {gameObject.name} has an invalid locationID (0)!");
            return;
        }

        // ★★★ 활성 퀘스트 확인 로직 추가 ★★★
        if (QuestManager.Instance.PlayerQuestDataManager.QuestData != null)
        {
            var activeQuests = QuestManager.Instance.PlayerQuestDataManager.QuestData.activeQuests;
            // 플레이어의 모든 활성 퀘스트를 순회
            foreach (var kvp in activeQuests)
            {
                QuestStatus status = kvp.Value;
                if (status.isCompleted || status.questData == null) continue; // 완료되었거나 데이터 없는 퀘스트 스킵

                // 퀘스트의 모든 목표를 순회
                for (int i = 0; i < status.questData.objectives.Count; i++)
                {
                    // 목표가 ReachLocationObjectiveDefinition 타입이고, locationID가 일치하며, 아직 완료되지 않았다면
                    if (status.questData.objectives[i] is ReachLocationObjectiveDefinition reachObjective &&
                        reachObjective.locationID == this.locationID &&
                        !status.IsObjectiveComplete(i))
                    {
                        // 해당 퀘스트를 진행 중인 것이 확인됨!
                        Debug.Log($"Player entered trigger for active quest '{status.questData.questName}' objective: Reach Location ID {this.locationID}");

                        // 이벤트 발행
                        locationReachedEvent.Raise(this.locationID);

                        // ★★★ 중요: 여기서 return 해야 함 ★★★
                        // 동일한 locationID를 목표로 하는 다른 퀘스트가 또 있을 수 있지만,
                        // 일반적으로 한 번의 트리거 진입으로 하나의 이벤트만 발생시키는 것이 좋습니다.
                        // 만약 여러 퀘스트의 동일 지역 도달 목표를 동시에 처리하고 싶다면 return을 제거할 수 있습니다.
                        return;
                    }
                }
            }
            // 여기까지 왔다면, 이 지역을 목표로 하는 '활성이고 미완료된' 퀘스트가 없는 것임
            // Debug.Log($"Player entered trigger for location {locationID}, but no matching active quest objective found.");
        }
        else
        {
            Debug.LogWarning("PlayerQuest.Instance not found. Cannot check active quests.");
        }
        // ★★★ 활성 퀘스트 확인 로직 끝 ★★★
    }

    // OnEnable/OnDisable의 eventRaised 리셋은 이제 불필요할 수 있음
    // void OnEnable() { eventRaised = false; }
}
