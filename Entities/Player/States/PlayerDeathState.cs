using Godot;

namespace Spaghetti.Godot;

public partial class PlayerDeathState : PlayerState
{
    [Export] public float FreezeTime { get; set; } = 0.33f;

    public override void Enter<TState>(TState? previousState) where TState : class
    {
        Player.StateMachine.Enabled = false;
        Player.IsDead = true;
        Player.BufferingCharge = false;

        // TODO: Dramatic death effects
    }
}
