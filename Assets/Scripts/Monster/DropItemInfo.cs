using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct DropItemInfo
{
    public int itemID;     // 아이템 ID (101001, 101002)
    [Range(0f, 100f)]
    public float dropChance;  // 드랍 확률 (%)
    public int minQuantity;   // 최소 수량
    public int maxQuantity;   // 최대 수량
}
