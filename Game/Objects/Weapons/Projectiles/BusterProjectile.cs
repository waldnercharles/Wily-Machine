using Godot;

namespace Spaghetti;

[SceneTree]
public partial class BusterProjectile : Projectile
{
    public AudioStreamPlayer ShootSoundEffect => _.ShootSoundEffect;

    [Export] public float ProjectileSpeed = 5f * 60f;

    public override void _Ready()
    {
        base._Ready();
        ShootSoundEffect.Play();

        if (Direction.X < 0)
        {
            Sprite.FlipH = true;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        GlobalPosition += ProjectileSpeed * Direction.Normalized() * (float)delta;
    }

    public override void Reflect()
    {
        base.Reflect();

        // TODO: Play sound effect

        Direction = new Vector2(-Direction.X, 0) + Vector2.Up;
        Sprite.FlipH = !Sprite.FlipH;
    }

    public override void QueueFree()
    {
        IsConsumed = true;

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
