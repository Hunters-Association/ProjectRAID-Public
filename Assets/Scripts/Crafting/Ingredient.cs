using ProjectRaid.Core;
using ProjectRaid.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Ingredient
{
    [Tooltip("필요한 재료 아이템 데이터 (ItemData 에셋 - ItemType이 Material 또는 Misc 권장)")]
    public ItemData materialItem; // ItemData 참조

    [Tooltip("필요한 재료의 수량")]
    [Min(1)] public int count = 1;

    //  에디터에서 유효성 검사
    public void OnValidate()
    {
        if (materialItem != null && materialItem.ItemType != ItemType.Material && materialItem.ItemType != ItemType.Misc)
        {
            Debug.LogWarning($"Ingredient에 재료/기타 타입 외 아이템({materialItem.DisplayNameKey}) 할당됨", materialItem);
        }
    }
}
