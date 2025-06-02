using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestTrackerUI : BaseUI // BaseUI 상속
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI questTitleText; // Inspector 연결
    [SerializeField] private TextMeshProUGUI questObjectivesText; // Inspector 연결

    private int trackedQuestID = 0; // 현재 추적 중인 퀘스트 ID (0이면 추적 안 함)

    // PlayerDataManager 참조 (이벤트 구독/데이터 접근용)
    private PlayerQuestDataManager dataManager;

    public override void Initialize()
    {
        base.Initialize();
        // 참조 확인
        if (questTitleText == null) Debug.LogError("QuestTrackerUI: questTitleText not assigned!");
        if (questObjectivesText == null) Debug.LogError("QuestTrackerUI: questObjectivesText not assigned!");

        // PlayerDataManager 인스턴스 가져오기 
        dataManager = QuestManager.Instance.PlayerQuestDataManager;
        if (dataManager == null)
        {
            Debug.LogError("QuestTrackerUI: PlayerDataManager Instance not found!");
            gameObject.SetActive(false); // 매니저 없으면 비활성화
        }
    }

    // OnEnable/OnDisable 대신 UIManager의 Show/Hide 관리 방식 사용
    // 또는 OnEnable/Disable에서 이벤트 구독/해제

    public override void OnShow()
    {
        Debug.Log("--- QuestTrackerUI OnShow called! ---"); // ★ 추가 ★
        base.OnShow();
        SubscribeToEvents();
        // ★ 표시될 때 현재 추적 중인 퀘스트 정보로 업데이트 ★
        
        UpdateTrackedQuestInfo();
    }

    public override void OnHide()
    {
        base.OnHide();
        UnsubscribeFromEvents();
    }

    // 이벤트 구독/해제 함수
    private void SubscribeToEvents()
    {
        if (dataManager != null)
        {
            // 퀘스트 상태가 변경될 수 있는 모든 관련 이벤트 구독
            dataManager.OnQuestAccepted += HandleQuestDataChanged;
            dataManager.OnQuestCompleted += HandleQuestDataChanged;
            dataManager.OnQuestProgressUpdated += HandleQuestProgressUpdated;
            dataManager.OnQuestAbandoned += HandleQuestDataChanged;
            dataManager.OnTrackedQuestChanged += HandleTrackedQuestChanged; 
        }
    }
    private void UnsubscribeFromEvents()
    {
        if (dataManager != null)
        {
            dataManager.OnQuestAccepted -= HandleQuestDataChanged;
            dataManager.OnQuestCompleted -= HandleQuestDataChanged;
            dataManager.OnQuestProgressUpdated -= HandleQuestProgressUpdated;
            dataManager.OnQuestAbandoned -= HandleQuestDataChanged;
            dataManager.OnTrackedQuestChanged -= HandleTrackedQuestChanged;
        }
    }

    // --- 이벤트 핸들러 ---

    /// <summary> 퀘스트 수락/완료/포기 시 호출되어 추적 정보 업데이트 </summary>
    private void HandleQuestDataChanged(QuestData changedQuest)
    {
        // 현재 추적 중인 퀘스트가 완료/포기되었거나,
        // 새로 수락한 퀘스트가 추적 대상일 수 있으므로 업데이트
        UpdateTrackedQuestInfo();
    }

    /// <summary> 퀘스트 진행도 업데이트 시 호출 </summary>
    private void HandleQuestProgressUpdated(QuestStatus updatedStatus)
    {
        // 업데이트된 퀘스트가 현재 추적 중인 퀘스트인지 확인
        if (updatedStatus != null && updatedStatus.questID == trackedQuestID)
        {
            // 맞다면 UI 내용 새로고침
            UpdateUIContent(updatedStatus);
        }
    }

    /// <summary> (추가 구현 필요) 추적 퀘스트 변경 이벤트 핸들러 </summary>
    private void HandleTrackedQuestChanged(int newTrackedQuestID)
    {
        trackedQuestID = newTrackedQuestID;
        UpdateTrackedQuestInfo();
    }


    // --- UI 업데이트 로직 ---

    /// <summary> 현재 추적 중인 퀘스트 정보를 찾아 UI를 업데이트합니다. </summary>
    private void UpdateTrackedQuestInfo()
    {
        // TODO: ★ 실제 추적 중인 퀘스트 ID를 가져오는 로직 필요 ★
        // 예시 1: PlayerDataManager에 trackedQuestID 변수 및 Get/Set 메서드 추가
        // trackedQuestID = dataManager.QuestData?.GetTrackedQuestID() ?? 0;
        // 예시 2: 가장 최근에 수락한 활성 퀘스트 자동 추적
        if (dataManager?.QuestData?.activeQuests != null && dataManager.QuestData.activeQuests.Count > 0)
        {
            // 가장 마지막에 추가된 퀘스트 ID를 가져온다고 가정 (순서 보장 안 될 수 있음)
            // trackedQuestID = dataManager.QuestData.activeQuests.Keys.LastOrDefault();
            // 또는 퀘스트 ID가 가장 큰 것을 추적? -> 규칙 정의 필요
            trackedQuestID = dataManager.QuestData.activeQuests.Keys.Max(); // 임시: ID 가장 큰 활성 퀘스트 추적
        }
        else
        {
            trackedQuestID = 0; // 활성 퀘스트 없으면 추적 안 함
        }


        // 추적할 퀘스트가 있다면 해당 정보로 UI 업데이트
        if (trackedQuestID != 0 && dataManager.QuestData.activeQuests.TryGetValue(trackedQuestID, out QuestStatus status))
        {
            UpdateUIContent(status);
            // UI 자체를 보이게 (BaseUI.OnShow에서 이미 처리됨)
            // gameObject.SetActive(true); // 불필요
        }
        else // 추적할 퀘스트가 없으면 UI 숨기기
        {
            ClearUIContent();
            // gameObject.SetActive(false); // UIManager.HideUI() 가 담당
            // 또는 빈 상태로 보이게 할 수도 있음
        }
    }

    /// <summary> 주어진 퀘스트 상태 정보로 UI 내용을 채웁니다. </summary>
    private void UpdateUIContent(QuestStatus status)
    {
        if (status?.questData == null) { ClearUIContent(); return; }

        if (questTitleText != null)
        {
            questTitleText.text = status.questData.questName; // 퀘스트 제목 표시
        }

        if (questObjectivesText != null)
        {
            // 목표 텍스트 생성 (QuestDetailUI와 유사)
            StringBuilder objectivesSB = new StringBuilder();
            if (status.questData.objectives != null)
            {
                for (int i = 0; i < status.questData.objectives.Count; i++)
                {
                    if (status.questData.objectives[i] != null)
                    {
                        // ★ 목표 설명만 간략하게 표시 (GetProgressDescription 대신) ★
                        string objectiveDesc = status.questData.objectives[i].description; // 기본 설명
                        int current = status.objectiveProgress[i];
                        int required = status.questData.objectives[i].requiredCount;
                        objectivesSB.AppendLine($"{objectiveDesc} ({current}/{required})"); // 진행도 함께 표시
                    }
                }
            }
            else { objectivesSB.AppendLine("목표 없음"); }
            questObjectivesText.text = objectivesSB.ToString(); // 최종 목표 텍스트 할당
        }
    }

    /// <summary> UI 내용을 비웁니다. </summary>
    private void ClearUIContent()
    {
        if (questTitleText != null) questTitleText.text = "";
        if (questObjectivesText != null) questObjectivesText.text = "";
    }
}
