using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DropItem
{
    // 드롭될 아이템 아이디
    public int id;
    // 드롭 확률
    public int probability;
}

[CreateAssetMenu(fileName = "BossDropTable", menuName = "Data/Boss/BossDropTable")]
public class BossDropTable : ScriptableObject
{
    // 갈무리시 드롭될 아이템들
    public DropItem[] captureTable;
    public DropItem[] cutTable;
    public DropItem[] catchTable;

    public DropItem[] GetTable(BossInteractableType type)
    {
        switch (type)
        {
            case BossInteractableType.Body:
                return captureTable;
            case BossInteractableType.Cut:
                return cutTable;
            default:
                return captureTable;
        }
    }


    public int GetDropItemID(DropItem[] dropTable)
    {
        DropItem dropItem = new() { id = -1 };
        int total = 0;

        for (int i = 0; i < dropTable.Length; i++)
        {
            total += dropTable[i].probability;
        }

        int randomValue = UnityEngine.Random.Range(0, total);
        int weight = 0;
        for (int i = 0; i < dropTable.Length; i++)
        {
            dropItem = dropTable[i];

            weight += dropItem.probability;
            if (randomValue < weight)
            {
                return dropItem.id;
            }
        }

        return -1;
    }
}
