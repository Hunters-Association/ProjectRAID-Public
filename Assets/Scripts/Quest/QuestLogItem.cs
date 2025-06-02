using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class QuestLogItem : MonoBehaviour
{
    public TextMeshProUGUI questNameText;
    public Button selectButton;
    public Button trackButton;

    [Header("Tracking Color")]
    [Tooltip("추적 중일 때 표시할 스프라이트 색상")]
    public Color trackingColor;

    [Tooltip("추적 중이 아닐 때 표시할 스프라이트 색상")]
    public Color notTrackingColor;


    private int questID;
    private Action<int> onSelectCallback;
    private Image trackButtonImage;

    void Awake()
    {
        // ★ 추적 버튼의 Image 컴포넌트 미리 찾아두기 ★
        if (trackButton != null)
        {
            trackButtonImage = trackButton.GetComponent<Image>();
            if (trackButtonImage == null)
            {
                Debug.LogError("Track Button is missing an Image component!", trackButton);
            }
        }
    }

    public void Setup(QuestStatus status, Action<int> selectCallback)
    {
        this.questID = status.questID;
        this.onSelectCallback = selectCallback;
        if (selectCallback == null) Debug.LogError("Setup received null callback!");
        if (questNameText != null)
        {
            questNameText.text = status.questData.questName;
            questNameText.color = status.AreAllObjectivesComplete() ? Color.yellow : Color.white;
        }
        else { Debug.LogError("QuestLogItem: questNameText not assigned!", this); }
        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(OnItemSelected);
        }
        else
        {
            Debug.LogError("QuestLogItem: selectButton not assigned!", this); // ★ 참조 오류 로그 ★
        }
        if (trackButton != null)
        {
            trackButton.onClick.RemoveAllListeners(); // 이전 리스너 제거
            trackButton.onClick.AddListener(ToggleTrackQuest); // ToggleTrackQuest 함수 연결
            UpdateTrackButtonVisual(); // 초기 버튼 모양 업데이트
        }
        else
        {
            Debug.LogWarning("QuestLogItem: trackButton not assigned. Tracking feature disabled.", this);
        }
    }

    private void OnItemSelected()
    {
        Debug.Log($"QuestLogItem {questID} clicked!");
        if (onSelectCallback != null)
        {
            Debug.Log($"Calling onSelectCallback for quest ID: {questID}"); // ★ 로그 추가 ★
            onSelectCallback(questID); // ★ 콜백 함수 호출 ★
        }
        else
        {
            // ★ 만약 이 로그가 찍힌다면, Setup 함수에서 콜백 전달이 잘못된 것 ★
            Debug.LogError($"onSelectCallback is null for QuestLogItem {questID}! Check QuestLogUI.UpdateQuestLogUI setup.");
        }
    }
    // ★ 추적 버튼 클릭 시 호출될 함수 ★
    private void ToggleTrackQuest()
    {
        Debug.Log($"Track button clicked for Quest ID: {questID}"); // 클릭 로그 추가
        if (QuestManager.Instance.PlayerQuestDataManager != null && UIManager.Instance != null)
        { // PlayerDataManager 이름 확인!
            int currentTracked = QuestManager.Instance.PlayerQuestDataManager.TrackedQuestID;
            int newTrackedID = (currentTracked == this.questID) ? 0 : this.questID; // 클릭 시 추적/해제 토글

            // PlayerDataManager에 추적 상태 업데이트 요청
            QuestManager.Instance.PlayerQuestDataManager.SetTrackedQuest(newTrackedID);

            if (newTrackedID != 0)
            {
                // 새로운 퀘스트 추적 시작 -> QuestTrackerUI 표시
                UIManager.Instance.ShowUI<QuestTrackerUI>();
                Debug.Log($"Quest {newTrackedID} tracking started. Showing Tracker UI.");
            }
            else
            {
                // 퀘스트 추적 해제 -> QuestTrackerUI 숨김
                UIManager.Instance.HideUI<QuestTrackerUI>();
                Debug.Log($"Quest {this.questID} tracking stopped. Hiding Tracker UI.");
            }

            // 버튼 시각 효과는 QuestLogUI의 이벤트 핸들러에서 처리하는 것이 좋음
            // UpdateTrackButtonVisual();
        }
        else
        {
            Debug.LogError("PlayerDataManager or UIManager Instance is null! Cannot toggle track quest.");
        }
    }

    // ★ 추적 상태에 따라 버튼 모양 변경  ★
    public void UpdateTrackButtonVisual()
    {
        // ★ Image 참조 및 PlayerDataManager 확인 ★
        if (QuestManager.Instance.PlayerQuestDataManager != null) // PlayerQuestDataManager 이름 확인!
        {
            // 현재 이 퀘스트를 추적 중인지 확인
            bool isTrackingThis = QuestManager.Instance.PlayerQuestDataManager.TrackedQuestID == this.questID;
            trackButtonImage.color = isTrackingThis ? trackingColor : notTrackingColor;
        }
    }
}
