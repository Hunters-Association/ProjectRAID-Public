using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateContext
{
    /// <summary>
    /// 다음 상태와 관련된 목표 위치입니다.
    /// 예: 몬스터가 땅에서 나타나야 할 위치.
    /// </summary>
    public Vector3 TargetPosition { get; set; }

    /// <summary>
    /// (선택 사항) 다음 상태와 관련된 목표 게임 오브젝트 참조입니다.
    /// 예: 상태 변경을 유발한 플레이어 게임 오브젝트.
    /// </summary>
    public GameObject TargetGameObject { get; set; }

    /// <summary>
    /// (선택 사항) 이 상태 전환을 유발했거나 다음 상태에서 고려해야 할
    /// 특정 공격 데이터(AttackData)입니다.
    /// </summary>
    public AttackData SelectedAttackData { get; set; } // AttackData 클래스가 정의되어 있어야 합니다.

    // 필요에 따라 다른 관련 컨텍스트 데이터 추가:
    // public float DurationOverride { get; set; } // 다음 상태의 기본 지속 시간 재정의?
    // public bool ForceAggression { get; set; } // 다음 상태가 즉시 공격적이 되어야 하는가?

    /// <summary>
    /// 기본 생성자입니다. 속성을 기본값(null 또는 Vector3.zero)으로 초기화합니다.
    /// </summary>
    public StateContext()
    {
        // 기본값 설정 (필수는 아니지만 명확성을 위해)
        TargetPosition = Vector3.zero;
        TargetGameObject = null;
        SelectedAttackData = null;
    }

    /// <summary>
    /// TargetPosition을 빠르게 설정하기 위한 편의 생성자입니다.
    /// </summary>
    /// <param name="targetPosition">초기 목표 위치입니다.</param>
    public StateContext(Vector3 targetPosition) : this() // 기본 생성자를 먼저 호출합니다.
    {
        this.TargetPosition = targetPosition;
    }

    // 필요하다면 더 많은 편의 생성자 추가 가능:
    // public StateContext(GameObject targetObject) : this()
    // {
    //     this.TargetGameObject = targetObject;
    //     if(targetObject != null) this.TargetPosition = targetObject.transform.position;
    // }
}
