using Godot;

namespace Spaghetti.Godot;

public partial class PlayerIdleState : PlayerOnGroundState
{
    [Export] public int StillFrameCount { get; set; } = 4;
    [Export] public Vector2 BusterPosition { get; set; } = new Vector2(21, 0);

    private int m_FrameCount;

    public override void Enter<TState>(TState? previousState) where TState : class
    {
        Player.AnimationPlayer.Play(PlayerAnimation.Idle);
        // TODO: Buster Position
        m_FrameCount = -1;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void HandleCommand(PlayerCommand command)
    {
        if (command is PlayerCommand.Shoot)
        {
            Player.AnimationPlayer.Play(PlayerAnimation.IdleShoot);
            Player.Shoot();
        }

        if (command is PlayerCommand.WeaponPrevious or PlayerCommand.WeaponNext)
        {
            // TODO: Change Weapon
        }

        base.HandleCommand(command);
    }

    public override void Update(float delta)
    {
        m_FrameCount++;

        if (m_FrameCount > StillFrameCount)
        {
            Player.IsStill = true;
        }

        Player.Velocity = Vector2.Down * Player.Gravity;
        Player.MoveAndSlide();

        var direction = Player.Input.GetInputDirection();

        if (!Player.IsOnFloor())
        {
            Player.StateMachine.ChangeState<PlayerJumpState>();
        }
        else if (direction.X != 0)
        {
            Player.StateMachine.ChangeState<PlayerMoveState>();
        }

        if (Player.ChargeLevel > 0 && !Player.Input.IsActionPressed(PlayerInputAction.Shoot))
        {
            HandleCommand(PlayerCommand.Shoot);
        }
    }

    public override void AnimationFinished(string animationName)
    {
        if (animationName == PlayerAnimation.IdleShoot)
        {
            Player.AnimationPlayer.Play(PlayerAnimation.Idle);
        }
    }
}
