using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjectRaid.Data;

[CreateAssetMenu(fileName = "SkillDatabase", menuName = "Game Data/Database/Skill Database")]
public class SkillDatabase : ScriptableObject
{
    [SerializeField] private AssetLabelReference skillLabel = new AssetLabelReference { labelString = "skill" }; // Addressables 레이블
    [System.NonSerialized] private Dictionary<string, SkillData> skillDictionary = new Dictionary<string, SkillData>();
    [System.NonSerialized] private bool isInitialized = false;
    [System.NonSerialized] private AsyncOperationHandle<IList<SkillData>> loadHandle;

    public bool IsInitialized => isInitialized;

    public async Task InitializeDatabaseAsync()
    {
        if (isInitialized || loadHandle.IsValid()) return; // 중복 실행 방지
        Debug.Log("[SkillDatabase] 초기화 시작...");
        skillDictionary = new Dictionary<string, SkillData>();
        loadHandle = Addressables.LoadAssetsAsync<SkillData>(skillLabel, null);
        IList<SkillData> loadedSkills = await loadHandle.Task;

        if (loadHandle.Status == AsyncOperationStatus.Succeeded && loadedSkills != null)
        {
            foreach (var skill in loadedSkills)
            {
                if (skill != null && !string.IsNullOrEmpty(skill.skillID))
                {
                    if (!skillDictionary.ContainsKey(skill.skillID))
                    {
                        skillDictionary.Add(skill.skillID, skill);
                    }
                    else Debug.LogWarning($"[SkillDatabase] 중복된 스킬 ID({skill.skillID}) 발견: '{skill.skillName}'");
                }
            }
            isInitialized = true;
            Debug.Log($"[SkillDatabase] 초기화 완료. {skillDictionary.Count}개 스킬 로드됨.");
        }
        else
        {
            Debug.LogError($"[SkillDatabase] Addressables 로딩 실패: {loadHandle.OperationException}");
            isInitialized = false;
        }
    }

    public SkillData GetSkillByID(string id)
    {
        if (!isInitialized) { Debug.LogWarning("[SkillDatabase] 아직 초기화되지 않았습니다."); return null; }
        skillDictionary.TryGetValue(id, out SkillData skill);
        return skill;
    }

    // OnDisable 등에서 Addressables 핸들 해제 로직 추가
}
