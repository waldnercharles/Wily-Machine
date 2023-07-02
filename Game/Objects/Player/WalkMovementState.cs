using Godot;

namespace Spaghetti;

public sealed class WalkMovementState : MovementState
{
    private int m_TipToeFrameCounter = 0;

    private Vector2 m_PreviousDirection;
    public WalkMovementState(Player player) : base(player) { }

    public override void Enter(State? previousState)
    {
        Player.IsWalking = true;
        Player.IsAirborn = false;
        Player.IsIdle = false;

        m_PreviousDirection = Vector2.Zero;
        m_TipToeFrameCounter = 0;
    }

    public override void Exit()
    {
        Player.IsWalking = false;
    }

    public override StateChange Update(float delta)
    {
        var controller = Player.Controller;
        m_PreviousDirection = Player.Direction;

        var isWalking = controller.ShouldMoveLeft() ^ controller.ShouldMoveRight();

        if (!isWalking)
        {
            Player.SetNextMovementState<IdleMovementState>();
            return StateChange.Next;
        }

        if (controller.ShouldMoveLeft())
        {
            Player.Direction = Vector2.Left;
        }
        else if (controller.ShouldMoveRight())
        {
            Player.Direction = Vector2.Right;
        }

        if (Player.Direction != m_PreviousDirection)
        {
            Player.IsFullAcceleration = false;
            m_TipToeFrameCounter = 0;
        }

        var velocity = Player.Velocity;

        // Tiptoe
        if (!Player.IsFullAcceleration)
        {
            velocity.X = Player.TipToeVelocity.X / Player.TipToeFrames * Player.Direction.X;

            Player.IsFullAcceleration = m_TipToeFrameCounter >= Player.TipToeFrames;
            m_TipToeFrameCounter++;
        }
        // Normal Walk
        else
        {
            velocity.X = Player.WalkVelocity.X * Player.Direction.X;
        }

        Player.Velocity = velocity;
        Player.ChooseSpriteAnimation();

        if (controller.ShouldSlide() && Player.CanSlide())
        {
            Player.SetNextMovementState<SlideMovementState>();
            return StateChange.Next;
        }

        if (controller.ShouldJump() && !controller.ShouldSlide() && Player.CanJump() || !Player.IsOnFloor())
        {
            Player.SetNextMovementState<JumpMovementState>();
            return StateChange.Next;
        }

        return StateChange.None;
    }
}
