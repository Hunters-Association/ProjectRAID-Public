// using System.Collections.Generic;
// using UnityEngine;
// using ProjectRaid.EditorTools;

// public class BlipManager : MonoBehaviour
// {
//     [FoldoutGroup("Blip", ExtendedColor.Crimson)]
//     [SerializeField] private BlipPool blipPool;
//     [SerializeField] private Transform minimapRotation;     // 미니맵 회전 기준 Transform
//     [SerializeField] private Transform minimapTransform;    // 미니맵 중심 Transform
//     [SerializeField] private float mapScale = 1f;
//     [SerializeField] private float maxVisibleDistance = 100f;

//     [FoldoutGroup("Settings", ExtendedColor.White)]
//     [SerializeField] private List<BlipSettings> blipSettingsList;

//     private MinimapSystem minimap;

//     public BlipSettings GetBlipSettings(BlipType type)
//     {
//         foreach (var setting in blipSettingsList)
//         {
//             if (setting.type == type) return setting;
//         }

//         Debug.LogWarning($"[BlipManager] {type} 타입에 맞는 BlipSettings를 찾을 수 없습니다.");
//         return null;
//     }

//     public void RegisterBlip(Transform target, BlipSettings settings)
//     {
//         if (activeBlips.ContainsKey(target)) return;

//         var blip = blipPool.Get();
//         blip.Initialize(target, settings);
//         activeBlips[target] = blip;
//     }

//     public void UnregisterBlip(Transform target)
//     {
//         if (activeBlips.TryGetValue(target, out var blip))
//         {
//             blipPool.Return(blip);
//             activeBlips.Remove(target);
//         }
//     }

//     private void Start()
//     {
//         minimap = MinimapSystem.Instance;
//     }

//     private void LateUpdate()
//     {
//         if (minimap == null) return;

//         Vector3 center = minimap.MinimapCenter;
//         Quaternion rotation = minimap.MinimapRotation;

//         foreach (var kvp in activeBlips)
//         {
//             var target = kvp.Key;
//             var blip = kvp.Value;

//             float distance = Vector3.Distance(target.position, center);

//             if (distance > maxVisibleDistance)
//             {
//                 blip.gameObject.SetActive(false);
//             }
//             else
//             {
//                 blip.gameObject.SetActive(true);
//                 blip.UpdateBlip(center, mapScale, rotation, minimapTransform);
//             }
//         }
//     }
// }
