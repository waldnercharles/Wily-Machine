using Godot;

namespace Spaghetti;

public sealed class IdleMovementState : MovementState
{
    public IdleMovementState(Player player) : base(player) { }

    public override void Enter(State? previousState)
    {
        Player.IsIdle = true;
        Player.IsFullAcceleration = false;

        Player.Velocity = new Vector2(0f, Player.Velocity.Y);
        Player.ChooseSpriteAnimation();
    }

    public override void Exit()
    {
        Player.IsIdle = false;
    }

    public override StateChange Update(float delta)
    {
        Player.IsDecelerating = false;
        Player.ChooseSpriteAnimation();

        var controller = Player.Controller;

        if (!Player.IsOnFloor())
        {
            Player.EnqueueMovementState<JumpMovementState>();
            return StateChange.Next;
        }

        if (controller.ShouldMoveLeft() && !controller.ShouldMoveRight() ||
            !controller.ShouldMoveLeft() && controller.ShouldMoveRight())
        {
            Player.EnqueueMovementState<WalkMovementState>();
            return StateChange.Next;
        }

        if (controller.ShouldSlide() && Player.CanSlide())
        {
            Player.EnqueueMovementState<SlideMovementState>();
            return StateChange.Next;
        }

        if (controller.ShouldJump() && !controller.ShouldSlide() && Player.CanJump())
        {
            Player.EnqueueMovementState<JumpMovementState>();
            return StateChange.Next;
        }

        return StateChange.None;
    }
}
