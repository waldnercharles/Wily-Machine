using Godot;

namespace Spaghetti.Godot;

public partial class PlayerStateMachine : StateMachine
{
    public bool Locked { get; set; }

    public new PlayerState? State => base.State as PlayerState;

    public override void ChangeState<T>()
    {
        Log.Trace($"{State?.GetType().Name ?? "NULL"} -> {typeof(T).Name}");

        if (Locked || !Enabled)
        {
            return;
        }

        base.ChangeState<T>();
    }
}
