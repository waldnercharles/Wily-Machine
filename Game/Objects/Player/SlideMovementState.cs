using Godot;

namespace WilyMachine;

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

        var slideDust = Player.Dust?.Instantiate<Effect>();

        if (slideDust != null)
        {
            Player.GetParent().AddChild(slideDust);
            var slideBoundingBox = Player.SlideCollisionShape.Shape.GetRect();
            slideDust.Position =
                Player.Position +
                new Vector2(slideBoundingBox.Position.X - 2, slideBoundingBox.End.Y + 2) *
                new Vector2(Player.Sprite.FlipH ? -1 : 1, Player.Sprite.FlipV ? -1 : 1);
        }

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
        Player.RemainingSlideFrames--;

        var ceilingCollisionTestOffset = Player.UpDirection;

        if (Player.SlideDownOneTileGapsNextToWall && Player.IsOnWall())
        {
            var depth = Global.TileSize -
                        ((Player.UprightCollisionShape.Shape as RectangleShape2D)?.Size.Y ?? Global.TileSize);

            ceilingCollisionTestOffset = new Vector2(0, depth) * Player.UpDirection;
        }

        var collidingWithCeiling = Player.TryGetUprightCollision(ceilingCollisionTestOffset, out _);

        Player.IsInTunnel = Player.IsOnFloor && collidingWithCeiling;

        if (Player.IsInTunnel ||
            Player.RemainingSlideFrames > 0 && Player.IsOnFloor &&
            !(Player.TryGetSlideCollision(Player.SlideVelocity * delta * Player.Direction.X, out var collision) &&
              !collision.IsFloorCollision(Player.UpDirection, Player.FloorMaxAngle)))

        {
            var velocity = Player.Velocity;

            if (Player.Controller.ShouldMoveLeft())
            {
                m_PreviousDirection ??= Vector2.Left;
                Player.Direction = Vector2.Left;
                velocity.X = -Player.SlideVelocity.X;
            }
            else if (Player.Controller.ShouldMoveRight())
            {
                m_PreviousDirection ??= Vector2.Right;
                Player.Direction = Vector2.Right;
                velocity.X = Player.SlideVelocity.X;
            }

            if (!Player.IsInTunnel && Player.Direction != m_PreviousDirection)
            {
                Log.Trace("Hit");
                Player.SetNextMovementState<WalkMovementState>();
                return StateChange.Next;
            }

            Player.Velocity = velocity;

            if (Player.Controller.ShouldJump() && Player.CanJump() && !Player.Controller.ShouldSlide())
            {
                Player.SetNextMovementState<JumpMovementState>();
                return StateChange.Next;
            }
        }
        else
        {
            if (Player.IsOnFloor)
            {
                if (Player.Controller.ShouldMoveLeft() ^ Player.Controller.ShouldMoveRight())
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
