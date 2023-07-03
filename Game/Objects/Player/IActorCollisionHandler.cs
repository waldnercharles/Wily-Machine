using Godot;

namespace WilyMachine;

public interface IActorCollisionHandler
{
    void OnCollision(CollisionObject2D collisionObject);
}
