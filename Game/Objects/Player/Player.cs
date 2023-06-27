using Godot;
using Spaghetti.Messaging;

namespace Spaghetti;

[SceneTree]
public sealed partial class Player : Actor
{
    public PlayerInputController Controller => _.Controller;

    public CollisionShape2D UprightCollisionShape => _.UprightCollisionShape;
    public CollisionShape2D SlideCollisionShape => _.SlideCollisionShape;

    public Hurtbox Hurtbox => _.Hurtbox;

    public CollisionShape2D HurtboxUprightCollisionShape => _.Hurtbox.UprightCollisionShape;
    public CollisionShape2D HurtboxSlideCollisionShape => _.Hurtbox.SlideCollisionShape;

    public AudioStreamPlayer2D LandSoundEffect => _.LandSoundEffect;
    public AudioStreamPlayer2D HitSoundEffect => _.HitSoundEffect;

    [Export] public int Health { get; set; } = 1;
    [Export] public Weapon? Weapon { get; set; }

    public Ladder? Ladder { get; set; }

    public bool IsIdle { get; set; }
    public bool IsWalking { get; set; }
    public bool IsDecelerating { get; set; }
    public bool IsSliding { get; set; }
    public bool IsInTunnel { get; set; }
    public bool IsJumping { get; set; }
    public bool IsFalling { get; set; }
    public bool IsAirborn { get; set; }
    public bool IsClimbing { get; set; }
    public bool IsTouchingLadder { get; set; }
    public bool IsTouchingLadderTop { get; set; }
    public bool IsFullAcceleration { get; set; }
    public bool IsShooting { get; set; }
    public bool IsTeleporting { get; set; }
    public bool IsMorphing { get; set; }

    public bool IsStunned { get; set; }

    public bool IsInvincible => RemainingInvincibilityFrames > 0;
    public bool IsBlinking => IsInvincible;

    public bool IsVulnerable => !IsInvincible && !IsStunned;

    [Export] public int JumpBufferFrames { get; set; } = 6;
    public ulong IsOnFloorTimestamp { get; set; }

    [Export] public int TipToeFrames { get; set; } = 7;
    [Export] public int SlideFrames { get; set; } = 26;
    public int SlideFrameCounter { get; set; }

    [Export] public int StunFrames { get; set; } = 32;
    [Export] public int StunInvincibilityFrames { get; set; } = 122;

    public int RemainingInvincibilityFrames { get; set; }

    [Export] public Vector2 WalkVelocity { get; set; } = new Vector2(1.3f, 0f) * 60f;
    [Export] public Vector2 TipToeVelocity { get; set; } = new Vector2(1f, 0f) * 60f;
    [Export] public Vector2 ClimbVelocity { get; set; } = new Vector2(1.3f, 1.3f) * 60f;
    [Export] public Vector2 JumpVelocity { get; set; } = -new Vector2(0f, 4.75f + 0.25f * 2) * 60;
    [Export] public Vector2 SlideVelocity { get; set; } = new Vector2(2.5f, 0f) * 60;
    [Export] public Vector2 KnockbackVelocity { get; set; } = new Vector2(-0.5f, 0) * 60;

    public Player()
    {
        Faction = Faction.Player;
        DeathType = ActorDeathType.PlayerExplosion;

        IsAffectedByGravity = true;

        m_MovementStateMachine.AddState(new IdleMovementState(this));
        m_MovementStateMachine.AddState(new WalkMovementState(this));
        m_MovementStateMachine.AddState(new JumpMovementState(this));
        m_MovementStateMachine.AddState(new SlideMovementState(this));
        m_MovementStateMachine.AddState(new ClimbMovementState(this));
        m_MovementStateMachine.AddState(new StunMovementState(this));

        m_ShootingStateMachine.AddState(new NotShootingState(this));
        m_ShootingStateMachine.AddState(new IsShootingState(this));

        MessageBus.Subscribe((in PlayerCamera.BeforeRoomTransition _) => BeforeRoomTransition());
        MessageBus.Subscribe((in PlayerCamera.AfterRoomTransition _) => AfterRoomTransition());
    }

    public override void _Ready()
    {
        SetMovementState<IdleMovementState>();
        SetShootingState<NotShootingState>();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (IsOnFloor())
        {
            if (IsAirborn && !IsStunned)
            {
                OnLanded();
            }

            IsOnFloorTimestamp = Time.GetTicksMsec();
        }

        if (RemainingInvincibilityFrames > 0)
        {
            RemainingInvincibilityFrames--;
        }

        ChooseEffectAnimation();

        m_MovementStateMachine.Update(delta);
        m_ShootingStateMachine.Update(delta);

        base._PhysicsProcess(delta);

        foreach (var hitbox in Hurtbox.Hitboxes)
        {
            TakeDamage(hitbox.Damage);
        }
    }

    public override bool CanShoot()
    {
        return base.CanShoot() && !IsSliding;
    }

    public override void Shoot()
    {
        Weapon?.Shoot();
    }

    public void StopShooting()
    {
        SetShootingState<NotShootingState>();
    }

    public bool CanSlide()
    {
        return !IsAirborn && !IsStunned && !TestSlideCollisionShape();
    }

    public void StopSliding()
    {
        SlideFrameCounter = SlideFrames;
    }

    public bool CanJump()
    {
        return !IsAirborn || IsClimbing;
    }

    public void OnLanded()
    {
        LandSoundEffect.Play();
    }

    public void TakeDamage(int damage)
    {
        if (IsVulnerable)
        {
            HitSoundEffect.Play();

            if (!IsStunned)
            {
                SetTemporaryMovementState<StunMovementState>();
            }

            IsStunned = true;
            Health -= damage;

            if (Health <= 0)
            {
                Kill();
            }
        }
    }

    public void BeforeRoomTransition()
    {
        SpriteAnimationPlayer.ProcessMode = ProcessModeEnum.WhenPaused;

        if (IsClimbing)
        {
            ChangeSpriteAnimation(PlayerAnimation.ClimbMove);
        }
        else if (IsIdle)
        {
            SetMovementState<WalkMovementState>();
            ChangeSpriteAnimation(PlayerAnimation.Move);
        }
    }

    public void AfterRoomTransition()
    {
        SpriteAnimationPlayer.ProcessMode = ProcessModeEnum.Inherit;

        if (!IsClimbing && !IsSliding && !IsAirborn)
        {
            SetMovementState<IdleMovementState>();
        }
    }

    public void Kill() { }

    public void ToggleSpriteDirection()
    {
        Sprite.FlipH = !Sprite.FlipH;
    }

    public bool TestUprightCollisionShape()
    {
        var uprightDisabled = UprightCollisionShape.Disabled;
        var slideDisabled = SlideCollisionShape.Disabled;

        UprightCollisionShape.Disabled = false;
        SlideCollisionShape.Disabled = true;

        var collision = MoveAndCollide(Vector2.Zero, true);

        UprightCollisionShape.Disabled = uprightDisabled;
        SlideCollisionShape.Disabled = slideDisabled;

        return collision != null;
    }

    public bool TestSlideCollisionShape()
    {
        var uprightDisabled = UprightCollisionShape.Disabled;
        var slideDisabled = SlideCollisionShape.Disabled;

        UprightCollisionShape.Disabled = true;
        SlideCollisionShape.Disabled = false;

        var collision = MoveAndCollide(SlideVelocity / 60.0f * Direction.X, true);

        UprightCollisionShape.Disabled = uprightDisabled;
        SlideCollisionShape.Disabled = slideDisabled;

        if (collision != null)
        {
            var normal = collision.GetNormal();
            var angle = collision.GetAngle();

            return Mathf.Sign(normal.Y) != Mathf.Sign(UpDirection.Y) || angle > FloorMaxAngle;
        }

        return false;
    }
}
