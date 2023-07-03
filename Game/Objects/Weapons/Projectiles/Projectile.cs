using System;
using Godot;
using WilyMachine.Pooling;

namespace WilyMachine;

[SceneTree]
public partial class Projectile : Node2D, IPoolable<Projectile>, IFactionComponent
{
    public event Action<Projectile>? Released;

    public Sprite2D Sprite => _.Sprite;
    public VisibleOnScreenNotifier2D VisibleOnScreenNotifier => _.VisibleOnScreenNotifier;

    public Vector2 Direction { get; set; }

    public bool IsConsumed { get; set; }

    [Export] public Faction Faction { get; set; }

    public virtual void Initialize() { }

    public override void _Ready()
    {
        VisibleOnScreenNotifier.ScreenExited += ScreenExited;
    }

    public virtual void Reflect()
    {
        IsConsumed = true;
    }

    public virtual void Release()
    {
        IsConsumed = true;
        Released?.Invoke(this);
    }

    private void ScreenExited()
    {
        Release();
    }
}
