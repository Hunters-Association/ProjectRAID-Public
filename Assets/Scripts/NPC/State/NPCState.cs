

public enum NPCState
{
    // 공통 및 비전투 상태
    None,             // 초기화 또는 오류 시 상태
    Idle,             // 평상시 대기 (주변 둘러보기 등)
    FollowPlayer,     // 플레이어 따라다니기 (비전투 시)
    MovingToPosition, // 특정 지점으로 이동 (퀘스트, 이벤트 등)
    Interacting,      // 플레이어와 상호작용 중 (대화, 퀘스트 UI 등)

    // 전투 관련 상태 (IsInCombatParty == true 일 때 주로 사용)
    CombatIdle,       // 전투 중 대기 (적 탐색, 스킬 사용 판단)
    ApproachingTarget,// 전투 대상 또는 스킬 사용 위치로 접근
    UsingSkill,       // 스킬 시전 중 (캐스팅 포함)
    Attacking,        // 기본 공격 중 (만약 NPC가 기본 공격을 한다면)
    Evading,          // 위험 회피 중 (선택적)

    // 특수 상태
    WaitForRide,      // 플레이어 탑승 대기 (기획서에 명시)
    Riding,           // 플레이어가 탑승 중 (선택적)
    Fainted,          // 전투 불능 (기절)
    ReturningToPost,  // 전투 종료 후 원래 위치로 복귀
}
