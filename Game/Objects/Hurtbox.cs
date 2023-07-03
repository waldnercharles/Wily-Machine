using System;
using System.Collections.Generic;
using Godot;

namespace WilyMachine;

[SceneTree]
public partial class Hurtbox : Area2D, IFactionComponent
{
    public event Action<Hitbox>? HitboxEntered;
    public event Action<Hitbox>? HitboxExited;

    public Faction Faction => Owner is IFactionComponent factionComponent ? factionComponent.Faction : Faction.Neutral;

    private HashSet<Hitbox>? m_Hitboxes;

    public IReadOnlyCollection<Hitbox> Hitboxes => (IReadOnlyCollection<Hitbox>?)m_Hitboxes ?? Array.Empty<Hitbox>();

    public override void _Ready()
    {
        AreaEntered += OnAreaEntered;
        AreaExited += OnAreaExited;
    }

    private void OnAreaEntered(Area2D area)
    {
        if (area is Hitbox hitbox && Faction.IsVulnerableTo(hitbox.Faction))
        {
            m_Hitboxes ??= new HashSet<Hitbox>();

            if (m_Hitboxes.Add(hitbox))
            {
                HitboxEntered?.Invoke(hitbox);
            }
        }
    }

    private void OnAreaExited(Area2D area)
    {
        if (area is Hitbox hitbox && Faction.IsVulnerableTo(hitbox.Faction) && (m_Hitboxes?.Remove(hitbox) ?? false))
        {
            HitboxExited?.Invoke(hitbox);
        }
    }
}
