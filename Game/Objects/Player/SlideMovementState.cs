using Godot;

namespace Spaghetti;

public sealed class SlideMovementState : MovementState
{
    private int m_SlideFrameCounter = 0;

    public SlideMovementState(Player player) : base(player) { }

    public override void Enter(State? previousState)
    {
        Player.Controller.ResetSlideBuffer();

        Player.UprightCollisionShape.Disabled = true;
        Player.SlideCollisionShape.Disabled = false;

        Player.CollisionShape = Player.SlideCollisionShape;

        m_SlideFrameCounter = 0;

        Player.IsWalking = false;
        Player.IsIdle = false;
        Player.IsAirborn = false;
        Player.IsFullAcceleration = true;
        Player.IsSliding = true;

        Player.StopShooting();

        // TODO: Sliding Particles
        Player.ChooseSpriteAnimation();
    }

    public override void Exit()
    {
        Log.Assert(!Player.IsInTunnel, "Player is in tunnel");

        Player.UprightCollisionShape.Disabled = false;
        Player.SlideCollisionShape.Disabled = true;

        Player.CollisionShape = Player.UprightCollisionShape;

        Player.IsSliding = false;
        Player.IsInTunnel = false;
    }

    public override StateChange Update(float delta)
    {
        m_SlideFrameCounter++;

        Player.IsInTunnel = Player.IsOnFloor() && Player.TestUprightCollisionShape();

        var controller = Player.Controller;

        if (Player.IsInTunnel || m_SlideFrameCounter < Player.SlideFrames &&
            !Player.TestSlideCollisionShape() && Player.IsOnFloor())
        {
            var velocity = Player.Velocity;

            if (controller.ShouldMoveLeft())
            {
                Player.Direction = Vector2.Left;
                velocity.X = -Player.SlideVelocity.X;
            }
            else if (controller.ShouldMoveRight())
            {
                Player.Direction = Vector2.Right;
                velocity.X = Player.SlideVelocity.X;
            }

            Player.Velocity = velocity;

            if (!Player.IsInTunnel && controller.ShouldJump() && Player.CanJump() &&
                !controller.ShouldSlide())
            {
                Player.EnqueueMovementState<JumpMovementState>();
                return StateChange.Next;
            }
        }
        else
        {
            if (controller.ShouldMoveLeft() ^ controller.ShouldMoveRight())
            {
                Player.IsFullAcceleration = true;
                Player.EnqueueMovementState<WalkMovementState>();
                return StateChange.Next;
            }

            if (!Player.IsOnFloor())
            {
                Player.EnqueueMovementState<JumpMovementState>();
                return StateChange.Next;
            }

            Player.IsDecelerating = true;
            Player.EnqueueMovementState<IdleMovementState>();
            return StateChange.Next;
        }

        return StateChange.None;
    }
}
