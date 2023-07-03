namespace WilyMachine;

public abstract class MovementState : State
{
    protected readonly Player Player;

    protected MovementState(Player player)
    {
        Player = player;
    }
}
