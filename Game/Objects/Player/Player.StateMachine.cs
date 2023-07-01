namespace Spaghetti;

public sealed partial class Player
{
    private readonly StateMachine m_MovementStateMachine = new StateMachine();
    private readonly StateMachine m_ShootingStateMachine = new StateMachine();

    public void SetNextMovementState<TState>() where TState : MovementState =>
        m_MovementStateMachine.SetNextState<TState>();

    public void SetNextMovementState(MovementState state) =>
        m_MovementStateMachine.SetNextState(state);

    public void SetMovementState<TState>() where TState : MovementState =>
        m_MovementStateMachine.SetState<TState>();

    public void SetMovementState(MovementState state) =>
        m_MovementStateMachine.SetState(state);

    public void SetNextShootingState<TState>() where TState : ShootingState =>
        m_ShootingStateMachine.SetNextState<TState>();

    public void SetNextShootingState(ShootingState state) =>
        m_ShootingStateMachine.SetNextState(state);

    public void SetShootingState<TState>() where TState : ShootingState =>
        m_ShootingStateMachine.SetState<TState>();

    public void SetShootingState(ShootingState state) =>
        m_ShootingStateMachine.SetState(state);
}
