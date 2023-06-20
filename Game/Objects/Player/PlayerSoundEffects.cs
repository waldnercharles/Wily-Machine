using Godot;

namespace Spaghetti;

public partial class PlayerSoundEffects : Node
{
    public AudioStreamPlayer Land { get; set; } = null!;
    public AudioStreamPlayer2D Hit { get; set; } = null!;
}
