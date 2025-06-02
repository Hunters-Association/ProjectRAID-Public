using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="BossData", menuName = "Data/Boss/BossData")]
public class BossSO : ScriptableObject
{
    // 적대적인 상태는 먼저 해두신 enum이 있을 듯?
    public int bossID;
    public string bossName; // 이름
    public int bossType;
    public string bossDes;
    public int bossArea;
    public float bossHP;     // 최대 체력
    public float bossAD;       // 공격력
    public float bossSpeed;     // 속도
    public float bossSTime;     // 재생성 대기시간
    public List<int> bossDrops; // 드롭할 아이템 리스트

    [Tooltip("몬스터 처치 시 발행되는 이벤트 (GameEventString 타입 SO 연결)")]
    public GameEventInt monsterKilledEvent; // Inspector에서 연결!
}
