using System;

namespace Spaghetti.Godot;

public abstract partial class PlayerState : State
{
    protected Player Player { get; set; } = null!;

    public override void _Ready()
    {
        Player = Owner as Player ?? throw new Exception("PlayerState must belong to a Player");
    }

    public virtual void HandleCommand(PlayerCommand command)
    {
    }
}
