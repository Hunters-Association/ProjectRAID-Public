using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MonsterState
{
    Idle,    // 대기 상태
    Attack,  // 공격 상태
    Flee,    // 도망 상태
    FleeToSpawn,// 스폰 지점으로 도망
    Dead,     // 죽음 상태
    Burrowing,   // 땅 파고 들어가는 중
    Burrowed,    // 땅 속에 숨어있는 중
    Emerging,     // 땅에서 나오는 중
    ReturnToSpawn
}
