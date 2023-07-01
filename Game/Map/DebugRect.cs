using Godot;

namespace Spaghetti;

[Tool]
public partial class DebugRect : ReferenceRect
{
    [Export] public CollisionShape2D? CollisionShape { get; set; }

    public override void _Ready()
    {
        ProcessMode = Engine.IsEditorHint() ? ProcessModeEnum.Always : ProcessModeEnum.Disabled;
    }

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint() && CollisionShape?.Shape is RectangleShape2D rectangleShape)
        {
            Size = rectangleShape.Size;
            GlobalPosition = -rectangleShape.Size / 2f;

            QueueRedraw();
        }
    }

    public override void _Draw()
    {
        base._Draw();
    }
}
