using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractableTest
{
    /// <summary>
    /// 플레이어가 상호작용을 시도할 때 호출될 메서드
    /// </summary>
    /// <param name="interactor">상호작용을 시도한 게임 오브젝트 (주로 플레이어)</param>
    void Interact(GameObject interactor);

    /// <summary>
    /// 플레이어에게 보여줄 상호작용 안내 문구 (예: "[E] 줄기 타기")
    /// </summary>
    string InteractionPrompt { get; }
}
