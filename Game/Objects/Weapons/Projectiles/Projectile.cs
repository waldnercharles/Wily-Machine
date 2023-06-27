using Godot;

namespace Spaghetti;

[SceneTree]
public partial class Projectile : CharacterBody2D, IFactionComponent
{
    public Sprite2D Sprite => _.Sprite;
    public CollisionShape2D CollisionShape => _.CollisionShape;
    public VisibleOnScreenNotifier2D VisibleOnScreenNotifier => _.VisibleOnScreenNotifier;

    public Vector2 Direction { get; set; }

    public bool IsConsumed { get; protected set; }

    [Export] public Faction Faction { get; set; }

    public override void _Ready()
    {
        VisibleOnScreenNotifier.ScreenExited += ScreenExited;
    }

    public virtual void Reflect()
    {
        IsConsumed = true;
    }

    public new virtual void QueueFree()
    {
        IsConsumed = true;
        base.QueueFree();
    }

    private void ScreenExited()
    {
        QueueFree();
    }
}
