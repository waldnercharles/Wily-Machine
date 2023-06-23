using Godot;
using Spaghetti.Messaging;

namespace Spaghetti;

[Tool] [SceneTree]
public partial class Room : TileMap
{
    public readonly record struct RoomEnteredMessage(Room Room);

    public Area2D Area => _.Area;
    public CollisionShape2D CollisionShape => _.Area.CollisionShape2D;

    public ReferenceRect DebugRect => _.DebugRect;

    [Export(PropertyHint.Range, "400,65536,")]
    public int Width
    {
        get => m_Width;
        set
        {
            m_Width = value;
            UpdateSize();
        }
    }

    [Export(PropertyHint.Range, "240,65536,")]
    public int Height
    {
        get => m_Height;
        set
        {
            m_Height = value;
            UpdateSize();
        }
    }

    private bool m_Active;

    public bool Active
    {
        get => m_Active;
        set => m_Active = value;
    }

    public int LimitLeft => (int)(CollisionShape.Shape.GetRect().Position.X + Area.GlobalPosition.X);
    public int LimitRight => (int)(CollisionShape.Shape.GetRect().End.X + Area.GlobalPosition.X);
    public int LimitTop => (int)(CollisionShape.Shape.GetRect().Position.Y + Area.GlobalPosition.Y);
    public int LimitBottom => (int)(CollisionShape.Shape.GetRect().End.Y + Area.GlobalPosition.Y);

    private int m_Width = 400;
    private int m_Height = 240;

    private PlayerCamera? m_Camera => (Owner as Level)?.Camera;

    public override void _Ready()
    {
        Area.BodyEntered += BodyEntered;
        Area.BodyExited += BodyExited;

        base._Ready();
    }

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint())
        {
            DebugRect.Size = new Vector2(Width, Height);
            DebugRect.Position = Area.GlobalPosition + -new Vector2(Width, Height) / 2f;
            QueueRedraw();
        }
    }

    private void BodyEntered(Node body)
    {
        if (Active || body is not Player)
        {
            return;
        }

        MessageBus.Publish(new RoomEnteredMessage(this));
    }

    private void BodyExited(Node2D body)
    {
        if (body is not Player)
        {
            return;
        }
    }

    private void UpdateSize()
    {
        if (CollisionShape.Shape is RectangleShape2D rectangleShape2D)
        {
            rectangleShape2D.Size = new Vector2(Width, Height);
        }

        if (Engine.IsEditorHint())
        {
            DebugRect.Size = new Vector2(Width, Height);
            DebugRect.Position = Area.GlobalPosition + -new Vector2(Width, Height) / 2f;
            QueueRedraw();
        }
    }
}
