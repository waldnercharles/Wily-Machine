using System;
using Godot;

namespace Spaghetti.Godot;

public partial class BusterState : WeaponState
{
    private Player m_Player =>
        Owner as Player ?? throw new Exception("BusterState must belong to a player");

    private PackedScene m_Projectile => ResourceLoader.Load<PackedScene>(
        "res://Entities/Player/Weapons/Projectiles/Buster.tscn");

    [Export] public int MaxProjectiles { get; set; } = 3;

    public override void Shoot()
    {
        var bullets = GetTree().GetNodesInGroup("BusterProjectiles");

        if (bullets.Count < MaxProjectiles)
        {
            var bullet = m_Projectile.Instantiate<BusterProjectile>();
            var weaponPosition = m_Player.WeaponPosition;
            weaponPosition.X = (int)(Mathf.Abs(weaponPosition.X) * m_Player.FacingDirection.X);

            bullet.Position = m_Player.GlobalPosition + weaponPosition;
            bullet.Direction = m_Player.FacingDirection;
            bullet.AddToGroup("BusterProjectiles");

            Owner.GetParent().AddChild(bullet);
        }
    }
}
