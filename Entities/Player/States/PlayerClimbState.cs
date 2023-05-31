using Godot;

namespace Spaghetti.Godot;

public partial class PlayerClimbState : PlayerState
{
    private Vector2 m_Direction;

    public override void Enter<TState>(TState? previousState) where TState : class
    {
        Player.AnimationPlayer.Play(PlayerAnimation.ClimbIdle);
        Player.IsClimbing = true;
    }

    public override void Exit()
    {
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
        if (Player.Ladder == null)
        {
            Player.StateMachine.ChangeState<PlayerIdleState>();
            return;
        }

        m_Direction = Player.Input.GetInputDirection();

        if (m_Direction.Y != 0)
        {
            Player.AnimationPlayer.Play(Player.IsExitingLadder()
                ? PlayerAnimation.ClimbExit
                : PlayerAnimation.ClimbMove);

            Player.Velocity = new Vector2(0f, m_Direction.Y * Player.ClimbSpeed);
            var collision = Player.MoveAndSlide();

            if (m_Direction.Y > 0 && collision) // Moving Down
            {
                Player.StateMachine.ChangeState<PlayerIdleState>();
            }

            // if (m_Direction.Y < 0 && Player.IsAboveLadder()) // Moving Up
            // {
            //     Log.Debug(Player.IsAboveLadder().ToString());
            //
            //     Player.StateMachine.ChangeState<PlayerIdleState>();
            // }
        }
        else
        {
            Player.AnimationPlayer.Play(Player.IsExitingLadder()
                ? PlayerAnimation.ClimbExit
                : PlayerAnimation.ClimbIdle);
        }
    }
}
