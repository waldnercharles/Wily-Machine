using Godot;

namespace Spaghetti;

public interface IActorCollisionHandler
{
    void OnCollision(CollisionObject2D collisionObject);
}
