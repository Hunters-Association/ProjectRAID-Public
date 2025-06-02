using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 페이즈 정보
[Serializable]
public class BossPhaseData
{
    public int phaseIndex;
    public string phaseType;              // 페이즈 타입
    public float changeHPPercent;         // 페이즈가 전환될 hp 조건

    // 페이즈에 필요한 정보들 세팅
    public Action setData;

    public void Init() { setData?.Invoke(); }

    public List<ActionPattern> attackPatternList;
}
