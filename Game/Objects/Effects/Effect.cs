using Godot;

namespace WilyMachine;

[SceneTree]
public partial class Effect : Node2D
{
    [Export] public bool Despawn { get; set; } = true;

    public override void _Ready()
    {
        _.VisibleOnScreenNotifier2D.ScreenExited += ScreenExited;
    }

    private void ScreenExited()
    {
        if (Despawn)
        {
            QueueFree();
        }
    }
}
