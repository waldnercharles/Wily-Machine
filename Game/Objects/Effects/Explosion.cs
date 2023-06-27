using Godot;

namespace Spaghetti;

public enum ExplosionType { Small, Big };

[SceneTree]
public partial class Explosion : Effect
{
    [Export] public int ItemDrop { get; set; } = -1;

    public void Play(ExplosionType explosionType)
    {
        if (explosionType == ExplosionType.Small)
        {
            _.AnimationPlayer.Play("SmallExplosion");
        }
        else if (explosionType == ExplosionType.Big)
        {
            _.AnimationPlayer.Play("BigExplosion");
        }
    }

    private void ExplosionFinished(StringName animName)
    {
        DropItem();
        QueueFree();
    }

    private void DropItem()
    {
        // TODO: Drop tables
    }
}
