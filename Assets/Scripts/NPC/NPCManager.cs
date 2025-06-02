using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NPCManager : MonoSingleton<NPCManager>,IInitializable
{
    [Header("동료 시스템 설정")]
    [SerializeField] private int maxActiveCompanions = 1; // 동시에 데리고 다닐 수 있는 최대 NPC 수
    //[SerializeField] private int nextSceneIndexAfterCompanionSelection = 2;

    private List<NPCController> _allNpcsInScene = new List<NPCController>(); // 씬 로드 시 채워짐
    private List<NPCController> _activeCompanions = new List<NPCController>();
    private QuestData _currentQuestForCompanionSelection; // 현재 동료 선택 UI를 띄워야 하는 퀘스트

    public NPCController PendingCompanion { get; private set; }

    private List<int> _persistedActiveCompanionIDs = new List<int>();
    private int _persistedPendingCompanionID = 1;

    public IEnumerator Initialize() // MonoSingleton의 Awake가 virtual이므로 override
    {
        // Debug.LogWarning($"[NPCManager Awake] InstanceID: {this.GetInstanceID()}. 이벤트 구독 시작.");
        SceneManager.sceneLoaded += OnSceneLoaded_HandleNpcList_Entry; // ★★★ _Entry 붙은 것으로 통일 ★★★
        SubscribeToQuestEvents();
        yield break;
    }
    private void SubscribeToQuestEvents()
    {
        if (QuestManager.Instance != null && QuestManager.Instance.PlayerQuestDataManager != null)
        {
            Debug.Log("이벤트 구독");
            QuestManager.Instance.PlayerQuestDataManager.OnQuestAccepted -= HandleQuestAccepted_ForCompanionUI; // 중복 구독 방지
            QuestManager.Instance.PlayerQuestDataManager.OnQuestAccepted += HandleQuestAccepted_ForCompanionUI;
            QuestManager.Instance.PlayerQuestDataManager.OnQuestAbandoned -= HandleQuestAbandoned_ForCompanions; // 중복 구독 방지
            QuestManager.Instance.PlayerQuestDataManager.OnQuestAbandoned += HandleQuestAbandoned_ForCompanions;

            QuestManager.Instance.PlayerQuestDataManager.OnQuestObjectivesMet -= HandleQuestObjectivesMet_ForCompanionReturn; // 중복 방지
            QuestManager.Instance.PlayerQuestDataManager.OnQuestObjectivesMet += HandleQuestObjectivesMet_ForCompanionReturn;
            //Debug.Log("[NPCManager] OnQuestObjectivesMet 이벤트 구독 완료.");
        }
        else
        {
            Debug.LogError("[NPCManager] QuestManager 또는 PlayerQuestDataManager 참조 오류 in SubscribeToQuestEvents.");
        }
    }
    private void UnsubscribeFromQuestEvents()
    {
        // Instance가 null 체크는 외부에서 이미 할 것이므로 여기서는 PlayerQuestDataManager만 체크
        if (QuestManager.Instance != null && QuestManager.Instance.PlayerQuestDataManager != null)
        {
            QuestManager.Instance.PlayerQuestDataManager.OnQuestAccepted -= HandleQuestAccepted_ForCompanionUI;
            QuestManager.Instance.PlayerQuestDataManager.OnQuestAbandoned -= HandleQuestAbandoned_ForCompanions;
        }
    }

    // void OnApplicationQuit() // DontDestroyOnLoad 사용 시 OnApplicationQuit이 더 적절할 수 있음
    // {
    //     // Debug.LogWarning($"[NPCManager OnDestroy] InstanceID: {this.GetInstanceID()}");
    //     // ▼▼▼ 이벤트 구독 해제 추가 ▼▼▼
    //     SceneManager.sceneLoaded -= OnSceneLoaded_HandleNpcList_Entry;

    //     if (QuestManager.Instance == null) return;
    //     if (QuestManager.Instance.PlayerQuestDataManager == null) return;

    //     QuestManager.Instance.PlayerQuestDataManager.OnQuestAccepted -= HandleQuestAccepted_ForCompanionUI;
    //     QuestManager.Instance.PlayerQuestDataManager.OnQuestAbandoned -= HandleQuestAbandoned_ForCompanions;

    //     QuestManager.Instance.PlayerQuestDataManager.OnQuestObjectivesMet -= HandleQuestObjectivesMet_ForCompanionReturn;
    // }



    // ▼▼▼ 퀘스트 수락 시 처리 ) ▼▼▼
    private void HandleQuestAccepted_ForCompanionUI(QuestData acceptedQuest)
    {
        if (acceptedQuest != null && acceptedQuest.isHuntQuest)
        {
            Debug.Log($"[NPCManager] 토벌 퀘스트 [{acceptedQuest.questName}] 수락됨. 동료 선택 UI 표시 시도 (씬 전환 없음).");
            ShowCompanionSelectionUI(acceptedQuest); // 콜백 없이 UI만 띄움
        }
        // else if (acceptedQuest != null) Debug.Log($"[NPCManager] 수락된 퀘스트 [{acceptedQuest.questName}]는 토벌 퀘스트가 아님.");
    }


    // ▼▼▼ 퀘스트 포기 시 처리 (신규 메서드) ▼▼▼
    private void HandleQuestAbandoned_ForCompanions(QuestData abandonedQuest)
    {
        Debug.Log("aasdf");
        if (abandonedQuest == null) return;
        Debug.Log($"[NPCManager] 퀘스트 [{abandonedQuest.questName}] 포기됨. 예비/활성 동료 상태 확인.");

        // 1. 예비 동료가 이 퀘스트와 관련되어 있다면 선택 취소
        if (PendingCompanion != null && PendingCompanion.npcData != null &&
            abandonedQuest.specificCompanionNpcIDs != null &&
            abandonedQuest.specificCompanionNpcIDs.Contains(PendingCompanion.npcData.npcID) &&
            abandonedQuest.isHuntQuest)
        {
            Debug.Log($"[NPCManager] 예비 동료 [{PendingCompanion.npcData.npcName}]가 포기된 퀘스트와 관련되어 선택 취소.");
            PendingCompanion = null;
        }

        // 2. 활성 동료 중 이 퀘스트로 인해 합류한 동료가 있다면 해제
        List<NPCController> companionsToRemove = new List<NPCController>();
        foreach (NPCController companion in _activeCompanions)
        {
            if (companion.npcData != null &&
                abandonedQuest.specificCompanionNpcIDs != null &&
                abandonedQuest.specificCompanionNpcIDs.Contains(companion.npcData.npcID) &&
                abandonedQuest.isHuntQuest)
            {
                companionsToRemove.Add(companion);
            }
        }
        foreach (NPCController npcToRemove in companionsToRemove)
        {
            RemoveActiveCompanion(npcToRemove); // 메서드 이름 변경
        }
    }
    private void OnSceneLoaded_HandleNpcList_Entry(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[NPCManager] 씬 로드됨: {scene.name}. NPC 목록 갱신 및 초기화 대기 시작.");
        StartCoroutine(EnsureNpcsInitializedAndRestoreCompanionsCoroutine(scene));
    }
    //private IEnumerator EnsureNpcsInitializedCoroutine(Scene scene) // Scene 파라미터 받음
    //{
    //    Debug.Log($"[NPCManager] 코루틴 EnsureNpcsInitializedCoroutine 시작. 씬: {scene.name}.");
    //    RefreshNpcListInScene();

    //    if (_allNpcsInScene.Count > 0)
    //    {
    //        Debug.Log($"[NPCManager] 씬의 NPC ({_allNpcsInScene.Count}명) 초기화 대기 시작...");
    //        float timeout = 5f; float waitStartTime = Time.realtimeSinceStartup;
    //        yield return new WaitUntil(() =>
    //        {
    //            if (Time.realtimeSinceStartup - waitStartTime > timeout) { Debug.LogError("[NPCManager] NPC 초기화 대기 시간 초과!"); return true; }
    //            return _allNpcsInScene.Where(npc => npc != null).All(npc => npc.IsFullyInitialized);
    //        });
    //        Debug.Log("[NPCManager] 모든 NPC 초기화 완료 또는 대기 시간 초과.");

    //        Debug.Log("[NPCManager] 대기 후, _allNpcsInScene의 NPC 상태 재확인:");
    //        foreach (NPCController npc in _allNpcsInScene)
    //        {
    //            if (npc != null && npc.npcData != null)
    //            {
    //                Debug.Log($"  - NPC: {npc.npcData.npcName} (InstanceID: {npc.GetInstanceID()}), IsFullyInitialized: {npc.IsFullyInitialized}, IsInCombatParty: {npc.IsInCombatParty}");
    //            }
    //        }
    //    }
    //    else Debug.Log("[NPCManager] 현재 씬에 NPC가 없습니다. 초기화 대기 스킵.");
    //    Debug.LogWarning($"[NPCManager EnsureNpcsInitializedCoroutine END] 실행 완료.");
    //    RestoreCompanionsAfterSceneLoad();
    //    // ▼▼▼ _currentQuestForCompanionSelection 관련 UI 호출 로직은 여기서 제거 (HandleQuestAccepted에서 직접 처리) ▼▼▼
    //}
    public void PrepareForSceneChange()
    {
        _persistedActiveCompanionIDs.Clear();
        foreach (NPCController companion in _activeCompanions)
        {
            if (companion != null && companion.npcData != null)
            {
                _persistedActiveCompanionIDs.Add(companion.npcData.npcID);
            }
        }

        _persistedPendingCompanionID = (PendingCompanion != null && PendingCompanion.npcData != null)
                                        ? PendingCompanion.npcData.npcID
                                        : -1; // PendingCompanion이 없으면 -1

        Debug.Log($"[NPCManager] PrepareForSceneChange 완료. 유지할 활성 동료 ID 수: {_persistedActiveCompanionIDs.Count}, 예비 동료 ID: {_persistedPendingCompanionID}");
        
    }
    private void RestoreCompanionsAfterSceneLoad()
    {
        Debug.Log($"[NPCManager] RestoreCompanionsAfterSceneLoad 시작. 저장된 활성 동료 ID 수: {_persistedActiveCompanionIDs.Count}, 예비 동료 ID: {_persistedPendingCompanionID}");
        _activeCompanions.Clear(); // 이전 씬의 GameObject 참조는 더 이상 유효하지 않으므로 목록을 비웁니다.

        // 저장된 활성 동료 ID를 기반으로 새 씬에서 NPC를 찾아 활성 동료 목록에 추가
        foreach (int companionID in _persistedActiveCompanionIDs)
        {
            NPCController npc = GetNPCByID(companionID); // _allNpcsInScene 목록에서 ID로 NPC를 찾습니다.
            if (npc != null && npc.IsFullyInitialized) // NPC가 새 씬에 존재하고 초기화가 완료되었는지 확인
            {
                Debug.Log($"[NPCManager] 복원 시도 대상 NPC: {npc.npcData.npcName} (ID: {companionID}), IsInCombatParty: {npc.IsInCombatParty}");
                if (npc.IsInCombatParty) // 전투 참여 자격이 여전히 유효한지 확인
                {
                    _activeCompanions.Add(npc);
                    npc.SetPlayerFollowingStatus(true); // 다시 플레이어를 따라다니도록 설정
                    Debug.Log($"[NPCManager] 이전 활성 동료 [{npc.npcData.npcName}] 복원 및 추종 시작됨.");
                }
                else
                {
                    Debug.LogWarning($"[NPCManager] 이전 활성 동료였던 [{npc.npcData?.npcName ?? "ID: " + companionID.ToString()}]가 새 씬에서 전투 참여 자격을 잃어 복원되지 않음. (IsInCombatParty: false)");
                }
            }
            else
            {
                Debug.LogWarning($"[NPCManager] 이전 활성 동료 ID [{companionID}]를 새 씬에서 찾지 못했거나 아직 초기화되지 않음 (IsFullyInitialized: {npc?.IsFullyInitialized}).");
            }
        }
        _persistedActiveCompanionIDs.Clear(); // 복원 후 임시 목록을 비웁니다.

        // 저장된 예비 동료 ID를 기반으로 새 씬에서 NPC를 찾아 예비 동료로 설정
        if (_persistedPendingCompanionID != -1)
        {
            NPCController npc = GetNPCByID(_persistedPendingCompanionID);
            if (npc != null && npc.IsFullyInitialized)
            {
                Debug.Log($"[NPCManager] 예비 동료 복원 시도 대상 NPC: {npc.npcData.npcName} (ID: {_persistedPendingCompanionID}), IsInCombatParty: {npc.IsInCombatParty}");
                if (npc.IsInCombatParty) // 예비 동료도 전투 참여 자격이 있어야 함
                {
                    PendingCompanion = npc; // SelectPendingCompanion을 직접 호출하기보다 상태만 복원
                    Debug.Log($"[NPCManager] 이전 예비 동료 [{npc.npcData.npcName}] 복원됨.");
                }
                else
                {
                    Debug.LogWarning($"[NPCManager] 이전 예비 동료였던 [{npc.npcData?.npcName ?? "ID: " + _persistedPendingCompanionID.ToString()}]가 새 씬에서 전투 참여 자격을 잃어 복원되지 않음.");
                    PendingCompanion = null; // 자격 없으면 예비 동료 해제
                }
            }
            else
            {
                // Debug.LogWarning($"[NPCManager] 이전 예비 동료 ID [{_persistedPendingCompanionID}]를 새 씬에서 찾지 못했거나 아직 초기화되지 않음.");
                PendingCompanion = null;
            }
        }
        _persistedPendingCompanionID = -1; // 복원 후 임시 ID를 비웁니다.
    }
    private void HandleQuestObjectivesMet_ForCompanionReturn(QuestData metQuest)
    {
        if (metQuest == null)
        {
            Debug.LogWarning("[NPCManager] HandleQuestObjectivesMet: metQuest가 null입니다.");
            return;
        }

        Debug.Log($"[NPCManager] 퀘스트 [{metQuest.questName}] (ID: {metQuest.questID}) 목표 달성됨. 관련 동료 복귀 처리 시작.");

        // 어떤 NPC를 복귀시킬지 결정하는 로직:
        // 시나리오 1: 이 퀘스트를 위해 특별히 지정된 동료가 있었고, 그 동료가 현재 활성 동료라면 복귀.
        // 시나리오 2: 이 퀘스트의 완료 보고를 받아야 하는 NPC가 현재 활성 동료라면 복귀. (덜 일반적)
        // 시나리오 3: 퀘스트를 준 NPC가 현재 활성 동료라면 복귀.

        // 여기서는 "이 퀘스트와 연관되어 현재 플레이어를 따라다니는 NPC"를 대상으로 합니다.
        // QuestData에 어떤 NPC가 이 퀘스트의 "주요 NPC"인지 나타내는 필드가 필요합니다.
        // 예를 들어, QuestData에 'int associatedNpcID' 또는 'List<int> keyNpcIDsForQuest' 같은 필드가 있다고 가정합니다.
        // 또는, 가장 간단하게는 퀘스트를 준 NPC (questGiverNpcID) 또는 보고 대상 NPC (reportToNpcID)를 기준으로 할 수 있습니다.

        List<NPCController> companionsToReturn = new List<NPCController>();

        // 예시: 퀘스트를 준 NPC (questGiverNpcID)가 현재 동료라면 그 NPC를 복귀 대상으로 추가
        // QuestData에 questGiverNpcID 필드가 있고, 0이 아니라고 가정합니다.
        if (metQuest.questGiverID != 0) // 퀘스트 제공자 ID가 유효한 경우
        {
            NPCController giverCompanion = _activeCompanions.FirstOrDefault(c => c.npcData != null && c.npcData.npcID == metQuest.questGiverID);
            if (giverCompanion != null && !companionsToReturn.Contains(giverCompanion)) // 아직 목록에 없고, 현재 활성 동료라면
            {
                companionsToReturn.Add(giverCompanion);
                Debug.Log($"[NPCManager] 퀘스트 제공자 [{giverCompanion.npcData.npcName}]가 현재 동료이므로 복귀 대상에 추가.");
            }
        }

        // 예시: 퀘스트 보고 대상 NPC (reportToNpcID)가 현재 동료이고, 퀘스트 제공자와 다른 경우 그 NPC도 복귀 대상으로 추가
        // QuestData에 reportToNpcID 필드가 있고, 0이 아니라고 가정합니다.
        if (metQuest.questCompleterID != 0 && metQuest.questCompleterID != metQuest.questGiverID) // 보고 대상이 있고, 제공자와 다른 경우
        {
            NPCController reporterCompanion = _activeCompanions.FirstOrDefault(c => c.npcData != null && c.npcData.npcID == metQuest.questCompleterID);
            if (reporterCompanion != null && !companionsToReturn.Contains(reporterCompanion))
            {
                companionsToReturn.Add(reporterCompanion);
                Debug.Log($"[NPCManager] 퀘스트 보고 대상 [{reporterCompanion.npcData.npcName}]가 현재 동료이므로 복귀 대상에 추가.");
            }
        }

        // 예시: 이 퀘스트에 특별히 지정된 동료가 있었고 (specificCompanionNpcIDs), 그들이 현재 활성 동료라면 복귀 대상에 추가
        // 이는 토벌 퀘스트 등에서 특정 NPC와 동행했을 경우에 해당될 수 있습니다.
        if (metQuest.specificCompanionNpcIDs != null && metQuest.specificCompanionNpcIDs.Count > 0)
        {
            foreach (int specificNpcID in metQuest.specificCompanionNpcIDs)
            {
                NPCController specificCompanion = _activeCompanions.FirstOrDefault(c => c.npcData != null && c.npcData.npcID == specificNpcID);
                if (specificCompanion != null && !companionsToReturn.Contains(specificCompanion))
                {
                    companionsToReturn.Add(specificCompanion);
                    Debug.Log($"[NPCManager] 퀘스트 지정 동료 [{specificCompanion.npcData.npcName}]가 현재 동료이므로 복귀 대상에 추가.");
                }
            }
        }


        if (companionsToReturn.Count == 0)
        {
            Debug.Log($"[NPCManager] 퀘스트 [{metQuest.questName}] 목표 달성. 복귀시킬 현재 활성 동료가 없습니다.");
            return;
        }

        // 선택된 동료들을 원래 위치로 복귀시키고 활성 동료 목록에서 제거 준비
        foreach (NPCController npcToReturn in companionsToReturn)
        {
            Debug.Log($"[NPCManager] 퀘스트 [{metQuest.questName}] 목표 달성. 동료 [{npcToReturn.npcData.npcName}]를 원래 위치로 복귀시킵니다 (플레이어 추종 중단).");
            npcToReturn.SetPlayerFollowingStatus(false); // NPC는 ReturningToPost 상태로 전환됨

            // 중요: SetPlayerFollowingStatus(false) 호출 후, 해당 NPC가 _activeCompanions 리스트에서 즉시 제거되어야 하는지,
            // 아니면 ReturningToPost 상태가 완료된 후 (Idle 상태 진입 시) 제거되어야 하는지 결정해야 합니다.
            // 여기서는 SetPlayerFollowingStatus(false)를 호출하여 복귀를 시작시키고,
            // _activeCompanions 리스트에서는 명시적으로 제거합니다.
            // 이렇게 하면 PrepareForSceneChange에서 이 NPC가 더 이상 _persistedActiveCompanionIDs에 포함되지 않습니다.
            if (_activeCompanions.Contains(npcToReturn))
            {
                _activeCompanions.Remove(npcToReturn);
                Debug.Log($"[NPCManager] 동료 [{npcToReturn.npcData.npcName}]를 활성 동료 목록에서 제거했습니다.");
            }
        }

        // 만약 퀘스트 목표 달성 시 모든 동료를 해산시켜야 한다면, 위 로직 대신 아래와 같이 할 수 있습니다:
        /*
        if (_activeCompanions.Count > 0)
        {
            Debug.Log($"[NPCManager] 퀘스트 [{metQuest.questName}] 목표 달성. 모든 활성 동료를 복귀시킵니다.");
            List<NPCController> companionsToDismiss = new List<NPCController>(_activeCompanions); // 복사본 사용
            foreach (NPCController companion in companionsToDismiss)
            {
                companion.SetPlayerFollowingStatus(false);
                _activeCompanions.Remove(companion); // 명시적으로 제거
            }
        }
        */
    }


    // ▼▼▼ 동료 선택 UI 표시 로직  ▼▼▼
    private void ShowCompanionSelectionUI(QuestData forQuest)
    {
        if (forQuest == null) { Debug.LogError("[NPCManager] ShowCompanionSelectionUI: forQuest가 null입니다."); return; }
        Debug.Log($"[NPCManager] ShowCompanionSelectionUI 호출됨. Quest: {forQuest.questName}");

        // ▼▼▼ ShowCompanionUIAfterRefreshAndShortWait 호출 시 콜백 함수 전달 ▼▼▼
        StartCoroutine(ShowCompanionUIAfterRefreshAndShortWait(forQuest, (selectedNpc) =>
        {
            // 이 부분이 CompanionSelectionUI가 닫힌 후 실행될 콜백 내용입니다.
            if (selectedNpc != null)
            {
                Debug.Log($"[NPCManager] CompanionSelectionUI에서 NPC [{selectedNpc.npcData.npcName}] 선택 완료. 동료 확정 및 활성화 시도.");
                bool activated = ActivateNewCompanion(selectedNpc); // ★★★ 선택된 NPC를 즉시 활성화 ★★★
                if (activated)
                {
                    Debug.Log($"[NPCManager] NPC [{selectedNpc.npcData.npcName}] 동료로 활성화 완료.");
                }
                else
                {
                    Debug.LogWarning($"[NPCManager] NPC [{selectedNpc.npcData.npcName}] 동료 활성화 실패 (예: 슬롯 부족 또는 자격 미달).");
                }
            }
            else
            {
                Debug.Log("[NPCManager] CompanionSelectionUI에서 NPC 선택 없이 닫힘 (취소됨).");
                // PendingCompanion = null; // 혹시 이전 로직에서 PendingCompanion을 설정했다면 여기서 초기화
            }
        }));
    }
    private IEnumerator ShowCompanionUIAfterRefreshAndShortWait(QuestData forQuest, Action<NPCController> onSelectionCompleteCallback)
    {
        RefreshNpcListInScene();
        if (_allNpcsInScene.Count > 0) yield return new WaitForEndOfFrame();

        List<NPCController> availableNpcs = FindAvailableCompanions(forQuest);
        if (availableNpcs.Count == 0)
        {
            Debug.Log("[NPCManager] 동행 가능한 NPC가 없습니다.");
            onSelectionCompleteCallback?.Invoke(null); // ★★★ 선택 가능한 NPC 없음 알림 ★★★
            yield break;
        }
        if (UIManager.Instance == null) { Debug.LogError("[NPCManager] UIManager.Instance is null!"); onSelectionCompleteCallback?.Invoke(null); yield break; }

        CompanionSelectionUI ui = UIManager.Instance.ShowUI<CompanionSelectionUI>(true);
        if (ui != null)
        {
            Debug.Log($"[NPCManager] CompanionSelectionUI 표시 요청. NPC 목록 전달: {availableNpcs.Count}명");
            ui.Setup(availableNpcs, onSelectionCompleteCallback); // ★★★ 콜백을 UI에 전달 ★★★
        }
        else { Debug.LogError("[NPCManager] UIManager.ShowUI<CompanionSelectionUI>가 null 반환!"); onSelectionCompleteCallback?.Invoke(null); }
    }

    // ▼▼▼ 동행 가능한 NPC 찾기 로직 (신규 메서드) ▼▼▼
    private List<NPCController> FindAvailableCompanions(QuestData forQuest)
    {
        if (forQuest == null)
        {
            Debug.LogWarning("[NPCManager FindAvailable] forQuest가 null입니다. 빈 리스트 반환.");
            return new List<NPCController>();
        }
        Debug.Log($"[NPCManager] FindAvailableCompanions 호출됨. Quest: {forQuest.questName}. _allNpcsInScene 수: {_allNpcsInScene.Count}");

        List<NPCController> potentialCompanions = new List<NPCController>();
        foreach (NPCController npc in _allNpcsInScene) // 미리 수집된 NPC 목록 사용
        {
            if (npc == null || npc.npcData == null || !npc.gameObject.activeInHierarchy) continue;
            if (!npc.gameObject.activeInHierarchy) { Debug.Log($"[NPCManager FindAvailable] NPC [{npc.npcData.npcName}]는 비활성화 상태입니다. 후보 제외."); continue; }

            // 조건 1: NPC가 전투 참여 '자격'이 있는가? (호감도, 개인 퀘스트 등)
            if (!npc.IsInCombatParty)
            {
                Debug.Log($"[NPCManager FindAvailable] NPC [{npc.npcData.npcName}]는 IsInCombatParty가 false입니다. 후보 제외.");
                continue;
            }

            // 조건 2: 이미 현재 동행 중인 NPC가 아닌가?
            if (_activeCompanions.Contains(npc))
            {
                Debug.Log($"[NPCManager FindAvailable] NPC [{npc.npcData.npcName}]는 이미 동행 중입니다. 후보 제외.");
                continue;
            }

            // 조건 3: (선택적) 이 토벌 퀘스트에서 특정 NPC만 허용하는 경우
            if (forQuest.specificCompanionNpcIDs != null && forQuest.specificCompanionNpcIDs.Count > 0)
            {
                if (!forQuest.specificCompanionNpcIDs.Contains(npc.npcData.npcID))
                {
                    Debug.Log($"[NPCManager FindAvailable] NPC [{npc.npcData.npcName}]는 퀘스트의 specificCompanionNpcIDs에 없습니다. 후보 제외.");
                    continue;
                }
            }
            // 조건 4: (선택적) NPC가 기절 상태가 아닌지 등 추가 조건

            potentialCompanions.Add(npc);
        }
        Debug.Log($"[NPCManager] 토벌 퀘스트 [{forQuest.questName}]에 동행 가능 후보 NPC 수: {potentialCompanions.Count}");
        return potentialCompanions;
    }

    // ▼▼▼ 동료 추가 시도 ( CompanionSelectionUI에서 호출) ▼▼▼
    public bool SelectPendingCompanion(NPCController npcToSelect)
    {
        // 이 함수는 UI에서 직접 호출되기보다, UI의 콜백을 통해 선택된 NPC가 바로 ActivateNewCompanion으로 갈 수 있습니다.
        // 만약 UI가 여전히 이 함수를 호출한다면, PendingCompanion에 잠시 저장하는 역할만 합니다.
        if (npcToSelect == null || !npcToSelect.IsInCombatParty)
        {
            PendingCompanion = null;
            return false;
        }
        PendingCompanion = npcToSelect;
        Debug.Log($"[NPCManager] NPC [{npcToSelect.npcData.npcName}] 예비 동료로 임시 선택됨 (다음 확정 단계 필요 시).");
        return true;
    }
    public bool ConfirmAndActivateCompanion() // 이 함수는 이제 SceneLoader에서 호출되지 않음
    {
        if (PendingCompanion == null)
        {
            Debug.Log("[NPCManager] 확정할 예비 동료가 없습니다 (ConfirmAndActivateCompanion).");
            return false;
        }
        bool result = ActivateNewCompanion(PendingCompanion);
        if (result)
        {
            PendingCompanion = null; // 확정 후 예비 동료 상태 해제
        }
        return result;
    }
    public bool ActivateNewCompanion(NPCController npcToActivate)
    {
        if (npcToActivate == null)
        {
            Debug.LogWarning("[NPCManager ActivateNewCompanion] 활성화할 NPC가 null입니다.");
            return false;
        }
        if (!npcToActivate.IsInCombatParty)
        {
            Debug.LogWarning($"[NPCManager ActivateNewCompanion] NPC [{npcToActivate.npcData.npcName}]는 전투 참여 자격이 없어 동료로 활성화할 수 없습니다.");
            return false;
        }
        if (_activeCompanions.Contains(npcToActivate))
        {
            Debug.Log($"[NPCManager ActivateNewCompanion] NPC [{npcToActivate.npcData.npcName}]는 이미 활성 동료입니다.");
            npcToActivate.SetPlayerFollowingStatus(true); // 확실히 따라오도록
            return true;
        }
        if (_activeCompanions.Count < maxActiveCompanions)
        {
            _activeCompanions.Add(npcToActivate);
            npcToActivate.SetPlayerFollowingStatus(true); // ★★★ 여기서 실제로 따라다니기 시작 ★★★
            Debug.Log($"[NPCManager ActivateNewCompanion] NPC [{npcToActivate.npcData.npcName}] 동료로 추가 및 활성화됨. 현재 동료 수: {_activeCompanions.Count}");
            return true;
        }
        else
        {
            Debug.LogWarning($"[NPCManager ActivateNewCompanion] 최대 동료 수({maxActiveCompanions}) 도달. NPC [{npcToActivate.npcData.npcName}] 활성화 불가.");
            // TODO: 플레이어에게 알림
            return false;
        }
    }
    private IEnumerator EnsureNpcsInitializedAndRestoreCompanionsCoroutine(Scene scene)
    {
        Debug.Log($"[NPCManager] 코루틴 EnsureNpcsInitializedAndRestoreCompanionsCoroutine 시작. 씬: {scene.name}.");
        RefreshNpcListInScene(); // 새 씬의 NPC 목록 가져오기

        if (_allNpcsInScene.Count > 0)
        {
            Debug.Log($"[NPCManager] 씬의 NPC ({_allNpcsInScene.Count}명) 초기화 대기 시작...");
            float timeout = 5f; float waitStartTime = Time.realtimeSinceStartup;
            yield return new WaitUntil(() =>
            {
                if (Time.realtimeSinceStartup - waitStartTime > timeout) { Debug.LogError("[NPCManager] NPC 초기화 대기 시간 초과!"); return true; }
                // 활성화된 NPC들만 초기화 완료를 기다립니다.
                return _allNpcsInScene.Where(npc => npc != null && npc.gameObject.activeInHierarchy).All(npc => npc.IsFullyInitialized);
            });
            Debug.Log("[NPCManager] 모든 활성 NPC 초기화 완료 또는 대기 시간 초과.");

            // ▼▼▼ NPCSkillUser에 각 NPC 스킬 정보 등록 (추가된 부분) ▼▼▼
            if (GameManager.Instance != null) // GameManager 인스턴스 확인
            {
                NPCSkillUser centralSkillUser = GameManager.Instance.GetComponent<NPCSkillUser>();
                if (centralSkillUser != null)
                {
                    Debug.Log("[NPCManager] GameManager의 NPCSkillUser에 씬의 NPC 스킬 정보 등록 시작...");
                    foreach (NPCController npcInScene in _allNpcsInScene)
                    {
                        if (npcInScene != null && npcInScene.IsFullyInitialized && npcInScene.gameObject.activeInHierarchy) // 활성화되고 초기화된 NPC만 등록
                        {
                            // Debug.Log($"[NPCManager] NPC [{npcInScene.npcData?.npcName ?? "Unknown"}]에 대해 RegisterAndInitializeNpc 호출 시도.");
                            centralSkillUser.RegisterAndInitializeNpc(npcInScene);
                        }
                    }
                    Debug.Log("[NPCManager] NPCSkillUser에 씬의 NPC 스킬 정보 등록 완료.");
                }
                else
                {
                    Debug.LogError("[NPCManager] GameManager에 NPCSkillUser 컴포넌트가 없습니다! NPC 스킬 초기화 불가.");
                }
            }
            else
            {
                Debug.LogError("[NPCManager] GameManager 인스턴스가 없습니다! NPC 스킬 초기화 불가.");
            }
            // ▲▲▲ NPCSkillUser 등록 로직 끝 ▲▲▲
        }
        else
        {
            Debug.Log("[NPCManager] 현재 씬에 NPC가 없습니다. 초기화 및 스킬 등록 대기 스킵.");
        }

        // 동료 상태 복원은 NPC 스킬 정보가 NPCSkillUser에 등록된 이후에 수행
        RestoreCompanionsAfterSceneLoad();

        // Debug.LogWarning($"[NPCManager EnsureNpcsInitializedAndRestoreCompanionsCoroutine END] 실행 완료.");
    }


    // ▼▼▼ 동료 해제 (신규 메서드, QuestDetailUI 또는 다른 곳에서 호출 가능) ▼▼▼
    public void RemoveActiveCompanion(NPCController npcToRemove)
    {
        if (npcToRemove != null && _activeCompanions.Remove(npcToRemove))
        {
            npcToRemove.SetPlayerFollowingStatus(false);
            Debug.Log($"[NPCManager] 활성 동료 [{npcToRemove.npcData.npcName}] 해제됨.");
        }
    }


    // ▼▼▼ 씬의 NPC 목록 갱신 ▼▼▼
    private void RefreshNpcListInScene()
    {
        _allNpcsInScene.Clear();
        _allNpcsInScene.AddRange(FindObjectsOfType<NPCController>(true)); // 비활성 포함 모든 NPC 찾기
        Debug.Log($"[NPCManager] 현재 씬의 NPC 목록 갱신 완료. 찾은 NPC 수: {_allNpcsInScene.Count}");
    }


    // ▼▼▼  특정 ID의 NPC를 찾아 반환하는 메서드 ▼▼▼
    public NPCController GetNPCByID(int npcID)
    {
        return _allNpcsInScene.FirstOrDefault(npc => npc.npcData != null && npc.npcData.npcID == npcID);
    }

}
