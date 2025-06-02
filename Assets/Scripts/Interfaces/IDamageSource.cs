using ProjectRaid.Data;

/// <summary>
/// 데미지를 주는 오브젝트가 구현할 인터페이스
/// </summary>
public interface IDamageSource
{
    WeaponData GetWeaponData(); // 또는 SkillData 등 확장 가능
}
