using Godot;

namespace Spaghetti.Godot;

public partial class Player : CharacterBody2D
{
    public PlayerInput Input { get; set; } = new PlayerInput();

    [Export] public Sprite2D Sprite { get; set; } = null!;
    [Export] public WeaponStateMachine Weapons { get; set; } = null!;

    public Vector2 FacingDirection => Sprite.FlipH ? Vector2.Left : Vector2.Right;

    [Export] public CollisionShape2D CollisionShape { get; set; } = null!;
    [Export] public PlayerStateMachine StateMachine { get; set; } = null!;
    [Export] public AnimationPlayer AnimationPlayer { get; set; } = null!;

    public Ladder? Ladder { get; set; }
    public bool IsExitingLadder() => Ladder?.IsExitingLadder(this) ?? false;

    [Export] public bool SlideEnabled { get; set; }

    [Export] public float Gravity { get; set; } = 15f;
    [Export] public float MaxFallSpeed { get; set; } = 420f;

    [Export] public float WalkSpeed { get; set; } = 82.5f;
    [Export] public float StepSpeed { get; set; } = 60f;
    [Export] public float ClimbSpeed { get; set; } = 78f;
    [Export] public float JumpSpeed { get; set; } = 285f;
    [Export] public float SlideSpeed { get; set; } = 150f;

    [Export] public int MaxHealth { get; set; } = 28;

    [Export] public float ProjectileSpeed { get; set; } = 300.0f;
    [Export] public float ChargeDurationLevelOne { get; set; } = 0.4f;
    [Export] public float ChargeDurationLevelTwo { get; set; } = 1.4f;

    public bool IsStill { get; set; }
    public bool IsClimbing { get; set; }
    public bool IsSliding { get; set; }
    public bool IsStaggered { get; set; }
    public bool IsDead { get; set; }
    public bool IsInvincible { get; set; }

    public Vector2I WeaponPosition { get; set; }


    public override void _UnhandledInput(InputEvent inputEvent)
    {
        if (Input.Controller is PlayerInputController.None || StateMachine.State == null)
        {
            return;
        }

        if (inputEvent.IsActionPressed(PlayerInputAction.Jump))
        {
            StateMachine.State.HandleCommand(PlayerCommand.Jump);
        }
        else if (inputEvent.IsActionReleased(PlayerInputAction.Jump))
        {
            StateMachine.State.HandleCommand(PlayerCommand.JumpStop);
        }

        if (inputEvent.IsActionPressed(PlayerInputAction.Shoot))
        {
            StateMachine.State.HandleCommand(PlayerCommand.Shoot);
        }
    }

    public void StartClimbing(Vector2 ladderDistance)
    {
        MoveAndCollide(ladderDistance);
        StateMachine.ChangeState<PlayerClimbState>();
    }

    public void StopClimbing()
    {
        if (IsClimbing)
        {
            StateMachine.ChangeState<PlayerIdleState>();
        }
    }

    public void Shoot()
    {
        Weapons.State?.Shoot();
    }

    public void UpdateSpriteDirection(Vector2 direction)
    {
        Sprite.FlipH = direction.X switch
        {
            > 0 => false,
            < 0 => true,
            _ => Sprite.FlipH
        };

        Sprite.Offset =
            new Vector2(FacingDirection.X * Mathf.Abs(Sprite.Offset.X), Sprite.Offset.Y);
    }
}
