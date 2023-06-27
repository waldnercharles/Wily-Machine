using Godot;

namespace Spaghetti;

public partial class Weapon : Node
{
    public Actor? Actor { get; private set; }

    [Export] public PackedScene? Projectile { get; set; }
    [Export] public int MaxProjectiles { get; set; }

    [Export] public AudioStreamPlayer2D? ShootSoundEffect { get; set; }

    public StringName? ProjectileGroup { get; private set; }

    public override void _Ready()
    {
        Actor = Owner as Actor;
        ProjectileGroup = $"{Owner.GetInstanceId().ToString()}.{Name}";
    }

    public virtual Projectile? Shoot()
    {
        var bullets = GetTree().GetNodesInGroup(ProjectileGroup);

        if (Projectile != null && Actor != null && (bullets.Count < MaxProjectiles || MaxProjectiles == 0))
        {
            var bullet = Projectile.Instantiate<Projectile>();
            var weaponPosition = Actor.WeaponOffset;
            var direction = Actor.Direction;

            weaponPosition.X = (int)(Mathf.Abs(weaponPosition.X) * direction.X);

            bullet.Position = Actor.Position + weaponPosition;
            bullet.Direction = direction;
            bullet.Faction = Actor.Faction;

            bullet.AddToGroup(ProjectileGroup);

            // TODO: This feels bad.
            Owner.GetParent().AddChild(bullet);

            return bullet;
        }

        return null;
    }
}
