namespace Spaghetti;

public sealed partial class Player
{
    private readonly StateMachine m_MovementStateMachine = new StateMachine();
    private readonly StateMachine m_ShootingStateMachine = new StateMachine();

    public void EnqueueMovementState<TState>() where TState : MovementState =>
        m_MovementStateMachine.SetNextState<TState>();

    public void EnqueueMovementState(MovementState state) =>
        m_MovementStateMachine.SetNextState(state);

    public void SetMovementState<TState>() where TState : MovementState =>
        m_MovementStateMachine.SetState<TState>();

    public void SetMovementState(MovementState state) =>
        m_MovementStateMachine.SetState(state);

    public void SetTemporaryMovementState<TState>() where TState : MovementState =>
        m_MovementStateMachine.SetTemporaryState<TState>();

    public void SetTemporaryMovementState(MovementState state) =>
        m_MovementStateMachine.SetTemporaryState(state);

    public void EnqueueShootingState<TState>() where TState : ShootingState =>
        m_ShootingStateMachine.SetNextState<TState>();

    public void EnqueueShootingState(ShootingState state) =>
        m_ShootingStateMachine.SetNextState(state);

    public void SetShootingState<TState>() where TState : ShootingState =>
        m_ShootingStateMachine.SetState<TState>();

    public void SetShootingState(ShootingState state) =>
        m_ShootingStateMachine.SetState(state);
}
