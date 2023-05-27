using Godot;

namespace Spaghetti.Godot;

public abstract partial class State : Node
{
    public StateMachine StateMachine { get; set; } = null!;

    public virtual void Enter<TState>(TState? previousState) where TState : State { }

    public virtual void Exit() { }

    public virtual void HandleInput(InputEvent input) { }

    public virtual void Update(float delta) { }

    public virtual void AnimationFinished(string animationName) { }
}
