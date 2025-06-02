using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// NPC가 퀘스트를 제공하거나 완료 처리하는 상호작용 로직을 담당하는 컴포넌트.
/// NPC GameObject에 부착됩니다.
/// </summary>
public class NPCQuestInteraction : MonoBehaviour, IInteractable
{
    [Header("NPC 데이터")]
    [Tooltip("이 NPC의 데이터를 담고 있는 NPCData ScriptableObject. Inspector에서 할당하거나 Awake에서 찾습니다.")]
    [SerializeField] private NPCData npcDataSource;

    [Tooltip("이 NPC의 고유 식별자 (선택적, QuestData의 Giver/Completer ID와 비교용)")]
    public int npcID;
    private NPCController _npcController;

    void Awake()
    {
        //  npcDataSource 자동 할당 로직 (Inspector에서 할당 안했을 경우) 
        if (npcDataSource == null)
        {
            _npcController = GetComponentInParent<NPCController>(); // NPCController 먼저 찾기
            if (_npcController != null && _npcController.npcData != null)
            {
                npcDataSource = _npcController.npcData;
                Debug.Log($"[{gameObject.name}] NPCDataSource를 부모 NPCController에서 자동으로 할당했습니다.");
            }
            else
            {
                Debug.LogError($"[{gameObject.name}] NPCDataSource(NPCData)가 할당되지 않았고, 부모 NPCController에서 찾을 수 없습니다!", this);
            }
        }
        //  npcDataSource 자동 할당 로직 

        // npcID도 npcDataSource에서 가져오도록 통일 가능
        if (npcDataSource != null && npcID == 0)
        {
            npcID = npcDataSource.npcID;
        }
        // _npcController가 위에서 할당되지 않았다면 여기서 다시 시도
        if (_npcController == null) _npcController = GetComponentInParent<NPCController>();
    }

    public void Interact(PlayerController player) // <<< 파라미터 PlayerController
    {
        if (npcDataSource == null)
        {
            Debug.LogError($"NPC [{gameObject.name}]의 npcDataSource(NPCData)가 없습니다. 상호작용 불가.");
            return;
        }
        if (QuestManager.Instance.PlayerQuestDataManager.QuestData == null)
        { // PlayerDataManager의 QuestData 확인
            Debug.LogError("Interact failed: PlayerDataManager Instance or its QuestData is null!");
            return;
        }
        // ★★★★★★★★★★★★★★★★★★★★★★★★

        if (QuestManager.Instance == null) { Debug.LogError("Interact failed: QuestManager Instance is null!"); return; }

        Debug.Log($"--- NPC {gameObject.name}: Interact called by {player.gameObject.name} ---");

        if (npcDataSource.completableQuestIDs != null && npcDataSource.completableQuestIDs.Count > 0)
        {
            foreach (int questID in npcDataSource.completableQuestIDs)
            {
                if (QuestManager.Instance.IsQuestReadyToComplete(questID))
                {
                    Debug.Log($"NPC [{npcDataSource.npcName}]이(가) 퀘스트 [{questID}] 완료를 제안합니다.");
                    NPCQuestUI npcUI = UIManager.Instance.ShowUI<NPCQuestUI>(true);
                    if (npcUI != null)
                    {
                        npcUI.ShowCompletion(questID, this); // this는 NPCQuestInteraction 인스턴스
                    }
                    else Debug.LogError("NPCQuestUI(완료)를 열 수 없습니다!");
                    return; // 완료 가능한 퀘스트가 있으면 다른 상호작용은 진행하지 않음
                }
            }

            // 3. 아이템 전달 목표 확인
            var activeQuests = QuestManager.Instance.PlayerQuestDataManager.QuestData.activeQuests;
            foreach (var kvp in activeQuests)
            {
                QuestStatus status = kvp.Value;
                if (status == null || status.questData == null || status.questData.objectives == null) continue;
                for (int i = 0; i < status.questData.objectives.Count; i++)
                {
                    if (status.questData.objectives[i] == null) continue;
                    if (status.questData.objectives[i] is DeliverItemObjectiveDefinition d &&
                            d.targetNPCID == this.npcID && // 현재 NPC의 ID와 일치하는지 확인
                            !status.IsObjectiveComplete(i))
                    {
                        // ★★★ 수정: TryCompleteObjective 호출 시 status만 전달 ★★★
                        if (d.TryCompleteObjective(status)) // <<< playerQuest 참조 제거
                        {
                            /* 성공 UI */
                            Debug.Log($"NPC: Thank you for delivering items for quest {status.questID}.");
                        }
                        else
                        {
                            /* 실패 UI */
                            Debug.Log($"NPC: You don't seem to have the items for quest {status.questID}.");
                        }
                        // ★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★
                        return;
                    }
                }
            }

            // 4. 진행 중인 관련 퀘스트 대화
            if (npcDataSource.availableQuestIDs != null && npcDataSource.availableQuestIDs.Count > 0)
            {

                foreach (int questID in npcDataSource.availableQuestIDs) // npcDataSource의 목록 사용
                {
                    if (QuestManager.Instance.IsQuestAvailable(questID))
                    {
                        Debug.Log($"NPC [{npcDataSource.npcName}]이(가) 새로운 퀘스트 [{questID}]를 제안합니다.");
                        NPCQuestUI npcUI = UIManager.Instance.ShowUI<NPCQuestUI>(true);
                        if (npcUI != null)
                        {
                            npcUI.ShowOffer(questID, this); // NPCQuestInteraction 자신(this) 전달
                        }
                        else Debug.LogError("NPCQuestUI(제공)를 열 수 없습니다!");
                        return; // 퀘스트를 제안하면 다른 상호작용은 진행하지 않음
                    }
                }
            }
            //  npcDataSource.availableQuestIDs 사용 확인 

            // 4. 진행 중인 관련 퀘스트 대화 (선택적, 위에서 퀘스트 제공/완료가 없었을 경우)
            //  npcDataSource의 퀘스트 ID 목록을 합쳐서 사용 
            List<int> allRelatedQuestIDs = new List<int>();
            if (npcDataSource.availableQuestIDs != null) allRelatedQuestIDs.AddRange(npcDataSource.availableQuestIDs);
            if (npcDataSource.completableQuestIDs != null) allRelatedQuestIDs.AddRange(npcDataSource.completableQuestIDs);
            var distinctRelatedIDs = allRelatedQuestIDs.Distinct(); // 중복 제거

            if (distinctRelatedIDs.Any()) // 관련 퀘스트 ID가 하나라도 있다면
            {
                foreach (int questID in distinctRelatedIDs)
                {
                    if (QuestManager.Instance.PlayerQuestDataManager.QuestData.activeQuests.ContainsKey(questID))
                    {
                        QuestStatus status = QuestManager.Instance.PlayerQuestDataManager.QuestData.activeQuests[questID];
                        Debug.Log($"NPC [{npcDataSource.npcName}]: 진행 중인 퀘스트 '{status.questData.questName}' 관련 대화 시작.");
                        // TODO: ShowQuestInProgressDialog UI 호출 (status 전달) 또는 일반 대화창에 관련 내용 표시
                        return; // 관련 대화 후 종료
                    }
                }
            }
            //  npcDataSource의 퀘스트 ID 목록을 합쳐서 사용 

            // 5. 기본 대화 (위 모든 조건에 해당하지 않을 경우)
            Debug.Log($"NPC [{npcDataSource.npcName}] ({gameObject.name}): 기본 대화 (제공/완료/진행 중 관련 퀘스트 없음).");
            // TODO: 기본 대화 UI 또는 시스템 호출
            //  DialogueManager.Instance.StartDefaultDialogue(npcDataSource.defaultDialogueID); // NPCData에 기본 대화 ID가 있다고 가정
        }

    }

    // --- 아래 함수들은 UI 버튼 등에서 호출됩니다 ---

    /// <summary>
    /// 플레이어가 UI를 통해 퀘스트 수락을 결정했을 때 호출될 함수.
    /// </summary>
    public void PlayerAcceptedQuest(int questID)
    {
        bool success = QuestManager.Instance.AcceptQuest(questID);
        if (success)
        {
            Debug.Log($"NPC: Quest '{questID}' accepted by player.");
            _npcController?.affinityComponent?.AddAffinity(5);
        }
        else
        {
            Debug.LogWarning($"NPC: Failed to accept quest '{questID}'.");
        }
    }

    /// <summary>
    /// 플레이어가 UI를 통해 퀘스트 완료를 결정했을 때 호출될 함수.
    /// </summary>
    public void PlayerCompletedQuest(int questID)
    {
        bool success = QuestManager.Instance.CompleteQuest(questID);
        if (success)
        {
            Debug.Log($"NPC: Quest '{questID}' completed by player. Well done!");

            _npcController?.affinityComponent?.AddAffinity(20);

            switch (questID)
            {
                case 1: AnalyticsManager.SendFunnelStep(10); break; // 퀘스트 ID 1 = Nutkey
                case 2: AnalyticsManager.SendFunnelStep(21); break; // 퀘스트 ID 2 = LAVIES
                case 3: AnalyticsManager.SendFunnelStep(26); break; // 퀘스트 ID 3 = VALKARION

                default: break;
            }
        }
        else
        {
            Debug.LogWarning($"NPC: Failed to complete quest '{questID}'.");
        }
    }

    public void ShowHighlight() { }
    public void HideHighlight() { }


    [SerializeField] private InteractableData data;

    public InteractableData GetInteractableData()
    {
        if (data == null) Debug.LogWarning("[NPCQuestInteraction] 상호작용 데이터가 등록되지 않았습니다.");
        return data;
    }
}
