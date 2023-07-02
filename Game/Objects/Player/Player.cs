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

    [Export] public int Health = 1;
    [Export] public Weapon? Weapon;

    public Ladder? Ladder;

    public bool IsIdle;
    public bool IsWalking;
    public bool IsSliding;
    public bool IsInTunnel;
    public bool IsJumping;
    public bool IsFalling;
    public bool IsAirborn;
    public bool IsClimbing;
    public bool IsTouchingLadder;
    public bool IsTouchingLadderTop;
    public bool IsFullAcceleration;
    public bool IsShooting;
    public bool IsTeleporting;
    public bool IsMorphing;

    public bool IsInvincible => RemainingInvincibilityFrames > 0;
    public bool IsBlinking => RemainingInvincibilityFrames > 0;

    public bool IsStunned;

    public bool IsVulnerable => !IsInvincible;

    [Export] public int JumpBufferFrames = 6;
    public ulong IsOnFloorTimestamp;

    [Export] public int TipToeFrames = 7;
    [Export] public int SlideFrames = 26;
    public int RemainingSlideFrames;

    [Export] public int StunFrames = 32;
    [Export] public int StunInvincibilityFrames = 122;

    public int RemainingInvincibilityFrames;
    public int RemainingHitEffectFrames;

    [Export] public Vector2 WalkVelocity = new Vector2(1.3f, 0f) * 60f;
    [Export] public Vector2 TipToeVelocity = new Vector2(1f, 0f) * 60f;
    [Export] public Vector2 ClimbVelocity = new Vector2(1.3f, 1.3f) * 60f;
    [Export] public Vector2 JumpVelocity = -new Vector2(0f, 4.75f + 0.25f * 2) * 60;
    [Export] public Vector2 SlideVelocity = new Vector2(2.5f, 0f) * 60;
    [Export] public Vector2 KnockbackVelocity = new Vector2(-0.5f, 0) * 60;

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
        base._Ready();

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

        if (RemainingHitEffectFrames > 0)
        {
            RemainingHitEffectFrames--;
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
        return base.CanShoot() && (Weapon?.CanShoot() ?? false) && !IsSliding;
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
        RemainingSlideFrames = 0;
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

            SetMovementState<StunMovementState>();

            RemainingInvincibilityFrames = StunInvincibilityFrames;
            RemainingHitEffectFrames = StunFrames;

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
