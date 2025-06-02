using UnityEngine;

/// <summary>
/// 데미지를 받을 수 있는 대상이 구현할 인터페이스
/// </summary>
public interface IDamageable
{
    /// <summary>
    /// 데미지를 받고, 공격자의 위치 기준으로 넉백 등을 처리
    /// </summary>
    /// <param name="info">데미지 정보</param>
    void TakeDamage(DamageInfo info);
}
