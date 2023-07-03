using Godot;
using WilyMachine.Pooling;

namespace WilyMachine;

public partial class Weapon : Node
{
    public Actor? Actor { get; private set; }

    [Export] public PackedScene? Projectile { get; set; }
    [Export] public int MaxProjectiles { get; set; }

    [Export] public AudioStreamPlayer2D? ShootSoundEffect { get; set; }

    public StringName? ProjectileGroup { get; private set; }

    private ObjectPool<Projectile>? m_ProjectilePool;

    public override void _Ready()
    {
        Actor = Owner as Actor;
        ProjectileGroup = $"{Owner.GetInstanceId().ToString()}.{Name}";

        if (Projectile != null)
        {
            m_ProjectilePool = ObjectPools.Get<Projectile>(Projectile);
        }
    }

    public virtual Projectile? Shoot()
    {
        if (m_ProjectilePool != null && Actor != null && CanShoot())
        {
            var bullet = m_ProjectilePool.Instantiate(Owner.GetParent());

            var weaponPosition = Actor.WeaponOffset;
            var direction = Actor.Direction;

            weaponPosition.X = (int)(Mathf.Abs(weaponPosition.X) * direction.X);

            bullet.Position = Actor.Position + weaponPosition;
            bullet.Direction = direction;
            bullet.Faction = Actor.Faction;
            bullet.IsConsumed = false;

            bullet.AddToGroup(ProjectileGroup);

            return bullet;
        }

        return null;
    }

    public bool CanShoot()
    {
        // TODO: Keep track of these ourselves? Godot heap-allocates an array every time this is called.
        var nodes = GetTree().GetNodesInGroup(ProjectileGroup);

        var activeProjectilesCount = 0;

        for (var i = 0; i < nodes.Count; i++)
        {
            if (activeProjectilesCount >= MaxProjectiles)
            {
                break;
            }

            if (nodes[i] is Projectile projectile && projectile.ProcessMode != ProcessModeEnum.Disabled)
            {
                activeProjectilesCount++;
            }
        }

        return m_ProjectilePool != null && Actor != null &&
               (activeProjectilesCount < MaxProjectiles || MaxProjectiles == 0);
    }
}
