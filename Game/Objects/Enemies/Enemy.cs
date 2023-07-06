using Godot;

namespace WilyMachine;

[SceneTree]
public partial class Enemy : Actor
{
    [Export] public int Health { get; set; } = 1;

    public Enemy()
    {
        IsAgeless = true;
        IsAffectedByGravity = true;
        IsPhasing = false;

        Faction = Faction.Enemy;
        DeathType = ActorDeathType.SmallExplosion;
    }

    public override void _Ready()
    {
        base._Ready();

        AddToGroup("Enemies");
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
    }

    public void TakeDamage(int damage)
    {
        // TODO: Sound Effect?
        Health -= damage;

        if (Health <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        IsDead = true;

        var explosion = Explosion?.Instantiate<Explosion>();

        if (explosion != null)
        {
            GetParent().AddChild(explosion);
            explosion.Position = Position;

            explosion.Play(DeathType == ActorDeathType.LargeExplosion ? ExplosionType.Big : ExplosionType.Small);
        }

        QueueFree();
    }
}
