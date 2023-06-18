using Godot;

namespace Spaghetti;

public partial class BusterProjectile : CharacterBody2D
{
    [Export] public Sprite2D Sprite { get; set; } = null!;
    [Export] public CollisionShape2D CollisionShape { get; set; } = null!;
    [Export] public AudioStreamPlayer ShootSoundEffect { get; set; } = null!;
    [Export] public VisibleOnScreenNotifier2D VisibleNotifier { get; set; } = null!;

    [Export] public int Damage { get; set; } = 1;
    [Export] public float ProjectileSpeed = 10f;

    public Vector2 Direction { get; set; }

    public override void _Ready()
    {
        Log.Assert(VisibleNotifier != null);
        VisibleNotifier.ScreenExited += ScreenExited;

        ShootSoundEffect.Play();

        if (Direction.X < 0)
        {
            Log.Assert(Sprite != null);
            Sprite.FlipH = true;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        MoveAndCollide(Direction.Normalized() * ProjectileSpeed);
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
