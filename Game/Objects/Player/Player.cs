using Godot;

namespace Spaghetti;

public sealed partial class Player : Actor
{
    [Export] public CollisionShape2D UprightCollisionShape { get; set; } = null!;
    [Export] public CollisionShape2D SlideCollisionShape { get; set; } = null!;

    [Export] public PlayerInputController Controller { get; set; } = null!;
    [Export] public Weapon Weapon { get; set; } = null!;

    [Export] public int Health { get; set; } = 1;

    public Ladder? Ladder { get; set; }

    public bool IsIdle;
    public bool IsWalking;
    public bool IsDecelerating;
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

    public bool IsStunned;

    public bool IsInvincible => RemainingInvincibilityFrames > 0;
    public bool IsBlinking => IsInvincible;

    public bool IsVulnerable => !IsInvincible && !IsStunned;

    [Export] public int JumpBufferFrames { get; set; } = 6;
    public ulong IsOnFloorTimestamp { get; set; }

    [Export] public int TipToeFrames { get; set; } = 7;
    [Export] public int SlideFrames { get; set; } = 26;

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
            if (IsOnFloorTimestamp == 0)
            {
                OnLand();
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
    }

    public override bool CanShoot()
    {
        return base.CanShoot() && !IsSliding;
    }

    public override void Shoot()
    {
        Weapon.Shoot();
    }

    public void StopShooting()
    {
        SetShootingState<NotShootingState>();
    }

    public bool CanSlide()
    {
        return !IsAirborn && !IsStunned && !TestSlideCollisionShape();
    }

    public bool CanJump()
    {
        return !IsAirborn || IsClimbing;
    }

    public void OnLand()
    {
        // SoundEffectPlayer.Play(SoundEffect.PlayerLand);
    }

    public void OnHit(int damage)
    {
        if (IsVulnerable)
        {
            // SoundEffectPlayer.Play(SoundEffect.PlayerHit);
            Stun();
            Health -= damage;

            if (Health <= 0)
            {
                Kill();
            }
        }
    }

    public void Stun()
    {
        if (!IsStunned)
        {
            SetTemporaryMovementState<StunMovementState>();
        }

        IsStunned = true;
    }

    public void Kill() { }

    public void ToggleSpriteDirection()
    {
        if (Sprite != null)
        {
            Sprite.FlipH = !Sprite.FlipH;
        }
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
