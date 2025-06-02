using UnityEngine;
using UnityEngine.UI;

public class NPCQuestUI : BaseUI, IBlockingUI // ★ BaseUI 상속 ★
{
    [Header("NPC UI References")]
    public Text npcQuestTitleText;
    public Text npcQuestDescriptionText;
    public Button acceptButton;
    public Button completeButton;
    public Button closeButton; // 닫기 버튼 추가

    private int currentQuestID;
    private NPCQuestInteraction currentNPC;

    public bool BlocksGameplay => true;

    public override void Initialize()
    {
        base.Initialize();
        if (closeButton != null) closeButton.onClick.AddListener(ClosePanel);
        // Accept/Complete 버튼 리스너는 SetupButtons에서 설정
    }

    // 외부에서 호출하여 UI 내용 설정 및 표시
    public void ShowOffer(int questID, NPCQuestInteraction npc)
    {
        if (QuestManager.Instance == null || QuestManager.Instance.Database == null)
        {
            Debug.LogError("NPCQuestUI: QuestManager 또는 QuestDatabase 인스턴스를 찾을 수 없습니다.");
            ClosePanel(); // 오류 시 패널 닫기
            return;
        }
        QuestData data = QuestManager.Instance.Database.GetQuestByID(questID);
        if (data == null)
        {
            Debug.LogError($"NPCQuestUI: Quest ID [{questID}]에 해당하는 QuestData를 찾을 수 없습니다.");
            ClosePanel();
            return;
        }
        if (npc == null) // 전달받은 npcInteraction null 체크
        {
            Debug.LogError("NPCQuestUI: ShowOffer 호출 시 npcInteraction이 null입니다.");
            ClosePanel();
            return;
        }
        currentNPC = npc;
        currentQuestID = questID;
        if (npcQuestTitleText != null) npcQuestTitleText.text = data.questName;
        if (npcQuestDescriptionText != null) npcQuestDescriptionText.text = data.description + "\n\n이 퀘스트를 수락하시겠습니까?";
        SetupButtons(true, false);
        OnShow(); // 애니메이션 재생 및 활성화
        CursorManager.SetCursorState(true); // 커서 보이기
    }

    public void ShowCompletion(int questID, NPCQuestInteraction npc)
    {
        if (QuestManager.Instance == null || QuestManager.Instance.Database == null)
        {
            Debug.LogError("NPCQuestUI: QuestManager 또는 QuestDatabase 인스턴스를 찾을 수 없습니다.");
            ClosePanel();
            return;
        }
        QuestData data = QuestManager.Instance.Database.GetQuestByID(questID);
        if (data == null)
        {
            Debug.LogError($"NPCQuestUI: Quest ID [{questID}]에 해당하는 QuestData를 찾을 수 없습니다.");
            ClosePanel();
            return;
        }
        if (npc == null)
        {
            Debug.LogError("NPCQuestUI: ShowCompletion 호출 시 npcInteraction이 null입니다.");
            ClosePanel();
            return;
        }
        currentNPC = npc;
        currentQuestID = questID;
        if (npcQuestTitleText != null) npcQuestTitleText.text = data.questName;
        if (npcQuestDescriptionText != null) npcQuestDescriptionText.text = "퀘스트를 완료했습니다! 보상을 받으세요.";
        SetupButtons(false, true);

        OnShow();

        CursorManager.SetCursorState(true);
    }

    private void SetupButtons(bool showAccept, bool showComplete)
    {
        acceptButton.gameObject.SetActive(showAccept);
        completeButton.gameObject.SetActive(showComplete);
        acceptButton.onClick.RemoveAllListeners(); if (showAccept) acceptButton.onClick.AddListener(AcceptQuest);
        completeButton.onClick.RemoveAllListeners(); if (showComplete) completeButton.onClick.AddListener(CompleteQuest);
    }

    private void AcceptQuest() { currentNPC.PlayerAcceptedQuest(currentQuestID); ClosePanel(); }
    private void CompleteQuest() { currentNPC.PlayerCompletedQuest(currentQuestID); ClosePanel(); }

    public void ClosePanel()
    {
        OnHide(); // 애니메이션 재생 및 비활성화
        currentNPC = null; currentQuestID = 0;

        CursorManager.SetCursorState(false); // 커서 숨기기
    }

    // BaseUI의 OnHide가 완료된 후 SetActive(false) 호출됨
    // public override void OnHide() { base.OnHide(); ... }

    // private void SetCursorState(bool show) { Cursor.visible = show; Cursor.lockState = show ? CursorLockMode.None : CursorLockMode.Locked; }
}
