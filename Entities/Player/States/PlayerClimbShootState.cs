using Godot;

namespace Spaghetti.Godot;

public partial class PlayerClimbShootState : PlayerClimbState
{
    [Export] public Vector2 BusterPosition { get; set; } = new Vector2(17, -2);

    public override void Enter<TState>(TState? previousState) where TState : class
    {
        Player.IsClimbing = true;
        // TODO: Play Climb Shoot Animation
        // TODO: MegaBuster Position
        Player.Shoot();
    }

    public override void Exit()
    {
        Player.IsClimbing = false;
    }

    public override void HandleCommand(PlayerCommand command)
    {
        if (command is PlayerCommand.Shoot)
        {
            // TODO: Animation Seek(0)
            Player.Shoot();
        }

        if (command is PlayerCommand.DropDown)
        {
            Player.StateMachine.ChangeState<PlayerJumpState>();
        }

        if (command is PlayerCommand.WeaponPrevious or PlayerCommand.WeaponNext)
        {
            // TODO: Change Weapon
        }
    }

    public override void Update(float delta)
    {
        var direction = Player.Input.GetInputDirection();

        if (direction.X != 0)
        {
            // TODO: Update LastShootingDirection
            // TODO: Update SpriteDirection
        }
    }

    public override void AnimationFinished(string animationName)
    {
        Player.StateMachine.ChangeState<PlayerClimbState>();
    }
}
