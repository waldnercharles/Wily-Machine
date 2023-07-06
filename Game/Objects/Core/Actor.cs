using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace WilyMachine;

public static class HashSet
{
    private static class EmptyHashSet<T>
    {
#pragma warning disable CA1825 // this is the implementation of Array.Empty<T>()
        internal static readonly HashSet<T> Value = new HashSet<T>();
#pragma warning restore CA1825
    }

    public static HashSet<T> Empty<T>() => EmptyHashSet<T>.Value;
}

[SceneTree]
public partial class Actor : CharacterBody2D, IFactionComponent
{
    private const float DEFAULT_AGE_MILLISECONDS = 0f;
    private const float DEFAULT_MAX_AGE_MILLISECONDS = 10f;

    private const float MIN_X_VELOCITY = -MAX_X_VELOCITY;
    private const float MAX_X_VELOCITY = 960f;

    private const float MIN_Y_VELOCITY = -960f;
    private const float MAX_Y_VELOCITY = 420f;

    public Sprite2D Sprite => _.Sprite;
    public AnimationPlayer SpriteAnimationPlayer => _.SpriteAnimationPlayer;

    public Sprite2D Effect => _.Effect;
    public AnimationPlayer EffectAnimationPlayer => _.EffectAnimationPlayer;

    [Export] public PackedScene? Explosion { get; set; }
    [Export] public CollisionShape2D? CurrentCollisionShape { get; set; }

    [Export] public Faction Faction { get; set; }
    [Export] public ActorDeathType DeathType { get; set; }

    public int WeaponId { get; set; }
    public int DamageId { get; set; }

    public bool IsObstacle { get; set; }
    public bool IsShielded { get; set; }
    public bool IsAgeless { get; set; }
    public bool IsPhasing { get; set; }

    public bool IsDead { get; set; }

    public new bool IsOnFloor { get; private set; }
    public bool WasOnFloor { get; private set; }

    [Export] public bool IsAffectedByGravity { get; set; }
    [Export] public float Gravity { get; set; } = 0.25f * 60;

    public float GravityDirection => -UpDirection.Y;

    public float Age { get; protected set; }
    public float MaxAge { get; set; }

    [Export] public Vector2 WeaponOffset { get; set; }

    public bool IsUsingPalette { get; set; }
    public int PaletteIndex { get; set; }

    private HashSet<ConstantForce>? m_Forces;

    private Vector2 m_Direction = Vector2.Right;

    [Export] public Vector2 Direction
    {
        get => m_Direction;
        set
        {
            m_Direction = value;

            Sprite.FlipH = Direction == Vector2.Left || Direction != Vector2.Right && Sprite.FlipH;

            Sprite.Offset = new Vector2
            (
                Mathf.Abs(Sprite.Offset.X) * Direction.X,
                Sprite.Offset.Y
            );

            if (CurrentCollisionShape != null)
            {
                CurrentCollisionShape.Position = new Vector2
                (
                    Mathf.Abs(CurrentCollisionShape.Position.X) * Direction.X,
                    CurrentCollisionShape.Position.Y
                );
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        var actorVelocity = Velocity;

        if (IsAffectedByGravity && !IsOnFloor)
        {
            actorVelocity.Y += Gravity * GravityDirection;
            // TODO: Underwater?
        }

        var constantLinearVelocity = Vector2.Zero;

        foreach (var force in m_Forces ?? Enumerable.Empty<ConstantForce>())
        {
            if (force.Enabled)
            {
                constantLinearVelocity += force.Velocity;
            }
        }

        if (!IsAgeless)
        {
            Age += (float)delta;

            if (Age >= MaxAge)
            {
                // TODO: Death
            }
        }

        constantLinearVelocity.X = Mathf.Clamp(constantLinearVelocity.X, MIN_X_VELOCITY, MAX_X_VELOCITY);
        constantLinearVelocity.Y = Mathf.Clamp(constantLinearVelocity.Y, MIN_Y_VELOCITY, MAX_Y_VELOCITY);
        MoveAndCollide(constantLinearVelocity * (float)delta);

        actorVelocity.X = Mathf.Clamp(actorVelocity.X, MIN_X_VELOCITY, MAX_X_VELOCITY);
        actorVelocity.Y = Mathf.Clamp(actorVelocity.Y, MIN_Y_VELOCITY, MAX_Y_VELOCITY);
        Velocity = actorVelocity;
        MoveAndSlide();

        WasOnFloor = IsOnFloor;
        IsOnFloor = base.IsOnFloor();
    }

    public virtual bool CanShoot() { return true; }
    public virtual void Shoot() { }

    public void AddConstanstForce(ConstantForce force)
    {
        m_Forces ??= new HashSet<ConstantForce>();
        m_Forces.Add(force);
    }

    public void RemoveConstantForce(ConstantForce force)
    {
        m_Forces?.Remove(force);
    }

    public void ChangeSpriteAnimation(string animationName)
    {
        SpriteAnimationPlayer.Play(animationName);
    }

    public void PauseSpriteAnimation()
    {
        SpriteAnimationPlayer.Pause();
    }

    public void ChangeEffectAnimation(string animationName)
    {
        EffectAnimationPlayer.Play(animationName);
    }

    public void PauseEffectAnimation()
    {
        EffectAnimationPlayer.Pause();
    }
}
