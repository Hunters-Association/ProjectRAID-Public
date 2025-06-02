using System.Buffers.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestLogUI : BaseUI, IBlockingUI // BaseUI 상속 확인
{
    [Header("Quest Log References")]
    [Tooltip("활성 퀘스트 목록이 생성될 부모 Transform (Scroll View의 Content)")]
    [SerializeField] private Transform activeQuestListContent;

    [Tooltip("퀘스트 목록 항목을 위한 프리팹 (QuestLogItem 스크립트 포함)")]
    [SerializeField] private QuestLogItem questListItemPrefab;

    [Tooltip("퀘스트 상세 정보를 표시할 UI 컴포넌트 참조 (QuestDetailUI 스크립트)")]
    [SerializeField] private QuestDetailUI questDetailUI; // Inspector 연결 필수!

    [Tooltip("퀘스트 로그 패널을 닫는 버튼")]
    [SerializeField] private Button closeButton; // Inspector 연결 필수!

    // ★ 현재 상세 정보 UI에 표시 중인 퀘스트 ID 저장용 변수 ★
    private int currentSelectedQuestID_Log = 0;

    public bool BlocksGameplay => true;

    /// <summary>
    /// UI 초기화 시 호출됩니다. 필수 참조 확인 및 버튼 리스너를 설정합니다.
    /// </summary>
    public override void Initialize()
    {
        base.Initialize(); // 부모 클래스의 Initialize 호출 (필요하다면)

        // 필수 참조 변수들이 Inspector에서 할당되었는지 확인
        if (activeQuestListContent == null)
            Debug.LogError("QuestLogUI Error: ActiveQuestListContent is not assigned in the Inspector!", this);
        if (questListItemPrefab == null)
            Debug.LogError("QuestLogUI Error: QuestListItemPrefab is not assigned in the Inspector!", this);
        if (questDetailUI == null)
            Debug.LogError("QuestLogUI Error: QuestDetailUI reference is not assigned in the Inspector! Cannot show quest details.", this);
        if (closeButton == null)
            Debug.LogWarning("QuestLogUI Warning: Close button is not assigned in the Inspector.", this);
        else
            closeButton.onClick.AddListener(ClosePanel); // 닫기 버튼 리스너 연결
    }

    /// <summary>
    /// 이 UI 패널이 활성화될 때 호출됩니다.
    /// 이벤트 구독, 목록 업데이트, 상세 패널 초기화, 커서 상태 설정을 수행합니다.
    /// </summary>
    private void OnEnable()
    {
        SubscribeToEvents(); // 퀘스트 데이터 변경 감지를 위한 이벤트 구독
        UpdateList();        // 현재 활성 퀘스트 목록으로 UI 업데이트
        currentSelectedQuestID_Log = 0; // 상세 정보 선택 초기화

        // 로그 패널 열릴 때 상세 정보 패널은 항상 닫힌 상태로 시작
        if (questDetailUI != null)
        {
            questDetailUI.ClosePanel(); // QuestDetailUI의 닫기 함수 호출
        }
        CursorManager.SetCursorState(true); // UI 상호작용을 위해 커서 보이기
    }

    /// <summary>
    /// 이 UI 패널이 비활성화될 때 호출됩니다.
    /// 이벤트 구독 해제, 상세 패널 닫기, 커서 상태 복원을 수행합니다.
    /// </summary>
    private void OnDisable()
    {
        UnsubscribeFromEvents(); // 이벤트 구독 해제
        currentSelectedQuestID_Log = 0; // 상세 정보 선택 초기화

        // 로그 패널 닫힐 때 상세 정보 패널도 닫기
        if (questDetailUI != null)
        {
            questDetailUI.ClosePanel();
        }
        CursorManager.SetCursorState(false); // 커서 숨기기 및 잠금
    }

    /// <summary>
    /// PlayerQuestDataManager의 퀘스트 관련 이벤트들을 구독합니다.
    /// </summary>
    private void SubscribeToEvents()
    {
        // PlayerQuestDataManager 인스턴스가 존재하는지 확인 후 이벤트 구독
        if (QuestManager.Instance.PlayerQuestDataManager != null) // ★ 실제 사용하는 매니저 이름 확인! ★
        {
            QuestManager.Instance.PlayerQuestDataManager.OnQuestAccepted += UpdateList;
            QuestManager.Instance.PlayerQuestDataManager.OnQuestCompleted += HandleQuestCompletion;
            QuestManager.Instance.PlayerQuestDataManager.OnQuestProgressUpdated += UpdateQuestDetailAndLog;
            QuestManager.Instance.PlayerQuestDataManager.OnQuestAbandoned += UpdateList;
            QuestManager.Instance.PlayerQuestDataManager.OnQuestUnlocked += HandleQuestUnlock; // 해금 이벤트 핸들러 연결
            QuestManager.Instance.PlayerQuestDataManager.OnTrackedQuestChanged += HandleTrackedQuestChanged;
        }
        else { Debug.LogWarning("PlayerDataManager not ready for event subscription in QuestLogUI."); }
    }

    /// <summary>
    /// PlayerQuestDataManager의 퀘스트 관련 이벤트 구독을 해제합니다.
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        // PlayerQuestDataManager 인스턴스가 존재하는지 확인 후 이벤트 구독 해제
        if (QuestManager.Instance.PlayerQuestDataManager != null) // ★ 실제 사용하는 매니저 이름 확인! ★
        {
            QuestManager.Instance.PlayerQuestDataManager.OnQuestAccepted -= UpdateList;
            QuestManager.Instance.PlayerQuestDataManager.OnQuestCompleted -= HandleQuestCompletion;
            QuestManager.Instance.PlayerQuestDataManager.OnQuestProgressUpdated -= UpdateQuestDetailAndLog;
            QuestManager.Instance.PlayerQuestDataManager.OnQuestAbandoned -= UpdateList;
            QuestManager.Instance.PlayerQuestDataManager.OnQuestUnlocked -= HandleQuestUnlock;
            QuestManager.Instance.PlayerQuestDataManager.OnTrackedQuestChanged -= HandleTrackedQuestChanged;
        }
    }

    /// <summary>
    /// 퀘스트 목록 UI를 최신 상태로 업데이트합니다.
    /// </summary>
    /// <param name="changedQuest">변경된 퀘스트 데이터 (현재 미사용)</param>
    private void UpdateList(QuestData changedQuest = null)
    {
        // 필요한 참조 및 데이터가 있는지 확인
        if (QuestManager.Instance.PlayerQuestDataManager.QuestData == null || activeQuestListContent == null || questListItemPrefab == null) return;

        // 기존 목록 아이템 모두 삭제
        foreach (Transform child in activeQuestListContent)
        {
            Destroy(child.gameObject);
        }

        // 활성 퀘스트 목록 가져오기
        var questsToShow = QuestManager.Instance.PlayerQuestDataManager.QuestData.activeQuests;
        if (questsToShow.Count == 0)
        {
            // TODO: "진행 중인 퀘스트가 없습니다" 메시지 표시 로직
            Debug.Log("No active quests to display in QuestLogUI.");
            return;
        }

        // 활성 퀘스트 목록을 순회하며 UI 항목 생성 (퀘스트 ID 오름차순 정렬)
        foreach (var kvp in questsToShow)
        {
            QuestStatus status = kvp.Value;
            // 유효한 데이터인지 확인
            if (status == null || status.questData == null) continue;

            // 목록 아이템 프리팹 인스턴스 생성
            QuestLogItem itemUI = Instantiate(questListItemPrefab, activeQuestListContent);

            if (itemUI != null)
            {
                // QuestLogItem 설정 (퀘스트 상태와 콜백 함수 전달)
                itemUI.Setup(status, SelectQuest); // SelectQuest 함수를 콜백으로 넘김
            }
            else
            {
                Debug.LogError("QuestLogItem prefab is missing the QuestLogItem script!", itemUI);
                // 대체 로직 (Text/Button 직접 찾기 - 비권장)
                // Text itemText = itemGO.GetComponentInChildren<Text>(); ...
                // Button itemButton = itemGO.GetComponent<Button>(); ...
            }
        }
    }

    /// <summary>
    /// 퀘스트 목록에서 특정 항목이 선택되었을 때 호출되는 콜백 함수입니다.
    /// </summary>
    /// <param name="questID">선택된 퀘스트의 ID</param>
    private void SelectQuest(int questID)
    {
        Debug.Log($"--- SelectQuestInLog called with QuestID: {questID} ---");
        currentSelectedQuestID_Log = questID; // 선택된 ID를 내부 변수에 저장

        // QuestDetailUI 참조가 유효한지 확인
        if (UIManager.Instance != null)
        {
            // UIManager에게 QuestDetailUI 표시/가져오기를 요청
            QuestDetailUI detailInstance = UIManager.Instance.ShowUI<QuestDetailUI>(); // ShowUI는 인스턴스를 반환
            if (detailInstance != null)
            {
                Debug.Log($"Calling detailInstance.ShowQuestDetails for {questID}");
                // ★ 반드시 생성/풀링된 '인스턴스'의 메서드를 호출해야 함 ★
                detailInstance.ShowQuestDetails(questID);
            }
            else { Debug.LogError("Failed to get QuestDetailUI instance from UIManager!"); }
        }
        else { Debug.LogError("UIManager Instance is null!"); }
    }

    // --- 이벤트 핸들러 함수들 ---

    /// <summary>
    /// 퀘스트 완료 이벤트 발생 시 호출됩니다. 목록을 업데이트하고, 필요시 상세 정보 창을 닫습니다.
    /// </summary>
    private void HandleQuestCompletion(QuestData completedQuest)
    {
        if (completedQuest == null) return;
        Debug.Log($"QuestLogUI received OnQuestCompleted event for: {completedQuest.questName}");
        UpdateList(); // 목록 새로고침 (완료된 퀘스트 제거)

        // ★ 수정: GetCurrentQuestID 대신 currentSelectedQuestID_Log 사용 ★
        // 만약 완료된 퀘스트가 현재 상세 정보 창에 표시 중이었다면 닫음
        if (questDetailUI != null && questDetailUI.gameObject.activeSelf && currentSelectedQuestID_Log == completedQuest.questID)
        {
            Debug.Log($"Closing detail panel because completed quest {completedQuest.questID} was selected.");
            questDetailUI.ClosePanel(); // 상세 정보 패널 닫기
            currentSelectedQuestID_Log = 0; // 선택된 ID 초기화
        }
    }

    /// <summary>
    /// 퀘스트 진행도 업데이트 이벤트 발생 시 호출됩니다. 목록 및 상세 정보를 업데이트합니다.
    /// </summary>
    private void UpdateQuestDetailAndLog(QuestStatus updatedStatus)
    {
        // updatedStatus가 null이면 아무것도 하지 않음
        if (updatedStatus == null)
        {
            Debug.LogWarning("UpdateQuestDetailAndLog received null QuestStatus.");
            return;
        }

        Debug.Log($"QuestLogUI received OnQuestProgressUpdated event for: {updatedStatus.questID}");
        UpdateList(); // 로그 목록은 항상 업데이트 (완료 가능 표시 등)

        // 상세 정보 패널이 열려 있고, 업데이트된 퀘스트가 현재 선택된 퀘스트와 같다면 상세 정보도 업데이트
        if (questDetailUI != null && questDetailUI.gameObject.activeSelf && currentSelectedQuestID_Log == updatedStatus.questID)
        {
            Debug.Log($"Updating detail panel for quest {updatedStatus.questID} due to progress update.");
            questDetailUI.ShowQuestDetails(updatedStatus.questID); // 상세 정보 다시 표시
        }
    }

    /// <summary>
    /// 퀘스트 해금 이벤트 발생 시 호출됩니다. (현재는 로그만 출력)
    /// </summary>
    private void HandleQuestUnlock(int unlockedQuestID)
    {
        Debug.Log($"QuestLogUI received OnQuestUnlocked event for Quest ID: {unlockedQuestID}. (No UI update implemented yet)");
        // 필요하다면 여기서 UI에 알림을 표시하거나, NPC 머리 위 아이콘 변경을 요청하는 등의 로직 추가
    }
    private void HandleTrackedQuestChanged(int newTrackedQuestID)
    {
        // Debug.Log($"QuestLogUI received OnTrackedQuestChanged: {newTrackedQuestID}"); // 필요시
        if (activeQuestListContent != null)
        {
            // 모든 목록 아이템을 순회하며 버튼 상태 업데이트 함수 호출
            foreach (Transform child in activeQuestListContent)
            {
                QuestLogItem itemUI = child.GetComponent<QuestLogItem>();
                itemUI.UpdateTrackButtonVisual(); // QuestLogItem의 함수 호출
            }
        }
    }

    /// <summary>
    /// 퀘스트 로그 패널 닫기 (UIManager 통해).
    /// </summary>
    public void ClosePanel()
    {
        // 중앙 UIManager에게 이 UI를 숨겨달라고 요청
        UIManager.Instance.HideUI<QuestLogUI>();
    }

    /// <summary>
    /// 마우스 커서 상태를 설정합니다.
    /// </summary>
    // private void SetCursorState(bool show)
    // {
    //     Cursor.visible = show;
    //     Cursor.lockState = show ? CursorLockMode.None : CursorLockMode.Locked;
    // }
}



