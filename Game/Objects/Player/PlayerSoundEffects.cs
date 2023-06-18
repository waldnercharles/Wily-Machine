using Godot;

namespace Spaghetti;

public partial class PlayerSoundEffects : Node
{
    [Node] public AudioStreamPlayer Land { get; set; } = null!;
    [Node] public AudioStreamPlayer2D Hit { get; set; } = null!;

    public override void _Ready()
    {
        RegisterNodes();
    }
}
