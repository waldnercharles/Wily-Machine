using Godot;

namespace Spaghetti;

public partial class DamageArea : StaticBody2D, IActorCollisionHandler
{
    [Export] public int Damage { get; set; } = 1;

    public void OnCollision(CollisionObject2D collisionObject)
    {
        if (collisionObject is Player player)
        {
            player.OnHit(Damage);
        }
    }
}
