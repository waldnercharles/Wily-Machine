using Godot;

namespace WilyMachine;

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

    public override void _Ready()
    {
        // MaxSlides = 4;
        // Log.Trace(MaxSlides.ToString());
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        WasOnFloor = IsOnFloor;
        IsOnFloor = base.IsOnFloor();

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

        MoveAndSlide();
    }

    public virtual bool CanShoot() { return true; }
    public virtual void Shoot() { }

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
