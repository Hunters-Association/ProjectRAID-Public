using System;

/// <summary>
/// 플레이어 FSM 상태 전이 처리 클래스
/// </summary>
public class PlayerStateMachine
{
    public PlayerState CurrentState { get; private set; }

    public event Action<PlayerState> OnChangePreviousState;
    public event Action<PlayerState> OnChangeCurrentState;

    public void Initialize(PlayerState startState)
    {
        OnChangeCurrentState?.Invoke(startState);

        CurrentState = startState;
        CurrentState.Enter();
    }

    public void ChangeState(PlayerState newState)
    {
        if (newState == null || CurrentState == null || newState == CurrentState) return;

        OnChangePreviousState?.Invoke(CurrentState);
        OnChangeCurrentState?.Invoke(newState);

        CurrentState.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }

    public void HandleInput() => CurrentState?.HandleInput();
    public void Update() => CurrentState?.Update();
    public void FixedUpdate() => CurrentState?.PhysicsUpdate();
    public void LateUpdate() => CurrentState?.LateUpdate();
}
