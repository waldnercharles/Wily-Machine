using Godot;

namespace Spaghetti;

[Tool]
public partial class Section : Node2D
{
    [Export] public Area2D? Area2D { get; set; }
    [Export] public CollisionShape2D? Area2D_CollisionShape2D { get; set; }

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

    public bool Active { get; set; }

    public int LimitLeft { get; set; }
    public int LimitRight { get; set; }
    public int LimitTop { get; set; }
    public int LimitBottom { get; set; }

    private int m_Width = 400;
    private int m_Height = 240;

    private PlayerCamera? m_Camera;

    public Section()
    {
        ZIndex = (int)RenderingServer.CanvasItemZMax;
    }

    public override void _Ready()
    {
        if (Engine.IsEditorHint())
        {
            return;
        }

        var rect = Area2D_CollisionShape2D!.Shape.GetRect();
        LimitLeft = (int)rect.Position.X;
        LimitTop = (int)rect.Position.Y;
        LimitRight = (int)rect.End.X;
        LimitBottom = (int)rect.End.Y;

        Area2D!.BodyEntered += BodyEntered;
        Area2D.BodyExited += BodyExited;

        CallDeferred(nameof(FindPlayerCamera));
    }

    public override void _Draw()
    {
        if (Engine.IsEditorHint())
        {
            DrawRect(new Rect2(-Width / 2f, -Height / 2f, Width, Height), Colors.Crimson, false, 4);
        }
        else
        {
            base._Draw();
        }
    }

    private void BodyEntered(Node body)
    {
        if (Active || body is not Player)
        {
            return;
        }

        if (m_Camera != null)
        {
            m_Camera.Section ??= this;
        }
    }

    private void BodyExited(Node2D body)
    {
        if (body is not Player)
        {
            return;
        }

        if (m_Camera != null)
        {
            m_Camera.Section = null;
        }
    }

    private void FindPlayerCamera()
    {
        foreach (var child in Owner.GetChildren())
        {
            if (child is PlayerCamera camera)
            {
                m_Camera = camera;
                return;
            }
        }
    }

    private void UpdateSize()
    {
        if (Engine.IsEditorHint())
        {
            QueueRedraw();
        }

        if (Area2D_CollisionShape2D?.Shape is RectangleShape2D rectangleShape2D)
        {
            rectangleShape2D.Size = new Vector2(Width, Height);
        }

        if (m_Camera?.Section == this)
        {
            m_Camera.Section = null;
            m_Camera.Section = this;
        }
    }
}
