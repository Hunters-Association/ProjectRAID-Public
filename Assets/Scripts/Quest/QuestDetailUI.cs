using System.Buffers.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestDetailUI : BaseUI, IBlockingUI 
{
    [Header("Detail References")]
    public TextMeshProUGUI questTitleText;
    public TextMeshProUGUI questDescriptionText;
    public TextMeshProUGUI questObjectivesText;
    public TextMeshProUGUI questRewardsText;
    public Button abandonButton;
    public Button closeButton; // 상세 패널 닫기 버튼

    [Header("NPC 동료 버튼")]
    [Tooltip("NPC와 함께하기/해제하기 버튼")]
    [SerializeField] private Button npcCompanionButton;
    [Tooltip("NPC 함께하기 버튼의 텍스트 (TMPUGUI)")]
    [SerializeField] private TextMeshProUGUI npcCompanionButtonText;

    private int currentQuestID;
    private NPCController _currentDisplayingNpcController;

    public bool BlocksGameplay => true;

    public override void Initialize()
    {
        base.Initialize();
        // 참조 확인
        if (closeButton != null) closeButton.onClick.AddListener(ClosePanel);
        else Debug.LogWarning("Close button not assigned.");
        if (abandonButton != null) abandonButton.onClick.AddListener(AbandonQuest);
        else Debug.LogWarning("Abandon button not assigned.");

        if (npcCompanionButton != null)
        {
            npcCompanionButton.onClick.AddListener(OnNpcCompanionButtonClicked);
        }
        else Debug.LogWarning("QuestDetailUI: npcCompanionButton이 할당되지 않았습니다. NPC 함께하기 기능 비활성화.");
    }
    private void OnEnable()
    {
        // 패널이 활성화될 때마다 이벤트 구독 시도
        SubscribeToEvents();
        if (_currentDisplayingNpcController != null)
        {
            SubscribeToNpcEvents(_currentDisplayingNpcController);
        }
        UpdateCompanionButtonAppearance();
    }    

    // ★★★ OnDisable: 이벤트 구독 해제 ★★★
    private void OnDisable()
    {
        // 패널이 비활성화될 때 이벤트 구독 해제
        UnsubscribeFromEvents();
        if (_currentDisplayingNpcController != null)
        {
            UnsubscribeFromNpcEvents(_currentDisplayingNpcController);
        }
        //currentQuestID = 0; // 비활성화 시 선택된 ID 초기화
    }    

    // ★ 이벤트 구독/해제 함수 ★
    private void SubscribeToEvents()
    {
        if (QuestManager.Instance != null && QuestManager.Instance.PlayerQuestDataManager != null)
        {
            QuestManager.Instance.PlayerQuestDataManager.OnQuestProgressUpdated += HandleProgressUpdate;
            // 필요하다면 다른 이벤트도 구독 ( OnQuestCompleted - 완료 시 창 닫기)
            // QuestManager.Instance.PlayerQuestDataManager.OnQuestCompleted += HandleQuestCompletion;
        }
        else
        {
            Debug.LogWarning("QuestDetailUI: QuestManager 또는 PlayerQuestDataManager 인스턴스를 찾을 수 없어 퀘스트 이벤트를 구독할 수 없습니다.");
        }
    }
    private void UnsubscribeFromEvents()
    {
        if (QuestManager.Instance != null && QuestManager.Instance.PlayerQuestDataManager != null)
        {
            QuestManager.Instance.PlayerQuestDataManager.OnQuestProgressUpdated -= HandleProgressUpdate;
            // PlayerDataManager.Instance.OnQuestCompleted -= HandleQuestCompletion;
        }
    }

    private void SubscribeToNpcEvents(NPCController npc)
    {
        if (npc == null) return;
        npc.OnCombatEligibilityChanged -= HandleNpcCompanionStateChanged; // 중복 구독 방지
        npc.OnFollowingStatusChanged -= HandleNpcCompanionStateChanged;   // 중복 구독 방지
        npc.OnCombatEligibilityChanged += HandleNpcCompanionStateChanged;
        npc.OnFollowingStatusChanged += HandleNpcCompanionStateChanged;
    }

    private void UnsubscribeFromNpcEvents(NPCController npc)
    {
        if (npc == null) return;
        npc.OnCombatEligibilityChanged -= HandleNpcCompanionStateChanged;
        npc.OnFollowingStatusChanged -= HandleNpcCompanionStateChanged;
    }

    // QuestLogUI에서 호출하여 특정 퀘스트 정보 표시
    public void ShowQuestDetails(int questID)
    {
        Debug.Log($"--- QuestDetailUI ShowQuestDetails called with QuestID: {questID} ---");
        currentQuestID = questID; // 현재 보고 있는 퀘스트 ID 저장

        if (_currentDisplayingNpcController != null)
        {
            UnsubscribeFromNpcEvents(_currentDisplayingNpcController);
        }
        _currentDisplayingNpcController = null;

        // 1.데이터 유효성 확인
        QuestStatus status;
        if (QuestManager.Instance == null || QuestManager.Instance.PlayerQuestDataManager == null || QuestManager.Instance.PlayerQuestDataManager.QuestData == null ||
            !QuestManager.Instance.PlayerQuestDataManager.QuestData.activeQuests.TryGetValue(questID, out status) ||
            status.questData == null)
        {
            Debug.LogError($"Cannot show details for Quest ID: {questID}. Data not found or invalid.");
            UpdateCompanionButtonAppearance();
            ClosePanel();
            return;
        }

        // --- 2.기본 정보 표시 ---
        if (questTitleText != null) questTitleText.text = status.questData.questName;
        else Debug.LogWarning("questTitleText not assigned.");

        if (questDescriptionText != null) questDescriptionText.text = status.questData.description;
        else Debug.LogWarning("questDescriptionText not assigned.");


        // ★★★ 목표 텍스트 생성 및 할당 (주석 해제 및 구현) ★★★
        StringBuilder objectivesSB = new StringBuilder("목표:\n"); // 제목 설정
        if (status.questData.objectives != null && status.questData.objectives.Count > 0)
        {
            for (int i = 0; i < status.questData.objectives.Count; i++)
            {
                if (status.questData.objectives[i] != null)
                {
                    // 각 목표 정의 SO의 GetProgressDescription 함수 호출하여 설명 가져오기
                    objectivesSB.AppendLine(status.questData.objectives[i].GetProgressDescription(status, i));
                }
                else { objectivesSB.AppendLine("- 유효하지 않은 목표 데이터 -"); }
            }
        }
        else { objectivesSB.AppendLine("- 목표 없음 -"); }

        // 최종 생성된 문자열을 Text 컴포넌트에 할당
        if (questObjectivesText != null) questObjectivesText.text = objectivesSB.ToString();
        else Debug.LogWarning("questObjectivesText not assigned.");
        // ★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★


        // ★★★ 보상 텍스트 생성 및 할당 (주석 해제 및 구현) ★★★
        StringBuilder rewardsSB = new StringBuilder("보상:\n"); // 제목 설정
        if (status.questData.rewards != null && status.questData.rewards.Count > 0)
        {
            foreach (var rewardDef in status.questData.rewards)
            {
                if (rewardDef != null)
                {
                    // 각 보상 정의 SO의 GetDescription 함수 호출하여 설명 가져오기
                    rewardsSB.AppendLine("- " + rewardDef.GetDescription());
                }
                else { rewardsSB.AppendLine("- 유효하지 않은 보상 데이터 -"); }
            }
        }
        else { rewardsSB.AppendLine("- 보상 없음 -"); }

        // 최종 생성된 문자열을 Text 컴포넌트에 할당
        if (questRewardsText != null) questRewardsText.text = rewardsSB.ToString();
        else Debug.LogWarning("questRewardsText not assigned.");

        // --- 3. 이 퀘스트와 관련된 NPC 찾기 ---
        int relatedNpcID = status.questData.questGiverID; 
        if (relatedNpcID != 0)
        {
            if (GameManager.Instance != null) // GameManager null 체크
            {
                NPCController[] allNpcs = FindObjectsOfType<NPCController>(); // 성능에 민감한 부분에서는 비권장
                NPCController foundNpc = allNpcs.FirstOrDefault(npc => npc.npcData != null && npc.npcData.npcID == relatedNpcID);

                if (foundNpc != null)
                {
                    _currentDisplayingNpcController = foundNpc; // ★★★ NPC 찾으면 할당 ★★★
                    Debug.Log($"[QuestDetailUI ShowQuestDetails] Quest 관련 NPC 찾음: {_currentDisplayingNpcController.npcData.npcName}. 이벤트 구독.");
                    SubscribeToNpcEvents(_currentDisplayingNpcController);
                }
                else
                {
                    Debug.LogWarning($"[QuestDetailUI] Quest 관련 NPC (ID: {relatedNpcID})를 씬에서 찾을 수 없습니다. _currentDisplayingNpcController는 null로 유지됩니다.");
                    // _currentDisplayingNpcController는 이미 위에서 null로 초기화되었으므로, 여기서는 별도의 할당이 필요 없음
                }
            }
            else
            {
                Debug.LogError("[QuestDetailUI] GameManager 인스턴스가 없습니다. NPC를 찾을 수 없습니다.");
                // _currentDisplayingNpcController는 null로 유지됨
            }
        }
        else
        {
            Debug.LogWarning($"[QuestDetailUI] 퀘스트 ID [{questID}]에 관련된 NPC ID가 0이거나 설정되지 않았습니다. _currentDisplayingNpcController는 null로 유지됩니다.");
            // _currentDisplayingNpcController는 null로 유지됨
        }

        // 포기 버튼 활성화 (반복 불가능 퀘스트일 때만 보이도록 예시)
        if (abandonButton != null)
        {
            abandonButton.gameObject.SetActive(!status.questData.isRepeatable);
        }

        UpdateCompanionButtonAppearance();

        // 패널 활성화 (BaseUI 상속 시 OnShow에서 처리하는 것이 더 일반적)
        // 이 스크립트가 BaseUI를 상속하고 UIManager로 관리된다면
        // 이 SetActive 호출 대신 UIManager.ShowUI<QuestDetailUI>()를 통해 열어야 합니다.
        // 만약 QuestLogUI에서 직접 참조하여 SetActive로 관리한다면 이 코드가 필요합니다.

    }
    private void HandleProgressUpdate(QuestStatus updatedStatus)
    {
        // 이 UI 패널이 활성화 상태이고, 업데이트된 퀘스트가 현재 보고 있는 퀘스트 ID와 같다면
        if (gameObject.activeInHierarchy && updatedStatus != null && updatedStatus.questID == currentQuestID)
        {
            Debug.Log($"QuestDetailUI received progress update for currently viewed quest {currentQuestID}. Refreshing UI.");
            // 내용을 다시 로드하고 UI를 갱신
            ShowQuestDetails(currentQuestID);
        }
    }

    private void AbandonQuest()
    {
        if (currentQuestID != 0)
        {
            QuestData abandonedQuest = QuestManager.Instance.Database.GetQuestByID(currentQuestID); // 포기할 퀘스트 데이터 가져오기

            // 퀘스트 포기 시, 해당 퀘스트와 관련된 NPC가 현재 함께하고 있었다면 해제
            // (이전에는 _currentDisplayingNpcController를 직접 참조했지만,
            //  이제는 NPCManager가 퀘스트 정보와 현재 동료 목록을 비교하여 처리하도록 할 수 있음)
            //  또는, _currentDisplayingNpcController가 여전히 유효하다면 직접 해제 요청.
            if (_currentDisplayingNpcController != null && _currentDisplayingNpcController.IsActivelyFollowingPlayer)
            {
                // 특정 조건 (예: 이 퀘스트가 이 NPC를 데려가게 한 '그' 퀘스트인가?)을 더 정교하게 판단 후 해제
                if (abandonedQuest != null && abandonedQuest.specificCompanionNpcIDs != null &&
                    abandonedQuest.specificCompanionNpcIDs.Contains(_currentDisplayingNpcController.npcData.npcID) &&
                    abandonedQuest.isHuntQuest)
                {
                    Debug.Log($"퀘스트 [{currentQuestID}] 포기로 인해 NPC [{_currentDisplayingNpcController.npcData.npcName}] 동료 상태 해제 (NPCManager 통해).");
                    NPCManager.Instance?.RemoveActiveCompanion(_currentDisplayingNpcController);
                }
            }
            // 또는 NPCManager의 HandleQuestAbandonedForCompanions 가 이 역할을 하도록 둘 수도 있음

            QuestManager.Instance.AbandonQuest(currentQuestID);
            ClosePanel();
        }
    }
    private void HandleNpcCompanionStateChanged(NPCController npc, bool newStatus) // 파라미터는 NPCController의 이벤트 시그니처와 일치
    {
        // 현재 UI에 표시 중인 NPC의 상태가 변경되었을 때만 버튼 업데이트
        if (npc != null && _currentDisplayingNpcController == npc)
        {
            UpdateCompanionButtonAppearance();
        }
    }
    // ▲▲▲ NPCController의 이벤트 발생 시 호출될 핸들러 ▲▲▲

    // ▼▼▼ "함께하기/해제하기" 버튼 상태 및 텍스트 업데이트 로직  ▼▼▼
    private void UpdateCompanionButtonAppearance()
    {
        if (npcCompanionButton == null) return;

        if (_currentDisplayingNpcController != null && _currentDisplayingNpcController.npcData != null)
        {
            // 버튼은 '현재 플레이어를 따라다니고 있을 때만' 활성화하여 "동료 해제" 기능만 제공
            if (_currentDisplayingNpcController.IsActivelyFollowingPlayer)
            {
                npcCompanionButton.gameObject.SetActive(true);
                npcCompanionButton.interactable = true;
                if (npcCompanionButtonText != null)
                {
                    npcCompanionButtonText.text = "동료 해제";
                }
            }
            else // 따라다니고 있지 않으면 버튼 숨김
            {
                npcCompanionButton.gameObject.SetActive(false);
            }
        }
        else
        {
            npcCompanionButton.gameObject.SetActive(false);
        }
    }
    // ▲▲▲ "함께하기/해제하기" 버튼 상태 및 텍스트 업데이트 로직  ▲▲▲

    // ▼▼▼ "함께하기/해제하기" 버튼 클릭 시 호출될 메서드 ▼▼▼
    private void OnNpcCompanionButtonClicked()
    {
        if (_currentDisplayingNpcController != null && _currentDisplayingNpcController.IsActivelyFollowingPlayer)
        {
            // NPCManager를 통해 동료 해제 요청
            NPCManager.Instance?.RemoveActiveCompanion(_currentDisplayingNpcController);
            // 버튼 상태는 NPCController의 OnFollowingStatusChanged 이벤트를 통해 업데이트됨
        }
        else
        {
            // 이 버튼은 IsActivelyFollowingPlayer가 true일 때만 활성화되므로, 이_else는 거의 호출되지 않음
            Debug.LogWarning("OnNpcCompanionButtonClicked: 현재 동행 중인 NPC가 아니거나, 버튼 상태 오류.");
        }
    }



    public void ClosePanel()
    {
        gameObject.SetActive(false);
        // 또는 base.OnHide(); 사용 가능 (애니메이션 원할 시)
    }

    // OnShow/OnHide는 UIManager로 관리 시 자동으로 호출됨
    public override void OnShow()
    {
        base.OnShow();
        SubscribeToEvents(); // 퀘스트 진행도 업데이트 이벤트 구독

        // ▼▼▼ OnShow에서는 _currentDisplayingNpcController가 이미 설정된 경우에만 이벤트 구독 및 버튼 업데이트 시도 ▼▼▼
        // (ShowQuestDetails가 호출된 후 UI가 다시 활성화되는 경우를 위함)
        if (_currentDisplayingNpcController != null)
        {
            Debug.Log($"[QuestDetailUI OnShow] 기존 _currentDisplayingNpcController [{_currentDisplayingNpcController.npcData.npcName}]에 대한 이벤트 재구독 및 버튼 업데이트.");
            SubscribeToNpcEvents(_currentDisplayingNpcController);
            UpdateCompanionButtonAppearance(); // NPC 참조가 이미 있다면 버튼 상태 업데이트
        }
        else
        {
            // _currentDisplayingNpcController가 null이면 (보통 처음 열릴 때),
            // ShowQuestDetails에서 NPC를 찾고 버튼을 업데이트할 것이므로 여기서는 버튼을 숨김 상태로 둠.
            Debug.Log("[QuestDetailUI OnShow] _currentDisplayingNpcController is NULL. 버튼은 ShowQuestDetails 이후 업데이트됩니다.");
            if (npcCompanionButton != null) npcCompanionButton.gameObject.SetActive(false);
        }
        // ▲▲▲ OnShow 로직 수정 ▲▲▲
        CursorManager.SetCursorState(true);
    }

    public override void OnHide()
    {
        UnsubscribeFromEvents();
        if (_currentDisplayingNpcController != null)
        {
            UnsubscribeFromNpcEvents(_currentDisplayingNpcController);
        }
        // _currentDisplayingNpcController = null; // 여기서 null로 만들면 ShowQuestDetails 호출 시 항상 새로 찾음
        // currentQuestID = 0; // ShowQuestDetails 호출 시 다시 설정됨
        CursorManager.SetCursorState(false);
        base.OnHide();
    }
}
