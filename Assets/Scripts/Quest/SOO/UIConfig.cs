using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectRaid.EditorTools;

[CreateAssetMenu(fileName = "UIConfig", menuName = "UI/UI Configuration")]
public class UIConfig : ScriptableObject
{
    [Serializable]
    public class Entry
    {
        public BaseUI prefab; // 반드시 BaseUI 파생 프리팹
        public bool isPersistent;
    }

    [SerializeField] private List<Entry> entries = new();
    private Dictionary<Type, (BaseUI prefab, bool persistent)> dict;

    public void Initialize(bool debugMode)
    {
        Debug.Log("<color=#c0c0c0><b>[UIConfig]</b> 초기화 시작</color>");

        dict = new Dictionary<Type, (BaseUI, bool)>();
        if (dict == null) { Debug.LogError("[UIConfig] Failed to create Dictionary!"); return; }

        if (entries != null)
        {
            foreach (var e in entries)
            {
                if (!e.prefab) { Debug.LogError("[UIConfig] Prefab missing!"); continue; }

                Type t = e.prefab.GetType();
                if (dict.ContainsKey(t))
                    Debug.LogWarning($"[UIConfig] Duplicate entry for {t} - later one ignored.");
                else
                    dict[t] = (e.prefab, e.isPersistent);
            }
        }

        Debug.Log($"<color=#c0c0c0><b>[UIConfig]</b> 로딩 완료  (Dictionary Count: {dict?.Count ?? -1})</color>");
    }

    public (BaseUI prefab, bool persistent) Get<T>() where T : BaseUI
        => dict.TryGetValue(typeof(T), out var v)
            ? v
            : throw new KeyNotFoundException($"[UIConfig] {typeof(T)} not registered.");


    public (BaseUI prefab, bool persistent) Get(Type t)
        => dict.TryGetValue(t, out var v)
            ? v
            : throw new KeyNotFoundException($"[UIConfig] {t} not registered.");

    public (BaseUI prefab, bool persistent) Get(BaseUI ui)
        => Get(ui.GetType());

    public BaseUI GetPrefab(Type t)
        => Get(t).prefab;

    public BaseUI GetPrefab(BaseUI ui)
        => Get(ui).prefab;


    public bool IsPersistent(Type t)
        => dict.TryGetValue(t, out var v) && v.persistent;

    public bool IsPersistent(BaseUI ui)
        => IsPersistent(ui.GetType());
}
