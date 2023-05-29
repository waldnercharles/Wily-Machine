using Godot;

namespace Spaghetti.Godot;

public partial class PlayerJumpShootState : PlayerJumpState
{
    [Export] public Vector2I WeaponPosition { get; set; } = new Vector2I(17, -4);

    public override void Enter<TState>(TState? previousState) where TState : class
    {
        Player.AnimationPlayer.Play(PlayerAnimation.JumpShoot);
        Player.WeaponPosition = WeaponPosition;

        Player.Shoot();
    }

    public override void HandleCommand(PlayerCommand command)
    {
        if (command is PlayerCommand.JumpStop && Player.Velocity.Y < 0)
        {
            Player.Velocity = new Vector2(Player.Velocity.X, 0);
        }

        if (command is PlayerCommand.Shoot)
        {
            Player.AnimationPlayer.Stop();
            Player.AnimationPlayer.Play(PlayerAnimation.JumpShoot);
            Player.Shoot();
        }

        if (command is PlayerCommand.WeaponPrevious or PlayerCommand.WeaponNext)
        {
            // TODO: Change Weapon
        }
    }

    public override void AnimationFinished(string animationName)
    {
        Player.StateMachine.ChangeState<PlayerJumpState>();
    }
}
