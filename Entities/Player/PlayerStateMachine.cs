using Godot;

namespace Spaghetti.Godot;

public partial class PlayerStateMachine : StateMachine
{
    public bool Locked { get; set; }

    public new PlayerState? State => base.State as PlayerState;

    public override void ChangeState<T>()
    {
        if (Locked || !Enabled)
        {
            return;
        }

        base.ChangeState<T>();
    }
}
