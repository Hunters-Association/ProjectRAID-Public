using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AbilityAction_Data", menuName = "Monster/Attack Data/Ability Action")]
public class AbilityAttackData : AttackData
{
    [Header("능력 특정 속성")]
    [Tooltip("발동시킬 특정 능력 로직 식별자 (예: 'Burrow', 'Charge', 'Teleport'). 몬스터 또는 상태 로직이 이 식별자를 이해해야 합니다.")]
    public string abilityIdentifier = "Burrow"; // 상태 클래스에서 이 문자열을 보고 해당 능력 로직 실행

    // 능력별 추가 파라미터가 필요하면 여기에 정의
    // 예: public float chargeDistance = 10f;
    // 예: public float teleportCooldown = 5f; // AbilityAttackData의 cooldown과 별개로 관리 가능
}
