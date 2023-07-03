using System.Collections.Generic;
using System.Linq;
using Godot;

namespace WilyMachine;

[SceneTree]
public partial class MM1Met : Enemy
{
    public Area2D DetectionArea => _.DetectionArea;
    public Hurtbox Hurtbox => _.Hurtbox;

    public HashSet<Player>? DetectedPlayers;
    public Player? Target => DetectedPlayers?.FirstOrDefault();

    private enum State { Guarding, Shooting }
    private State m_State;

    private int m_FrameCounter;

    [Export] public int GuardFrames = 50;
    [Export] public int ShootDelayFrames = 17;
    [Export] public int ShootVulnerableFrames = 30;

    public Weapon Weapon => _.Weapon;

    public MM1Met()
    {
        m_State = State.Guarding;
    }

    public override void _Ready()
    {
        base._Ready();

        // TODO: Refactor so every Actor has a list of potential targets?
        DetectionArea.BodyEntered += DetectionAreaBodyEntered;
        DetectionArea.BodyExited += DetectionAreaBodyExited;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        if (m_State == State.Guarding)
        {
            if (m_FrameCounter >= GuardFrames && Target != null)
            {
                FaceTarget();

                m_State = State.Shooting;
                Sprite.Frame = 1;

                m_FrameCounter = 0;
            }
            else
            {
                m_FrameCounter++;
            }
        }
        else if (m_State == State.Shooting)
        {
            m_FrameCounter++;

            if (m_FrameCounter == ShootDelayFrames)
            {
                Shoot();
            }
            else if (m_FrameCounter == ShootVulnerableFrames)
            {
                m_State = State.Guarding;
                Sprite.Frame = 0;
                m_FrameCounter = 0;
            }
        }

        foreach (var hitbox in Hurtbox.Hitboxes)
        {
            HandleHitboxCollision(hitbox);
        }
    }

    public override void Shoot()
    {
        void ShootAtAngle(float angle)
        {
            if (Weapon.Shoot() is MM1MetProjectile projectile)
            {
                projectile.Angle = angle;
            }
        }

        ShootAtAngle(-45);
        ShootAtAngle(0);
        ShootAtAngle(45);
    }

    private void DetectionAreaBodyEntered(Node2D body)
    {
        if (body is Player player)
        {
            DetectedPlayers ??= new HashSet<Player>();
            DetectedPlayers.Add(player);
        }
    }

    private void DetectionAreaBodyExited(Node2D body)
    {
        if (body is Player player)
        {
            DetectedPlayers ??= new HashSet<Player>();
            DetectedPlayers.Remove(player);
        }
    }

    private void FaceTarget()
    {
        if (Target != null)
        {
            Direction = GlobalPosition > Target.GlobalPosition ? Vector2.Left : Vector2.Right;
        }
    }

    private void HandleHitboxCollision(Hitbox hitbox)
    {
        if (hitbox.Owner is Projectile projectile)
        {
            if (projectile.IsConsumed)
            {
                return;
            }

            if (m_State == State.Guarding)
            {
                projectile.Reflect();
            }
            else
            {
                TakeDamage(hitbox.Damage);
                projectile.QueueFree();
            }
        }
    }
}
