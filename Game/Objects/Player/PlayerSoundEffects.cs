using Godot;

namespace Spaghetti;

public sealed partial class PlayerSoundEffects : Node
{
    [Export] public AudioStreamPlayer2D? Land { get; set; }
}
