using Godot;

namespace Spaghetti;

public sealed class SlideMovementState : MovementState
{
    private Vector2? m_PreviousDirection;

    public SlideMovementState(Player player) : base(player) { }

    public override void Enter(State? previousState)
    {
        m_PreviousDirection = null;

        Player.Controller.ResetSlideBuffer();

        Player.UprightCollisionShape.Disabled = true;
        Player.SlideCollisionShape.Disabled = false;

        Player.HurtboxUprightCollisionShape.Disabled = true;
        Player.HurtboxSlideCollisionShape.Disabled = false;

        Player.CurrentCollisionShape = Player.SlideCollisionShape;

        Player.RemainingSlideFrames = Player.SlideFrames;

        Player.IsWalking = false;
        Player.IsIdle = false;
        Player.IsAirborn = false;
        Player.IsFullAcceleration = true;
        Player.IsSliding = true;

        Player.StopShooting();

        // TODO: Sliding Particles
        Player.ChooseSpriteAnimation();
    }

    public override bool CanExit()
    {
        return !Player.IsInTunnel;
    }

    public override void Exit()
    {
        Player.UprightCollisionShape.Disabled = false;
        Player.SlideCollisionShape.Disabled = true;

        Player.HurtboxUprightCollisionShape.Disabled = false;
        Player.HurtboxSlideCollisionShape.Disabled = true;

        Player.CurrentCollisionShape = Player.UprightCollisionShape;

        Player.IsSliding = false;
        Player.IsInTunnel = false;
    }

    public override StateChange Update(float delta)
    {
        Player.IsInTunnel = Player.IsOnFloor() && Player.TestUprightCollisionShape();

        var controller = Player.Controller;

        Player.RemainingSlideFrames--;

        if (Player.IsInTunnel ||
            Player.RemainingSlideFrames > 0 && !Player.TestSlideCollisionShape() && Player.IsOnFloor())
        {
            var velocity = Player.Velocity;

            if (controller.ShouldMoveLeft())
            {
                m_PreviousDirection ??= Vector2.Left;
                Player.Direction = Vector2.Left;
                velocity.X = -Player.SlideVelocity.X;
            }
            else if (controller.ShouldMoveRight())
            {
                m_PreviousDirection ??= Vector2.Right;
                Player.Direction = Vector2.Right;
                velocity.X = Player.SlideVelocity.X;
            }

            if (!Player.IsInTunnel && Player.Direction != m_PreviousDirection)
            {
                Player.SetNextMovementState<WalkMovementState>();
                return StateChange.Next;
            }

            Player.Velocity = velocity;

            if (controller.ShouldJump() && Player.CanJump() && !controller.ShouldSlide())
            {
                Player.SetNextMovementState<JumpMovementState>();
                return StateChange.Next;
            }
        }
        else
        {
            if (Player.IsOnFloor())
            {
                if (controller.ShouldMoveLeft() ^ controller.ShouldMoveRight())
                {
                    Player.IsFullAcceleration = true;
                    Player.SetNextMovementState<WalkMovementState>();
                    return StateChange.Next;
                }

                Player.SetNextMovementState<IdleMovementState>();
                return StateChange.Next;
            }

            Player.SetNextMovementState<JumpMovementState>();
            return StateChange.Next;
        }

        return StateChange.None;
    }
}
