using Godot;

namespace Spaghetti;

[SceneTree]
public partial class Spike : StaticBody2D, IFactionComponent
{
    [Export] public Faction Faction { get; set; } = Faction.Enemy;
}
