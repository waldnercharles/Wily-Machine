namespace Spaghetti;

public abstract class ShootingState : State
{
    protected readonly Player Player;

    protected ShootingState(Player player)
    {
        Player = player;
    }
}
