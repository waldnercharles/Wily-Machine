namespace Spaghetti.Godot;

public partial class PlayerSpawnState : PlayerState
{
    public override void Enter<TState>(TState? previousState) where TState : class
    {
        Player.StateMachine.Locked = true;
        // TODO: Spawn Animation
        Player.IsInvincible = true;
    }

    public override void AnimationFinished(string animationName)
    {
        if (animationName == "Spawn")
        {
            Player.IsDead = false;
            Player.IsInvincible = false;
            Player.StateMachine.Locked = false;
            Player.StateMachine.ChangeState<PlayerIdleState>();
        }
    }
}
