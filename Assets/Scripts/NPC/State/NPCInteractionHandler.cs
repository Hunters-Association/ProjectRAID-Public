using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInteractionHandler : MonoBehaviour, IInteractable 
{
    private NPCController _npcController;

    public void Initialize(NPCController controller)
    {
        _npcController = controller;
    }

    // --- IInteractable 인터페이스 구현 ---
    public void Interact(PlayerController player)
    {
        if (_npcController == null || _npcController.npcData == null) return;
        Debug.Log($"Player [{player.name}] interacts with NPC [{_npcController.npcData.npcName}]");
        // TODO: 대화 시작, 퀘스트 UI 열기 등 실제 상호작용 로직
        // 예: DialogueManager.Instance.StartDialogue(_npcController.npcData.defaultDialogueID);
        // 예: UIManager.Instance.ShowUI<NPCQuestOfferUI>(_npcController.GetAvailableQuest());
    }

    public void ShowHighlight()
    {
        // TODO: NPC 하이라이트 표시 로직
    }

    public void HideHighlight()
    {
        // TODO: NPC 하이라이트 숨김 로직
    }

    [SerializeField] private InteractableData data;

    public InteractableData GetInteractableData()
    {
        if (data == null) Debug.LogWarning("[NPCQuestInteraction] 상호작용 데이터가 등록되지 않았습니다.");
        return data;
    }
}
