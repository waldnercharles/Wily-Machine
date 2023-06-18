namespace Spaghetti;

public sealed class NotShootingState : ShootingState
{
    public NotShootingState(Player player) : base(player) { }

    public override void Enter(State? previousState)
    {
        Player.IsShooting = false;
        Player.ChooseSpriteAnimation();
    }

    public override StateChange Update(float delta)
    {
        var controller = Player.Controller;

        if (controller.ShouldShoot() && Player.CanShoot())
        {
            Player.EnqueueShootingState<IsShootingState>();
            return StateChange.Next;
        }

        return StateChange.None;
    }
}
