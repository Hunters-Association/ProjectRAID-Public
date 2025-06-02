using System;
using UnityEngine;

public static class CursorManager
{
    public enum State { Show, Hide }
    public static State CurrentState { get; private set; }

    public static event Action<State> OnCursorStateChange;

    /// <summary>
    /// 마우스 커서의 가시성과 잠금 상태를 설정합니다.
    /// </summary>
    /// <param name="show">true이면 커서를 보이고 잠금을 해제, false이면 숨기고 잠금.</param>
    public static void SetCursorState(bool show)
    {
        Cursor.visible = show;
        Cursor.lockState = show ? CursorLockMode.None : CursorLockMode.Locked;

        CurrentState = show ? State.Show : State.Hide;
        OnCursorStateChange?.Invoke(CurrentState);

        // Debug.Log($"[CursorManager] Cursor State Set: Visible = {show}, LockMode = {Cursor.lockState}"); // 확인용 로그
    }

    public static void SetCursorState(CursorLockMode cursorLockMode)
    {
        SetCursorState(cursorLockMode is CursorLockMode.None);
    }

    public static void SetCursorState(State state)
    {
        SetCursorState(state is State.Show);
    }
}
