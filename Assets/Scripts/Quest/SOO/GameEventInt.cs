using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameEvent_Int", menuName = "Events/Game Event (Int)")]
public class GameEventInt : ScriptableObject
{
    // 이 이벤트를 구독하는 리스너(메서드) 목록
    private List<System.Action<int>> listeners =
        new List<System.Action<int>>();

    /// <summary>
    /// 이벤트를 발생시켜 모든 리스너에게 값을 전달합니다.
    /// </summary>
    /// <param name="value">전달할 문자열 값</param>
    public void Raise(int value)
    {
        // 리스트를 뒤에서부터 순회 (안전한 리스너 제거를 위해)
        for (int i = listeners.Count - 1; i >= 0; i--)
        {
            // 각 리스너 실행
            listeners[i]?.Invoke(value);
        }
    }

    /// <summary>
    /// 이 이벤트에 리스너(메서드)를 등록합니다.
    /// </summary>
    /// <param name="listener">등록할 메서드</param>
    public void RegisterListener(System.Action<int> listener)
    {
        if (!listeners.Contains(listener))
            listeners.Add(listener);
    }

    /// <summary>
    /// 이 이벤트에서 리스너(메서드)를 제거합니다.
    /// </summary>
    /// <param name="listener">제거할 메서드</param>
    public void UnregisterListener(System.Action<int> listener)
    {
        if (listeners.Contains(listener))
            listeners.Remove(listener);
    }
}
