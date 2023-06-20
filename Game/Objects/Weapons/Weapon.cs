using Godot;

namespace Spaghetti;

public abstract partial class Weapon : Node
{
    public Actor? Actor { get; private set; }

    [Export] public PackedScene? Projectile { get; set; }
    [Export] public int MaxProjectiles { get; set; }

    [Export] public AudioStreamPlayer2D? ShootSoundEffect { get; set; }

    public StringName? ProjectileGroup { get; private set; }

    public override void _Ready()
    {
        Actor = Owner as Actor;
        Log.Assert(Actor != null);

        ProjectileGroup = $"{Owner.GetInstanceId().ToString()}.{Name}";
    }

    public virtual void Shoot()
    {
        var bullets = GetTree().GetNodesInGroup(ProjectileGroup);

        if (Projectile != null && Actor != null && (bullets.Count < MaxProjectiles || MaxProjectiles == 0))
        {
            var bullet = Projectile.Instantiate<BusterProjectile>();
            var weaponPosition = Actor.WeaponOffset;
            var direction = Actor.Direction;

            weaponPosition.X = (int)(Mathf.Abs(weaponPosition.X) * direction.X);

            bullet.Position = Actor.GlobalPosition + weaponPosition;
            bullet.Direction = direction;

            bullet.AddToGroup(ProjectileGroup);

            Owner.GetParent().AddChild(bullet);
        }
    }
}
