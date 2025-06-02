using System.Collections.Generic;
using UnityEngine;
using ProjectRaid.EditorTools;

public class BlipPoolManager : MonoBehaviour
{
    [FoldoutGroup("Pool", ExtendedColor.Cyan)]
    [SerializeField] private MinimapBlip blipPrefab;
    [SerializeField] private Transform parent;
    [SerializeField] private int poolSize = 50;

    private readonly Queue<MinimapBlip> pool = new();

    private void Awake()
    {
        InitPool();
    }

    private void InitPool()
    {
        foreach (var b in pool)
            if (b != null) Destroy(b.gameObject);
        pool.Clear();

        for (int i = 0; i < poolSize; i++)
        {
            var blip = Instantiate(blipPrefab, parent);
            blip.gameObject.SetActive(false);
            pool.Enqueue(blip);
        }
    }

    public MinimapBlip Get()
    {
        MinimapBlip blip = null;

        // pool에 남아 있는 정상 객체 꺼내기
        while (pool.Count > 0)
        {
            var candidate = pool.Dequeue();
            if (candidate != null) { blip = candidate; break; }
        }

        // 모자라면 새로 만들기
        if (blip == null)
            blip = Instantiate(blipPrefab, parent);

        // 활성화 전용 부모(예: 씬의 blipRoot)로 붙여 줄 건 호출 측에서 처리
        blip.gameObject.SetActive(true);
        return blip;
    }

    public void Return(MinimapBlip blip)
    {
        // 1) 파괴되었거나 null이면 아무것도 안 함
        if (blip == null) return;

        // 2) 비활성화 & 영구 루트로 재부모화
        blip.gameObject.SetActive(false);
        blip.transform.SetParent(parent, worldPositionStays: false);

        // 3) 큐에 다시 넣기
        pool.Enqueue(blip);
    }
}
