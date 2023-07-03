using Godot;

namespace WilyMachine;

public partial class MM1MetProjectile : Projectile
{
    [Export] public float ProjectileSpeed = 1.5f * 60f;

    public float Angle = 0;

    private Vector2 m_Velocity;

    public override void Initialize()
    {
        m_Velocity = Vector2.Zero;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (m_Velocity == Vector2.Zero)
        {
            var radians = Mathf.DegToRad(Angle);
            var velocity = new Vector2(Mathf.Cos(radians) * Direction.X, -Mathf.Sin(radians)) * ProjectileSpeed;

            m_Velocity = velocity;
        }

        GlobalPosition += m_Velocity * (float)delta;
    }
}
