using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 파괴될 부위 하위 파츠
public class BossDestructionLowParts : BossLowParts
{
    private void Awake()
    {
        if(highParts == null)
        {
            Debug.Log("상위 파츠가 연결이 안되어있습니다.");
        }
        else
        {
            highParts.partsList.Add(this);
        }
    }
}
