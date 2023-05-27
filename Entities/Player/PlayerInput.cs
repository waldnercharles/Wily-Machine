using System;
using Godot;

namespace Spaghetti.Godot;

public partial class PlayerInput : DeviceInput
{
    public PlayerInputController Controller { get; set; } = PlayerInputController.Player;

    public PlayerInput() : base(-1) { }

    public Vector2 GetInputDirection()
    {
        if (Controller is PlayerInputController.None)
        {
            return Vector2I.Zero;
        }

        if (Controller is PlayerInputController.AI)
        {
            throw new NotImplementedException();
        }

        var up = IsActionPressed(PlayerInputAction.Up) ? -1 : 0;
        var down = IsActionPressed(PlayerInputAction.Down) ? 1 : 0;
        var left = IsActionPressed(PlayerInputAction.Left) ? -1 : 0;
        var right = IsActionPressed(PlayerInputAction.Right) ? 1 : 0;

        return new Vector2(left + right, up + down);
    }
}
