using Godot;

namespace Spaghetti;

[SceneTree]
public abstract partial class Projectile : CharacterBody2D
{
    [Export] public int Damage { get; set; } = 1;
    [Export] public float ProjectileSpeed = 5f * 60f;
    
    public Vector2 Direction { get; set; }
    
    public VisibleOnScreenNotifier2D VisibleNotifier => _.VisibleOnScreenNotifier;
}

[SceneTree]
public partial class BusterProjectile : CharacterBody2D
{
    public Sprite2D Sprite => _.Sprite;
    public CollisionShape2D CollisionShape => _.CollisionShape;
    public AudioStreamPlayer ShootSoundEffect => _.ShootSoundEffect;
    public VisibleOnScreenNotifier2D VisibleNotifier => _.VisibleOnScreenNotifier;

    [Export] public int Damage { get; set; } = 1;
    [Export] public float ProjectileSpeed = 5f * 60f;

    public Vector2 Direction { get; set; }

    public override void _Ready()
    {
        VisibleNotifier.ScreenExited += ScreenExited;

        ShootSoundEffect.Play();

        if (Direction.X < 0)
        {
            Sprite.FlipH = true;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        MoveAndCollide(Direction.Normalized() * ProjectileSpeed * (float)delta);
    }

    public void ScreenExited()
    {
        QueueFree();
    }

    public new void QueueFree()
    {
        if (ShootSoundEffect.Playing)
        {
            Visible = false;
            CollisionShape.SetDeferred(nameof(CollisionShape.Disabled), true);
            ShootSoundEffect.Finished += base.QueueFree;
        }
        else
        {
            base.QueueFree();
        }
    }
}
