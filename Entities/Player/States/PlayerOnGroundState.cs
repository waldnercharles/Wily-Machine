namespace Spaghetti.Godot;

public abstract partial class PlayerOnGroundState : PlayerState
{
    public override void HandleCommand(PlayerCommand command)
    {
        if (command == PlayerCommand.Slide && Player.SlideEnabled)
        {
            StateMachine.ChangeState<PlayerSlideState>();
        }
        else if (command == PlayerCommand.Jump)
        {
            StateMachine.ChangeState<PlayerJumpState>();
        }
        else
        {
            base.HandleCommand(command);
        }
    }
}
