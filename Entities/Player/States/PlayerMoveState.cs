using Godot;

namespace Spaghetti.Godot;

public partial class PlayerMoveState : PlayerOnGroundState
{
    [Export] public int TipToeFrameCountMax = 5;
    [Export] public int ShootFrameCountMax = 19;

    [Export] public Vector2I WeaponPosition = new Vector2I(17, 0);

    private int m_FrameCount;
    private int m_ShootFrameCount;
    private int m_TipToeFrameCount;

    public override void Enter<TState>(TState? previousState) where TState : class
    {
        Player.Velocity = Vector2.Zero;
        m_FrameCount = -1;
        m_ShootFrameCount = ShootFrameCountMax;
        m_TipToeFrameCount = Player.IsStill ? TipToeFrameCountMax : 0;

        Player.WeaponPosition = WeaponPosition;
    }

    public override void Exit()
    {
        Player.IsStill = false;
    }

    public override void HandleCommand(PlayerCommand command)
    {
        if (command is PlayerCommand.Shoot)
        {
            m_ShootFrameCount = -1;
            // TODO: Change Animation
            // TODO: Actually Shoot
        }

        if (command is PlayerCommand.WeaponPrevious or PlayerCommand.WeaponNext)
        {
            // TODO: Change Weapon
        }

        base.HandleCommand(command);
    }

    public override void Update(float delta)
    {
        m_FrameCount++;
        m_ShootFrameCount++;

        var direction = Player.Input.GetInputDirection();
        Player.UpdateSpriteDirection(direction);

        if (direction.X == 0)
        {
            StateMachine.ChangeState<PlayerIdleState>();
            return;
        }

        if (m_FrameCount == 0 && m_ShootFrameCount > ShootFrameCountMax)
        {
            Player.AnimationPlayer.Play(PlayerAnimation.TipToe);
        }

        Player.Velocity += new Vector2(0, Player.Gravity);

        if (m_FrameCount < 1 && m_TipToeFrameCount > 0)
        {
            Player.Velocity = new Vector2(Player.StepSpeed * direction.X, Player.Velocity.Y);
        }
        else if (m_FrameCount >= m_TipToeFrameCount)
        {
            Player.Velocity = new Vector2(Player.WalkSpeed * direction.X, Player.Velocity.Y);
        }
        else
        {
            Player.Velocity = new Vector2(0, Player.Velocity.Y);
        }

        Player.MoveAndSlide();

        if (m_FrameCount >= m_TipToeFrameCount && m_ShootFrameCount > ShootFrameCountMax)
        {
            if (Player.AnimationPlayer.CurrentAnimation != PlayerAnimation.Move)
            {
                Player.AnimationPlayer.Play(PlayerAnimation.Move);
            }
        }

        if (!Player.IsOnFloor())
        {
            StateMachine.ChangeState<PlayerJumpState>();
        }
    }
}
