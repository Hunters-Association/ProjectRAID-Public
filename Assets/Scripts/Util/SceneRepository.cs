using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class SceneEntry
{
    public int sceneID;
    public string scenePath;
}

#if UNITY_EDITOR
[System.Serializable]
public class SceneData
{
    public int sceneID = -1;
    public SceneAsset sceneAsset;
}
#endif

[CreateAssetMenu(fileName = "SceneRepository", menuName = "SceneRepository")]
public class SceneRepository : ScriptableObject
{
#if UNITY_EDITOR
    public List<SceneData> sceneAssets;
    private void OnValidate()
    {
        sceneEntries.Clear();

        foreach (var data in sceneAssets)
        {
            if (data.sceneID != -1 && data.sceneAsset != null)
            {
                string path = AssetDatabase.GetAssetPath(data.sceneAsset);
                sceneEntries.Add(new SceneEntry { sceneID = data.sceneID, scenePath = path });
            }
        }
    }
#endif

    [SerializeField] private List<SceneEntry> sceneEntries = new();
    private Dictionary<int, string> sceneMap;

    /// <summary>
    /// 딕셔너리에 저장된 ID로 씬 경로 반환
    /// </summary>
    public string GetScenePath(int id)
    {
        if (sceneMap == null)
        {
            sceneMap = new();
            foreach (var entry in sceneEntries)
            {
                sceneMap[entry.sceneID] = entry.scenePath;
            }
        }

        return sceneMap.TryGetValue(id, out var path) ? path : null;
    }
}