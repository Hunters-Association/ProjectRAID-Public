using ProjectRaid.EditorTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "BossBodyPartData", menuName = "Data/Boss/BossBodyPartData")]
public class BossBodyPartData : ScriptableObject
{
    public float partDef;           // 파츠 별 방어력

    public bool canDstr;            // 파괴 가능 여부
    [ShowIf(nameof(canDstr), true)]
    public float DstValue;          // 파괴 수치

    public bool canCut;             // 절단 가능 여부
    [ShowIf(nameof(canCut), true)]
    public float cutValue;          // 절단 수치
}
