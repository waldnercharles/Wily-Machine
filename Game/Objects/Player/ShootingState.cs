namespace WilyMachine;

public abstract class ShootingState : State
{
    protected readonly Player Player;

    protected ShootingState(Player player)
    {
        Player = player;
    }
}
