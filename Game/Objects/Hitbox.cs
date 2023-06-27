using Godot;

namespace Spaghetti;

public partial class Hitbox : Area2D, IFactionComponent
{
    [Export] public int Damage { get; set; } = 1;

    public Faction Faction => Owner is IFactionComponent factionComponent ? factionComponent.Faction : Faction.Neutral;
}
