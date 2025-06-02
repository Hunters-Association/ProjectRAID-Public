using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NPCAffinity : MonoBehaviour
{
    private NPCController _npcController;
    [Tooltip("이 NPC의 호감도 레벨 정의 (AffinityLevelData SO 목록)")]
    public List<AffinityLevelData> affinityLevelDefinitions;

    [SerializeField] // 디버깅용
    private int _currentAffinityPoints = 0;
    public int CurrentAffinityPoints => _currentAffinityPoints;

    [SerializeField] // 디버깅용
    private int _currentAffinityLevel = 0;
    public int CurrentAffinityLevel => _currentAffinityLevel;

    public event Action<int, int> OnAffinityPointsChanged; // 이전 값, 새 값
    public event Action<int, AffinityLevelData> OnAffinityLevelUp; // 새 레벨, 해당 레벨 데이터

    public void Initialize(NPCController controller)
    {
        _npcController = controller;
        if (_npcController == null)
        {
            Debug.LogError("NPCAffinity requires a valid NPCController to initialize.", this);
            enabled = false;
            return;
        }

        // TODO: 저장된 호감도 데이터 로드
        // _currentAffinityPoints = LoadAffinityPoints(_npcController.npcData.npcID);
        _currentAffinityPoints = 0; // 임시 초기값

        if (affinityLevelDefinitions == null || affinityLevelDefinitions.Count == 0)
        {
            Debug.LogError($"[NPCAffinity] NPC [{_npcController.npcData.npcName}]의 AffinityLevelDefinitions 리스트가 비어있거나 할당되지 않았습니다!", this);
        }
        else
        {
            Debug.Log($"[NPCAffinity] NPC [{_npcController.npcData.npcName}]의 AffinityLevelDefinitions 개수: {affinityLevelDefinitions.Count}");
            foreach (var levelData in affinityLevelDefinitions)
            {
                if (levelData != null)
                    Debug.Log($"  - Level: {levelData.level}, RequiredAffinity: {levelData.requiredAffinity}, CanJoinCombat: {levelData.canJoinCombat}, UnlockQuestID: {levelData.unlockQuestID}");
                else
                    Debug.LogWarning("  - 리스트에 null인 AffinityLevelData가 있습니다.");
            }
        }
        CalculateAndNotifyAffinityLevel(true); // 초기 레벨 계산 및 알림
    }
    public AffinityLevelData CurrentAffinityLevelData
    {
        get
        {
            if (affinityLevelDefinitions == null || affinityLevelDefinitions.Count == 0) return null;
            // 저장된 _currentAffinityLevel을 사용하여 해당 레벨 데이터 찾기
            return affinityLevelDefinitions.FirstOrDefault(l => l != null && l.level == _currentAffinityLevel);
        }
    }

    public void AddAffinity(int amount)
    {
        if (_npcController == null || amount == 0) return;

        int oldAffinity = _currentAffinityPoints;
        _currentAffinityPoints = Mathf.Max(0, _currentAffinityPoints + amount); // 호감도는 0 이하로 내려가지 않음 (기획에 따라 다름)
        // TODO: 최대 호감도 제한이 있다면 Clamp 사용

        Debug.Log($"NPC [{_npcController.npcData.npcName}] 호감도 변경: {oldAffinity} -> {_currentAffinityPoints} (증가량: {amount})");
        OnAffinityPointsChanged?.Invoke(oldAffinity, _currentAffinityPoints);
        CalculateAndNotifyAffinityLevel();
    }

    private void CalculateAndNotifyAffinityLevel(bool forceNotify = false)
    {
        if (_npcController == null)
        {
            Debug.LogError("NPCAffinity: _npcController is null. Cannot calculate affinity level.", this);
            return;
        }

        if (affinityLevelDefinitions == null || affinityLevelDefinitions.Count == 0)
        {
            // 레벨 정의가 없으면 항상 기본 레벨 (예: 0 또는 1)로 처리하거나,
            // 전투 참여 불가능 상태로 둘 수 있음
            if (_currentAffinityLevel != 0 || forceNotify) // 기본 레벨이 0이라고 가정
            {
                _currentAffinityLevel = 0;
                _npcController.UpdateCombatPartyStatus(false); // 레벨 정의 없으면 전투 참여 불가
                // OnAffinityLevelUp?.Invoke(0, null); // 레벨 데이터 없으므로 null 전달
            }
            return;
        }

        int newDeterminedLevel = 0;
        AffinityLevelData newLevelData = null;

        // 가장 높은 달성 가능한 레벨 찾기
        for (int i = affinityLevelDefinitions.Count - 1; i >= 0; i--)
        {
            if (affinityLevelDefinitions[i] != null && _currentAffinityPoints >= affinityLevelDefinitions[i].requiredAffinity)
            {
                newDeterminedLevel = affinityLevelDefinitions[i].level;
                newLevelData = affinityLevelDefinitions[i];
                break;
            }
        }
        // 만약 아무 레벨도 달성 못했다면 (예: 호감도 0이고, 레벨1의 requiredAffinity가 0보다 크면)
        // 기본 레벨(0 또는 1) 또는 가장 낮은 레벨로 설정할 수 있음.
        // 여기서는 달성한 가장 높은 레벨, 없으면 0으로 가정.

        if (newDeterminedLevel != _currentAffinityLevel || forceNotify)
        {
            Debug.Log($"NPC [{_npcController.npcData.npcName}] 호감도 레벨 변경: {_currentAffinityLevel} -> {newDeterminedLevel}");
            _currentAffinityLevel = newDeterminedLevel; //  먼저 새 레벨로 업데이트 

            //  업데이트된 새 레벨에 해당하는 데이터를 가져옴 
            AffinityLevelData actualNewLevelData = CurrentAffinityLevelData; 
            //  가져온 새 레벨 데이터로 이벤트를 발생시킴 
            OnAffinityLevelUp?.Invoke(_currentAffinityLevel, actualNewLevelData);

            if (actualNewLevelData != null)
            {
                bool canActuallyJoin = actualNewLevelData.canJoinCombat; // SO에 정의된 기본 참여 가능 여부

                // 디버깅 로그: SO의 canJoinCombat 값 확인
                Debug.Log($"[NPCAffinity] NPC: {_npcController.npcData.npcName}, 결정된 레벨 데이터: {actualNewLevelData.name}, SO의 canJoinCombat: {actualNewLevelData.canJoinCombat}, 해금 퀘스트 ID: {actualNewLevelData.unlockQuestID}");

                // 해금 퀘스트 ID가 있고 (0이 아니고), 해당 퀘스트를 플레이어가 완료하지 않았다면, 전투 참여 불가로 변경
                if (actualNewLevelData.unlockQuestID != 0)
                {
                    bool questCompleted = false;
                    if ( QuestManager.Instance.PlayerQuestDataManager != null && QuestManager.Instance.PlayerQuestDataManager.QuestData != null)
                    {
                        questCompleted = QuestManager.Instance.PlayerQuestDataManager.QuestData.completedQuests.Contains(actualNewLevelData.unlockQuestID);
                    }
                    else
                    {
                        Debug.LogWarning("[NPCAffinity] PlayerQuestDataManager 또는 QuestData를 찾을 수 없어 퀘스트 완료 여부를 확인할 수 없습니다.");
                    }

                    Debug.Log($"[NPCAffinity] 해금 퀘스트 ID [{actualNewLevelData.unlockQuestID}] 완료 여부: {questCompleted}");
                    if (!questCompleted)
                    {
                        canActuallyJoin = false; // 퀘스트 미완료 시 참여 불가
                    }
                }

                // 최종 결정된 canActuallyJoin 값 로그 출력
                Debug.Log($"[NPCAffinity] NPC: {_npcController.npcData.npcName}, 최종 전투 참여 가능 여부 (canActuallyJoin): {canActuallyJoin}");

                _npcController.UpdateCombatPartyStatus(canActuallyJoin); // 최종 결정된 값으로 상태 업데이트

                // TODO: 스탯 보너스 적용, 스킬 해금 등의 로직 호출 (determinedLevelData 사용)
                // ApplyStatBonuses(determinedLevelData.npcStatBonuses);
                // _npcController.skillUserComponent.UnlockSkills(determinedLevelData.unlockSkillIDs);
            }
            else // 레벨 데이터가 없는 경우 (예: 호감도가 어떤 레벨의 requiredAffinity에도 미치지 못함, newDeterminedLevel이 0으로 유지된 경우)
            {
                Debug.Log($"[NPCAffinity] NPC: {_npcController.npcData.npcName}, 해당하는 호감도 레벨 데이터를 찾지 못했습니다 (아마도 레벨 0). 전투 참여 불가.");
                _npcController.UpdateCombatPartyStatus(false);
            }
            // ▲▲▲ 전투 참여 가능 여부 판단 및 디버깅 로그 추가 ▲▲▲
        }
    }
    

    // 외부에서 호감도 레벨 체크를 강제로 트리거할 때 사용 ( NPC 부활 시)
    public void TriggerAffinityLevelCheck()
    {
        CalculateAndNotifyAffinityLevel(true);
    }
}
