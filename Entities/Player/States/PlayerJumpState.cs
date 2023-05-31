using System;
using Godot;

namespace Spaghetti.Godot;

public partial class PlayerJumpState : PlayerState
{
    [Export] public AudioStreamPlayer LandingSoundEffect = null!;

    public override void Enter<TState>(TState? previousState) where TState : class
    {
        Player.AnimationPlayer.Play(PlayerAnimation.Jump);

        if (Player.IsOnFloor())
        {
            // Delay Gravity by 2 frames to be able to jump 3 tiles high.
            // Gravity should only be applied in the air, i.e. After the first jump frame.
            Player.Velocity =
                new Vector2(Player.Velocity.X, -Player.JumpSpeed - Player.Gravity * 2);
        }
    }

    public override void HandleCommand(PlayerCommand command)
    {
        if (command is PlayerCommand.JumpStop && Player.Velocity.Y < 0)
        {
            Player.Velocity = new Vector2(Player.Velocity.X, 0);
        }

        if (command is PlayerCommand.Shoot)
        {
            Player.StateMachine.ChangeState<PlayerJumpShootState>();
        }

        if (command is PlayerCommand.WeaponPrevious or PlayerCommand.WeaponNext)
        {
            // TODO: Weapon change
        }
    }

    public override void Update(float delta)
    {
        var direction = Player.Input.GetInputDirection();
        Player.UpdateSpriteDirection(direction);

        var velocity = Player.Velocity;

        velocity.Y =
            Mathf.Clamp(velocity.Y + Player.Gravity, -Player.MaxFallSpeed, Player.MaxFallSpeed);

        if (Player.IsOnCeiling() && velocity.Y < 0)
        {
            velocity.Y = 0;
        }

        velocity.X = Player.WalkSpeed * direction.X;

        Player.Velocity = velocity;
        Player.MoveAndSlide();

        if (Player.IsOnFloor())
        {
            LandingSoundEffect.Play();
            Player.StateMachine.ChangeState<PlayerMoveState>();
        }
    }
}
