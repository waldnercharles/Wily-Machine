using Godot;

namespace Spaghetti;

public sealed class ClimbMovementState : MovementState
{
    private const int LADDER_COLLISION_MASK = 7;

    public ClimbMovementState(Player player) : base(player) { }

    public override void Enter(State? previousState)
    {
        Player.IsClimbing = true;

        Player.IsJumping = false;
        Player.IsFalling = false;
        Player.IsAirborn = false;
        Player.IsDecelerating = false;
        Player.IsFullAcceleration = false;
        Player.IsIdle = false;
        Player.IsWalking = false;

        Player.Velocity = Vector2.Zero;
        Player.IsAffectedByGravity = false;

        if (Player.Ladder == null)
        {
            Player.SetMovementState<JumpMovementState>();
            return;
        }

        Log.Assert(Player.CollisionShape != null, "Player.CollisionShape != null");

        var playerCollisionShape = Player.CollisionShape;
        var ladderCollisionShape = Player.Ladder.CollisionShape;

        var distanceToLadder = ladderCollisionShape.GlobalPosition - playerCollisionShape.GlobalPosition;
        distanceToLadder.Y = 0;

        Player.SetCollisionMaskValue(LADDER_COLLISION_MASK, false);
        Player.Position += distanceToLadder;
    }

    public override void Exit()
    {
        Player.IsClimbing = false;
        Player.IsAffectedByGravity = true;
        Player.SetCollisionMaskValue(LADDER_COLLISION_MASK, true);
    }

    public override StateChange Update(float delta)
    {
        Player.IsAffectedByGravity = false;

        if (Player.Ladder == null)
        {
            Player.IsFalling = true;
            Player.EnqueueMovementState<JumpMovementState>();
            return StateChange.Next;
        }

        var velocity = Vector2.Zero;
        var controller = Player.Controller;

        Player.Velocity = velocity;

        if (Player.IsShooting)
        {
            if (controller.ShouldMoveLeft())
            {
                Player.Direction = Vector2.Left;
            }
            else if (controller.ShouldMoveRight())
            {
                Player.Direction = Vector2.Right;
            }

            Player.ChooseSpriteAnimation();
            return StateChange.None;
        }

        Log.Assert(Player.CollisionShape != null);
        Log.Assert(Player.Ladder.CollisionShape != null);

        var playerFeet = Player.CollisionShape.GlobalPosition.Y +
                         Player.CollisionShape.Shape.GetRect().End.Y;

        var ladderTop = Player.Ladder.TopCollisionShape.GlobalPosition.Y +
                        Player.Ladder.TopCollisionShape.Shape.GetRect().Position.Y;

        var distanceToLadderTop = playerFeet - ladderTop;

        Player.IsTouchingLadderTop = distanceToLadderTop <= 8;
        Player.ChooseSpriteAnimation();

        Player.PauseSpriteAnimation();

        if (controller.ShouldMoveUp())
        {
            Player.ChooseSpriteAnimation();

            velocity.Y = -Player.ClimbVelocity.Y;

            var newPosition = playerFeet + velocity.Y * delta;
            var willBeAboveLadder = newPosition < ladderTop - 1;

            if (willBeAboveLadder)
            {
                Player.Position += new Vector2(0, -distanceToLadderTop);

                Player.Velocity = Vector2.Zero;
                Player.EnqueueMovementState<IdleMovementState>();
                return StateChange.Next;
            }

            Player.Velocity = velocity;
        }
        else if (controller.ShouldMoveDown())
        {
            Player.ChooseSpriteAnimation();

            velocity.Y = Player.ClimbVelocity.Y;
            Player.Velocity = velocity;

            if (Player.IsOnFloor() && distanceToLadderTop > 0)
            {
                Player.EnqueueMovementState<IdleMovementState>();
                return StateChange.Next;
            }
        }
        else if (controller.ShouldJump())
        {
            Player.EnqueueMovementState<JumpMovementState>();
            return StateChange.Next;
        }

        return StateChange.None;
    }
}
