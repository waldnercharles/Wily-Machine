using Godot;
using Spaghetti.Messaging;

namespace Spaghetti;

[SceneTree]
public partial class PlayerCamera : Camera2D
{
    public readonly record struct BeforeRoomTransition();
    public readonly record struct AfterRoomTransition();

    [Export] public Player Player { get; set; } = null!;
    [Export] public float TransitionTime { get; set; } = 1.0f;

    public ColorRect BlackScreen => _.CanvasLayer.ColorRect;

    private readonly Subscription m_RoomEnteredSubscription;

    public Room? CurrentRoom
    {
        get => m_CurrentRoom;
        set
        {
            if (CurrentRoom != null)
            {
                CurrentRoom.Active = false;
            }

            if (value == null)
            {
                m_CurrentRoom = null;
                return;
            }

            m_CurrentRoom = value;
            m_CurrentRoom.Active = true;

            LimitLeft = m_CurrentRoom.LimitLeft;
            LimitRight = m_CurrentRoom.LimitRight;
            LimitTop = m_CurrentRoom.LimitTop;
            LimitBottom = m_CurrentRoom.LimitBottom;
        }
    }

    private Room? m_CurrentRoom;

    private AnimationPlayer m_AnimationPlayer => _.AnimationPlayer;

    public PlayerCamera()
    {
        m_RoomEnteredSubscription =
            MessageBus.Subscribe((in Room.RoomEnteredMessage message) => TransitionToRoom(message.Room));
    }

    public override void _Ready()
    {
        GlobalPosition = Player.GlobalPosition;
    }

    public override void _PhysicsProcess(double delta)
    {
        GlobalPosition = Player.GlobalPosition;
    }

    public void TransitionToRoom(Room room)
    {
        if (CurrentRoom == null)
        {
            CurrentRoom = room;
            return;
        }

        MessageBus.Publish(new BeforeRoomTransition());

        GetTree().Paused = true;
        SetPhysicsProcess(false);

        var transitionDirection = GetTransitionDirection(CurrentRoom, room);
        var transitionPosition = 0f;

        if (transitionDirection == Vector2.Right)
        {
            transitionPosition = room.LimitLeft;
        }
        else if (transitionDirection == Vector2.Left)
        {
            transitionPosition = room.LimitRight;
        }
        else if (transitionDirection == Vector2.Up)
        {
            transitionPosition = room.LimitBottom;
        }
        else if (transitionDirection == Vector2.Down)
        {
            transitionPosition = room.LimitTop;
        }

        var internalResolutionCenter = Global.InternalResolution / 2;

        Position = GetScreenCenterPosition();
        var targetCameraPosition = Position;

        if (transitionDirection.X != 0)
        {
            LimitLeft = -10000000;
            LimitRight = 10000000;
            targetCameraPosition.X = transitionPosition + internalResolutionCenter.X * transitionDirection.X;
        }
        else if (transitionDirection.Y != 0)
        {
            LimitTop = -10000000;
            LimitBottom = 10000000;
            targetCameraPosition.Y = transitionPosition + internalResolutionCenter.Y * transitionDirection.Y;
        }

        var tween = CreateTween();

        tween
            .TweenProperty(this, "position", targetCameraPosition, TransitionTime)
            .SetTrans(Tween.TransitionType.Linear)
            .SetEase(Tween.EaseType.InOut);

        if (!Player.IsDead)
        {
            var playerSize =
                Player.CollisionShape?.Shape.GetRect().Size ?? Vector2.Zero;

            var playerMovement = transitionDirection.X != 0
                ? new Vector2(transitionPosition - Player.GlobalPosition.X + playerSize.X * transitionDirection.X, 0)
                : new Vector2(0, transitionPosition - Player.GlobalPosition.Y + playerSize.Y * transitionDirection.Y);

            Log.Trace(playerMovement.ToString());

            tween
                .Parallel()
                .TweenProperty(Player, "global_position", playerMovement, TransitionTime)
                .AsRelative()
                .SetTrans(Tween.TransitionType.Linear)
                .SetEase(Tween.EaseType.InOut);
        }

        ToSignal(tween, Tween.SignalName.Finished).OnCompleted(() =>
        {
            CurrentRoom = room;

            GetTree().Paused = false;
            SetPhysicsProcess(true);

            MessageBus.Publish(new AfterRoomTransition());
        });
    }

    private Vector2 GetTransitionDirection(Room? from, Room? to)
    {
        var transitionDirection = Vector2.Zero;

        if (from == null || to == null)
        {
            return Vector2.Zero;
        }

        var isVertical = from.LimitBottom <= to.LimitTop || from.LimitTop >= to.LimitBottom;

        Log.Trace("From: {0}, {1}", from.LimitBottom, from.LimitTop);
        Log.Trace("To: {0}, {1}", to.LimitBottom, to.LimitTop);

        if (isVertical)
        {
            Log.Trace("IsVertical");

            if (to.GlobalPosition.Y > from.GlobalPosition.Y)
            {
                transitionDirection = Vector2.Down;
            }
            else if (to.GlobalPosition.Y < from.GlobalPosition.Y)
            {
                transitionDirection = Vector2.Up;
            }
        }
        else
        {
            if (to.GlobalPosition.X > from.GlobalPosition.X)
            {
                transitionDirection = Vector2.Right;
            }
            else if (to.GlobalPosition.X < from.GlobalPosition.X)
            {
                transitionDirection = Vector2.Left;
            }
        }

        return transitionDirection;
    }

    protected override void Dispose(bool disposing)
    {
        m_RoomEnteredSubscription.Dispose();

        base.Dispose(disposing);
    }
}
