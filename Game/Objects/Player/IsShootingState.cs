using Godot;

namespace Spaghetti;

public sealed class IsShootingState : ShootingState
{
    // TODO: Base this on current weapon
    private const int COOLDOWN_FRAMES = 15;

    private int m_CooldownFrameCounter;
    public IsShootingState(Player player) : base(player) { }

    public override void Enter(State? previousState)
    {
        Player.IsShooting = true;
        m_CooldownFrameCounter = 0;

        var controller = Player.Controller;

        if (controller.ShouldMoveLeft())
        {
            Player.Direction = Vector2.Left;
        }
        else if (controller.ShouldMoveRight())
        {
            Player.Direction = Vector2.Right;
        }

        Player.ChooseSpriteAnimation();
        Player.Shoot();
    }

    public override StateChange Update(float delta)
    {
        m_CooldownFrameCounter++;

        var controller = Player.Controller;

        if (controller.ShouldShoot() && Player.CanShoot())
        {
            m_CooldownFrameCounter = 0;
            Player.Shoot();
        }

        if (m_CooldownFrameCounter > COOLDOWN_FRAMES)
        {
            Player.EnqueueShootingState<NotShootingState>();
            return StateChange.Next;
        }

        return StateChange.None;
    }
}
