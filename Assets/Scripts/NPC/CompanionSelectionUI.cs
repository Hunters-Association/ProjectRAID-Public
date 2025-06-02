using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CompanionSelectionUI : BaseUI, IBlockingUI 
{
    [Header("UI References")]
    [SerializeField] private Transform npcListContent; // NPC 버튼들이 생성될 부모
    [SerializeField] private GameObject npcSelectionButtonPrefab; // NPC 선택 버튼 프리팹
    [SerializeField] private Button testNPCButton;
    [SerializeField] private Button closeButton; // 또는 확인 버튼

    private List<NPCController> _displayedNpcs = new List<NPCController>();
    private Action<NPCController> _onNpcConfirmed;
    public bool BlocksGameplay => true;

    public override void Initialize()
    {
        base.Initialize();
        closeButton?.onClick.AddListener(HandleCloseOrConfirm);
        // npcSelectionButtonPrefab null 체크
        // if (npcSelectionButtonPrefab == null) Debug.LogError("CompanionSelectionUI: npcSelectionButtonPrefab is not assigned!");
    }
    public void Setup(List<NPCController> availableNpcs, Action<NPCController> onNpcConfirmedCallback)
    {
        PopulateNPCList(availableNpcs);
        _onNpcConfirmed = onNpcConfirmedCallback; // 전달받은 콜백 저장
    }

    public void Setup(List<NPCController> availableNpcs)
    {
        PopulateNPCList(availableNpcs);
    }

    public void PopulateNPCList(List<NPCController> availableNpcs)
    {
        testNPCButton.onClick.AddListener(() => OnNPCSelected(availableNpcs[0]));

        if (npcListContent == null || npcSelectionButtonPrefab == null) return;

        // 기존 버튼들 제거
        foreach (Transform child in npcListContent)
        {
            Destroy(child.gameObject);
        }
        _displayedNpcs.Clear();

        if (availableNpcs == null || availableNpcs.Count == 0)
        {
            // TODO: 동행 가능한 NPC가 없을 때의 메시지 표시
            Debug.Log("[CompanionSelectionUI] 표시할 NPC가 없습니다.");
            // 필요하다면 이 UI를 바로 닫거나, 메시지 표시 후 닫도록 처리
            Close();
            return;
        }

        foreach (NPCController npc in availableNpcs)
        {
            if (npc == null || npc.npcData == null) continue;

            GameObject buttonGO = Instantiate(npcSelectionButtonPrefab, npcListContent);
            Button npcButton = buttonGO.GetComponent<Button>();
            TextMeshProUGUI buttonText = buttonGO.GetComponentInChildren<TextMeshProUGUI>(); // 프리팹 구조에 맞게

            if (buttonText != null)
            {
                buttonText.text = npc.npcData.npcName; // NPC 이름 표시
            }
            if (npcButton != null)
            {
                // 클로저 문제를 피하기 위해 지역 변수 사용
                NPCController capturedNpc = npc;
                npcButton.onClick.AddListener(() => OnNPCSelected(capturedNpc));
            }
            _displayedNpcs.Add(npc);
        }
    }

    private void OnNPCSelected(NPCController selectedNpc)
    {
        Debug.Log($"[CompanionSelectionUI] NPC [{selectedNpc.npcData.npcName}] 선택됨.");
        //NPCManager.Instance?.SelectPendingCompanion(selectedNpc); // 선택 정보만 NPCManager에 저장
        _onNpcConfirmed?.Invoke(selectedNpc);
        Close(); // UI 닫기
    }

    private void HandleCloseOrConfirm()
    {
        Debug.Log("[CompanionSelectionUI] UI 닫기 (선택된 NPC 없음 또는 취소).");
        // NPCManager.Instance?.ClearPendingCompanion(); // 선택적으로 예비 동료 선택 취소
        _onNpcConfirmed?.Invoke(null);
        Close(); 
    }
}
