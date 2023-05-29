using Godot;

namespace Spaghetti.Godot;

public partial class PlayerClimbState : PlayerState
{
    private Vector2 m_Direction;

    private bool m_IsMovingAnimation;

    public override void Enter<TState>(TState? previousState) where TState : class
    {
        // TODO: climb_idle
        Player.IsClimbing = true;
    }

    public override void Exit()
    {
        m_IsMovingAnimation = false;
        Player.IsClimbing = false;
    }

    public override void HandleCommand(PlayerCommand command)
    {
        if (command is PlayerCommand.Shoot)
        {
            StateMachine.ChangeState<PlayerClimbShootState>();
        }
        else if (command is PlayerCommand.DropDown)
        {
            StateMachine.ChangeState<PlayerJumpState>();
        }
        else if (command is PlayerCommand.WeaponPrevious or PlayerCommand.WeaponNext)
        {
            // TODO: Weapon Swapping
        }
    }

    public override void Update(float delta)
    {
        m_Direction = Player.Input.GetInputDirection();

        if (m_Direction.Y != 0)
        {
            if (Player.IsExitingLadder())
            {
                // TODO: Climb Exit Animation
            }
            else if (!m_IsMovingAnimation)
            {
                m_IsMovingAnimation = true;
                // TODO: Climb Move Animation
            }

            if (m_Direction.Y > 0 &&
                Player.TestMove(Player.Transform, new Vector2(0, m_Direction.Y) * delta))
            {
                Player.StateMachine.ChangeState<PlayerIdleState>();
            }
            else
            {
                Player.Velocity = new Vector2(0f, m_Direction.Y * Player.ClimbSpeed);
                Player.MoveAndSlide();
            }
        }
        else
        {
            m_IsMovingAnimation = false;

            if (Player.IsExitingLadder())
            {
                // TODO: Climb Exit Animation
            }
            else
            {
                // TODO: Climb Idle Animation
            }
        }
    }
}
