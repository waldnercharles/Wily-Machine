using Godot;
using Spaghetti.Messaging;

namespace Spaghetti;

[SceneTree] [Tool]
public partial class Room : TileMap
{
    public readonly record struct RoomEnteredMessage(Room Room);

    public Area2D Area => _.Area;
    public CollisionShape2D CollisionShape => _.Area.CollisionShape2D;

    public ReferenceRect DebugRect => _.DebugRect;

    [Export(PropertyHint.Range, "25,65536,")]
    public int Width
    {
        get => m_Width;
        set
        {
            m_Width = value;
            UpdateSize();
        }
    }

    [Export(PropertyHint.Range, "15,65536,")]
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
        set
        {
            m_Active = value;

            if (!Engine.IsEditorHint())
            {
                Visible = value;
                ProcessMode = value ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;
            }
        }
    }

    public int LimitLeft => (int)(CollisionShape.Shape.GetRect().Position.X + Area.GlobalPosition.X);
    public int LimitRight => (int)(CollisionShape.Shape.GetRect().End.X + Area.GlobalPosition.X);
    public int LimitTop => (int)(CollisionShape.Shape.GetRect().Position.Y + Area.GlobalPosition.Y);
    public int LimitBottom => (int)(CollisionShape.Shape.GetRect().End.Y + Area.GlobalPosition.Y);

    private int m_Width = 25;
    private int m_Height = 15;

    private PlayerCamera? m_Camera => (Owner as Level)?.Camera;

    public override void _Ready()
    {
        if (Engine.IsEditorHint())
        {
            SetNotifyTransform(true);
            UpdateSize();

            return;
        }

        if (Material is ShaderMaterial shader)
        {
            shader.SetShaderParameter("enabled", false);
        }

        Active = false;
        RemoveExteriorCells();

        base._Ready();

        Area.BodyEntered += BodyEntered;
        ProcessMode = ProcessModeEnum.Disabled;
    }

    public override void _Notification(int what)
    {
        if (Engine.IsEditorHint())
        {
            if (what == NotificationTransformChanged)
            {
                UpdateSize();
            }
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

    private void RemoveExteriorCells()
    {
        if (!Engine.IsEditorHint())
        {
            if (CollisionShape.Shape is not RectangleShape2D rectangleShape)
            {
                return;
            }

            var rect = rectangleShape.GetRect();
            rect.Position += Area.Position;

            var layerCount = GetLayersCount();

            for (var layer = 0; layer < layerCount; layer++)
            {
                foreach (var cell in GetUsedCells(layer))
                {
                    var localPosition = MapToLocal(cell);

                    if (!rect.HasPoint(localPosition))
                    {
                        SetCell(layer, cell, -1);
                    }
                }
            }
        }
    }

    private void UpdateSize()
    {
        var tileSize = TileSet?.TileSize ?? new Vector2I(16, 16);

        Area.Position = new Vector2(Width % 2, Height % 2) * tileSize / 2f;

        if (CollisionShape.Shape is RectangleShape2D rectangleShape2D)
        {
            rectangleShape2D.Size = new Vector2(Width, Height) * tileSize;
        }

        if (Engine.IsEditorHint())
        {
            DebugRect.Size = new Vector2(Width, Height) * tileSize;
            DebugRect.Position = Position + Area.Position + -new Vector2(Width, Height) * tileSize / 2f;
            QueueRedraw();

            if (Material is ShaderMaterial shader)
            {
                shader.SetShaderParameter("enabled", true);
                shader.SetShaderParameter("position", GlobalPosition);
                shader.SetShaderParameter("width", Width);
                shader.SetShaderParameter("height", Height);
                shader.SetShaderParameter("tile_size", TileSet?.TileSize ?? new Vector2(16f, 16f));
            }
        }
    }
}
