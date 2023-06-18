using Godot;

namespace Spaghetti;

public abstract partial class Actor : CharacterBody2D
{
    private const float DEFAULT_AGE_MILLISECONDS = 0f;
    private const float DEFAULT_MAX_AGE_MILLISECONDS = 10f;

    private const float MIN_X_VELOCITY = -MAX_X_VELOCITY;
    private const float MAX_X_VELOCITY = 960f;

    private const float MIN_Y_VELOCITY = -960f;
    private const float MAX_Y_VELOCITY = 420f;

    [Node] public Sprite2D? Sprite { get; set; }
    [Node] public AnimationPlayer SpriteAnimationPlayer { get; set; } = null!;

    [Node] public Sprite2D Effect { get; set; } = null!;
    [Node] public AnimationPlayer EffectAnimationPlayer { get; set; } = null!;

    [Export] public CollisionShape2D CollisionShape { get; set; } = null!;

    [Export] public Faction Faction { get; set; }
    [Export] public ActorDeathType DeathType { get; set; }

    public int WeaponId { get; set; }
    public int DamageId { get; set; }

    public bool IsObstacle { get; set; }
    public bool IsShielded { get; set; }
    public bool IsAgeless { get; set; }
    public bool IsPhasing { get; set; }

    [Export] public bool IsAffectedByGravity { get; set; }
    [Export] public float Gravity { get; set; } = 0.25f * 60;
    public float GravityDirection => -UpDirection.Y;

    public float Age { get; protected set; }
    public float MaxAge { get; set; }

    [Export] public Vector2 WeaponOffset { get; set; }

    public bool IsUsingPalette { get; set; }
    public int PaletteIndex { get; set; }

    private Vector2 m_Direction = Vector2.Right;

    [Export] public Vector2 Direction
    {
        get => m_Direction;
        set
        {
            m_Direction = value;

            if (Sprite != null)
            {
                Sprite.FlipH = Direction == Vector2.Left || Direction != Vector2.Right && Sprite.FlipH;

                Sprite.Offset = new Vector2
                (
                    Mathf.Abs(Sprite.Offset.X) * Direction.X,
                    Sprite.Offset.Y
                );

                if (CollisionShape != null)
                {
                    CollisionShape.Position = new Vector2
                    (
                        Mathf.Abs(CollisionShape.Position.X) * Direction.X,
                        CollisionShape.Position.Y
                    );
                }
            }
        }
    }

    public override void _Ready()
    {
        RegisterNodes();
    }

    public override void _PhysicsProcess(double delta)
    {
        var velocity = Velocity;

        if (IsAffectedByGravity)
        {
            velocity.Y += Gravity * GravityDirection;
            // TODO: Underwater?
        }

        velocity.X = Mathf.Clamp(velocity.X, MIN_X_VELOCITY, MAX_X_VELOCITY);
        velocity.Y = Mathf.Clamp(velocity.Y, MIN_Y_VELOCITY, MAX_Y_VELOCITY);

        if (!IsAgeless)
        {
            Age += (float)delta;

            if (Age >= MaxAge)
            {
                // TODO: Death
            }
        }

        Velocity = velocity;

        if (MoveAndSlide())
        {
            var collisionCount = GetSlideCollisionCount();

            for (var i = 0; i < collisionCount; i++)
            {
                var collision = GetSlideCollision(i);

                if (collision.GetCollider() is IActorCollisionHandler collisionHandler)
                {
                    collisionHandler.OnCollision(this);
                }
            }
        }

        base._PhysicsProcess(delta);
    }

    public virtual bool CanShoot() { return true; }
    public virtual void Shoot() { }

    public void ChangeSpriteAnimation(string animationName)
    {
        Log.Assert(SpriteAnimationPlayer != null);
        SpriteAnimationPlayer.Play(animationName);
    }

    public void PauseSpriteAnimation()
    {
        Log.Assert(SpriteAnimationPlayer != null);
        SpriteAnimationPlayer.Pause();
    }

    public void ChangeEffectAnimation(string animationName)
    {
        Log.Assert(EffectAnimationPlayer != null);
        EffectAnimationPlayer.Play(animationName);
    }

    public void PauseEffectAnimation()
    {
        Log.Assert(EffectAnimationPlayer != null);
        EffectAnimationPlayer.Pause();
    }
}
