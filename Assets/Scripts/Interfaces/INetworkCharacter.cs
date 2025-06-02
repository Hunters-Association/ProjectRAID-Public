public interface INetworkCharacter
{
    void Server_EquipWeapon(int weaponID);
    void Server_PerformAttack(int comboStep);
    void Client_PlayHitEffect();
}