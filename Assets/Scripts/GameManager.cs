using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using ProjectRaid.Runtime;
using ProjectRaid.EditorTools;
using DG.Tweening;
using System.Collections.Generic;

public class GameManager : MonoSingleton<GameManager>, IInitializable
{
    [FoldoutGroup("시스템 컴포넌트", ExtendedColor.Cyan)]
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private InventorySystem inventorySystem;
    [SerializeField] private CraftingSystem craftingSystem;

    [FoldoutGroup("데이터베이스 설정", ExtendedColor.Orange)]
    [Header("Skill Database (Direct List)")] // 헤더 추가
    [Tooltip("SkillData ScriptableObject 에셋들을 여기에 직접 할당하세요.")]
    [SerializeField] private List<SkillData> allSkillDataAssets; // <<< 스킬 SO 리스트 직접 할당 필드
    [System.NonSerialized] private Dictionary<string, SkillData> skillDictionary = new Dictionary<string, SkillData>();
    [System.NonSerialized] private bool isSkillDatabaseInitialized = false;

    [FoldoutGroup("유틸 컴포넌트", ExtendedColor.Silver)]
    [SerializeField] private HitStopManager hitStopManager;
    [SerializeField] private CameraShakeManager cameraShakeManager;
    [SerializeField] private DamagePopupManager damagePopupManager;
    [SerializeField] private BlipPoolManager blipPoolManager;

    [FoldoutGroup("씬 전환 연출", ExtendedColor.White)]
    [SerializeField] public CanvasGroup fader;
    [SerializeField] private LoadingUI loadingUI;
    [SerializeField] private float fadeDuration = 0.4f;

    public MinimapSystem MinimapSystem { get; set; }

    public event Action OnDataReady;
    public bool IsDataInitialized { get; private set; } = false;

    // System
    public ItemDatabase Database => itemDatabase;
    public InventorySystem Inventory => inventorySystem;
    public ICraftingSystem Crafting => craftingSystem;
    public bool IsSkillDatabaseReady => isSkillDatabaseInitialized;

    // Utility
    public HitStopManager HitStop => hitStopManager;
    public CameraShakeManager CameraShake => cameraShakeManager;
    public DamagePopupManager DamagePopup => damagePopupManager;
    public BlipPoolManager BlipPool => blipPoolManager;

    // SceneLoader
    private static bool isLoading = false;

    private bool refMissing = false;

    public IEnumerator Initialize()
    {
        if (fader != null) fader.alpha = 0f;

        CheckReference(itemDatabase);
        CheckReference(inventorySystem);
        CheckReference(craftingSystem);

        Debug.Log("<color=#00ffff><b>[GameManager]</b> 초기화 시작</color>");

        if (refMissing)
        {
            enabled = false;
            yield break;
        }
        InitializeSkillDatabase();

        StartCoroutine(itemDatabase.InitAsync());
        yield return new WaitUntil(() => itemDatabase.IsInitialized);

        craftingSystem.InitializeSystem(inventorySystem, itemDatabase);
        IsDataInitialized = true;
        OnDataReady?.Invoke();
        
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;

        Debug.Log("<color=#00ffff><b>[GameManager]</b> 초기화 성공!</color>");
    }
    private void InitializeSkillDatabase()
    {
        if (isSkillDatabaseInitialized) return; // 중복 초기화 방지

        Debug.Log("<color=#FFA500>[GameManager]</color> 스킬 데이터베이스 초기화 시작 (Direct List)...");
        skillDictionary = new Dictionary<string, SkillData>();

        if (allSkillDataAssets == null || allSkillDataAssets.Count == 0)
        {
            Debug.LogWarning("[GameManager/SkillDB] Inspector에 할당된 SkillData 에셋 리스트가 비어있습니다.");
            isSkillDatabaseInitialized = true; // 비어있어도 초기화는 완료된 것으로 간주
            return;
        }

        foreach (var skill in allSkillDataAssets)
        {
            if (skill != null && !string.IsNullOrEmpty(skill.skillID))
            {
                if (!skillDictionary.ContainsKey(skill.skillID))
                {
                    skillDictionary.Add(skill.skillID, skill);
                }
                else Debug.LogWarning($"[GameManager/SkillDB] 중복된 스킬 ID({skill.skillID}) 발견: '{skill.skillName}'");
            }
            else if (skill != null) Debug.LogWarning($"[GameManager/SkillDB] skillID가 비어있는 SkillData 에셋 발견: {skill.name}");
            else Debug.LogWarning("[GameManager/SkillDB] 리스트에 null인 SkillData 에셋이 있습니다.");
        }
        isSkillDatabaseInitialized = true;
        Debug.Log($"<color=#90EE90>[GameManager]</color> 스킬 데이터베이스 초기화 완료. {skillDictionary.Count}개 스킬 로드됨.");
    }
    public SkillData GetSkillByID(string skillID)
    {
        if (!isSkillDatabaseInitialized)
        {
            Debug.LogWarning($"[GameManager] 스킬 데이터베이스가 아직 초기화되지 않았습니다. Skill ID [{skillID}] 조회 실패.");
            return null;
        }
        if (skillDictionary.TryGetValue(skillID, out SkillData skill))
        {
            return skill;
        }
        else
        {
            // Debug.LogWarning($"[GameManager] 스킬 ID [{skillID}]를 찾을 수 없습니다."); // 필요 시 주석 해제
            return null;
        }
    }


    private bool CheckReference(object reference)
    {
        if (reference == null)
        {
            Debug.LogError($"[GameManager] {reference} 참조 누락!", this);
            refMissing = true;
            return false;
        }

        return true;
    }

    public void ReloadScene()
    {
        LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadScene(int index)
    {
        CursorManager.SetCursorState(false);
        AnalyticsManager.CallLoadSceneEvent(SceneManager.GetActiveScene().buildIndex, index, QuestManager.Instance.PlayerQuestDataManager.TrackedQuestID);


        if (!isLoading)
        {
            if (loadingUI != null) loadingUI.Init();
            StartCoroutine(LoadSceneAsync(index));
        }
    }

    public IEnumerator LoadSceneAsync(int index)
    {
        isLoading = true;

        try
        {
            // 1) 페이드 아웃
            if (fader != null)
                yield return fader
                    .DOFade(1f, fadeDuration)
                    .SetEase(Ease.OutQuad)
                    .WaitForCompletion();

            // 2) 비동기 로드 시작 (활성화 잠금)
            AsyncOperation operation = SceneManager.LoadSceneAsync(index);
            operation.allowSceneActivation = false;

            // 3) 로드 진행이 90%에 도달할 때까지 대기
            //while (operation.progress < 0.9f)
            //{
            //    // 로딩 바 채우기
            //    if (loadingUI != null) loadingUI.SetProgress(operation.progress);
            //    yield return null;
            //}


            //if (loadingUI != null) loadingUI.SetProgress(operation.progress);

            //DOVirtual.DelayedCall(0.25f, () => { loadingUI.SetProgress(1f); });

            float loadingInterval = 0.25f;
            float displayProgress = 0;
            float displayDuration = 0.25f;
            for (int i = 0; i < 3; i++)
            {
                displayProgress += loadingInterval;
                displayProgress = Mathf.Clamp(displayProgress, 0f, 0.75f);
                if (loadingUI != null) loadingUI.SetProgress(displayProgress);

                yield return new WaitForSeconds(displayDuration);
            }

            yield return new WaitUntil(() => operation.progress >= 0.9f);

            if (loadingUI != null) loadingUI.SetProgress(1f);

            // 4) 씬 활성화 허용
            operation.allowSceneActivation = true;

            // 5) 실제로 씬로딩이 완료(씬 전환)될 때까지 한 프레임 대기
            yield return new WaitUntil(() => operation.isDone);
            // yield return null;

            // 6) 페이드 인 (새 씬을 보여주며 밝히기)
            if (fader != null)
                yield return fader
                    .DOFade(0f, fadeDuration)
                    .SetEase(Ease.OutQuad)
                    .WaitForCompletion();
        }
        finally
        {
            isLoading = false;
        }
    }

    private void OnDisable()
    {
        isLoading = false;
    }

    private void OnDestroy()
    {
        isLoading = false;
    }
}
