using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 모든 QuestData SO를 관리하고 ID로 빠르게 검색할 수 있도록 돕는 데이터베이스.
/// </summary>
[CreateAssetMenu(fileName = "QuestDatabase", menuName = "Quest System/Database")]
public class QuestDatabase : ScriptableObject
{
    [Tooltip("프로젝트 내 모든 QuestData SO를 여기에 할당하세요.")]
    public List<QuestData> allQuests;
    [System.NonSerialized]
    private Dictionary<int, QuestData> questDictionary;
    private bool isInitialized = false;

    private void OnEnable()
    {        
        isInitialized = false;        
        questDictionary = null;        
    }

    /// <summary>
    /// 퀘스트 딕셔너리를 초기화합니다. 게임 시작 시 한 번 호출해야 합니다.
    /// </summary>
    public void InitializeDatabase()
    {
        if (isInitialized) return;
        Debug.Log("[QuestDatabase] 초기화 시작");
        questDictionary = new Dictionary<int, QuestData>();
        if (allQuests != null) 
        {
            foreach (QuestData quest in allQuests)
            {
                if (quest != null && quest.questID != 0)
                {
                    if (!questDictionary.ContainsKey(quest.questID))
                    {
                        questDictionary.Add(quest.questID, quest);
                    }
                    else
                    {
                        Debug.LogWarning($"Duplicate QuestID '{quest.questID}' found in QuestDatabase. Quest '{quest.questName}' ignored.");
                    }
                }
                else if (quest != null)
                {
                    Debug.LogWarning($"Quest '{quest.questName}' has empty QuestID. Ignored.");
                }
            }
            Debug.Log($"Quest Database Initialized. {questDictionary.Count} unique quests loaded.");
            Debug.Log("[QuestDatabase] 초기화 성공!");
        }
        else
        {
            Debug.LogWarning("QuestDatabase 'allQuests' list is null or empty.");
        }
        isInitialized = true;
    }

    /// <summary>
    /// ID로 QuestData를 찾아 반환합니다.
    /// </summary>
    public QuestData GetQuestByID(int questID)
    {
        if (!isInitialized || questDictionary == null)
        {
            Debug.LogError("QuestDatabase not initialized!");
            return null;
        }
        questDictionary.TryGetValue(questID, out QuestData quest);
        return quest;
    }

    /// <summary>
    /// 모든 QuestData 리스트를 반환합니다. (참고용)
    /// </summary>
    public List<QuestData> GetAllQuests() => new List<QuestData>(allQuests);
}
