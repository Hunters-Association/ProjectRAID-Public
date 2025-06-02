using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using ProjectRaid.Core;
using ProjectRaid.Data;

namespace ProjectRaid.Runtime
{
    /// <summary>
    /// Addressables 라벨 "item" 으로 빌드된 모든 ItemData SO를
    /// 런타임에 비동기 로드하여 ID → SO 딕셔너리로 캐싱
    /// </summary>
    public class ItemDatabase : MonoBehaviour
    {
        private readonly Dictionary<int, ItemData> dict = new();

        public bool IsInitialized { get; private set; }

        public IEnumerator InitAsync()
        {
            Debug.Log($"<color=#c0c0c0><b>[ItemDatabase]</b> 초기화 시작</color>");

            var handle = Addressables.LoadAssetsAsync<ItemData>("item", item =>
            {
                if (item == null)
                {
                    Debug.LogError("[ItemDatabase] null ItemData 로드됨");
                    return;
                }

                if (dict.ContainsKey(item.ItemID))
                {
                    Debug.LogWarning($"[ItemDatabase] 중복된 ItemID: {item.ItemID}");
                    return;
                }

                dict[item.ItemID] = item;
            }, true);

            yield return handle;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError("[ItemDatabase] Addressables 로딩 실패!");
                yield break;
            }

            Debug.Log($"<color=#c0c0c0><b>[ItemDatabase]</b> 초기화 성공! ({dict.Count}개 아이템 로드 완료.)</color>");
            IsInitialized = true;
        }

        public ItemData GetItem(int id) => dict.TryGetValue(id, out var item) ? item : null;

        public bool TryGetItem(int id, out ItemData data)
        {
            return dict.TryGetValue(id, out data);
        }

        public List<ItemData> GetItemListByType(ItemType type)
        {
            if (!IsInitialized || dict == null || dict.Values.Count == 0)
                return new List<ItemData>();

            return dict.Values
                .Where(x => x.ItemType == type)
                .OrderBy(x => x.ItemID)
                .ToList();
        }

        public List<ItemData> GetItemListByWeaponClass(WeaponClass weaponClass)
        {
            if (!IsInitialized || dict == null || dict.Values.Count == 0)
                return new List<ItemData>();

            return dict.Values
                .Where(x => x.Equipment is WeaponData w && w.Class == weaponClass)
                .OrderBy(x => x.ItemID)
                .ToList();
        }

        public List<ItemData> GetItemListByArmorClass(ArmorClass armorClass)
        {
            if (!IsInitialized || dict == null || dict.Values.Count == 0)
                return new List<ItemData>();

            return dict.Values
                .Where(x => x.Equipment is ArmorData a && a.Class == armorClass)
                .OrderBy(x => x.ItemID)
                .ToList();

        }
    }
}
