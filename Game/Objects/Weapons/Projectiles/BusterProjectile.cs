using Godot;

namespace WilyMachine;

[SceneTree]
public partial class BusterProjectile : Projectile
{
    public AudioStreamPlayer ShootSoundEffect => _.ShootSoundEffect;

    [Export] public float ProjectileSpeed = 5f * 60f;

    public override void Initialize()
    {
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

    public override void Release()
    {
        IsConsumed = true;

        if (ShootSoundEffect.Playing)
        {
            Visible = false;
            ShootSoundEffect.Finished += base.Release;
        }
        else
        {
            base.Release();
        }
    }
}
