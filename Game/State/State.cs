using Godot;

namespace Spaghetti;

public enum StateChange
{
    None,
    Next
}

public abstract class State
{
    public StateMachine StateMachine { get; set; } = null!;

    public virtual void Enter(State? previousState) { }

    public virtual void Exit() { }

    public virtual void HandleInput(InputEvent input) { }

    public virtual StateChange Update(float delta) { return StateChange.None; }
}
