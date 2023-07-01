namespace Spaghetti;

public enum StateChange
{
    None,
    Next
}

public abstract class State
{
    public virtual void Enter(State? previousState) { }

    public virtual bool CanExit() { return true; }
    public virtual void Exit() { }

    public virtual StateChange Update(float delta) { return StateChange.None; }
}
