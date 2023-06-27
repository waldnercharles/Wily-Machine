using Godot;

namespace Spaghetti;

public sealed class StunMovementState : MovementState
{
    private int m_StunFrameCounter = 0;

    public StunMovementState(Player player) : base(player) { }

    public override void Enter(State? previousState)
    {
        m_StunFrameCounter = 0;

        Player.IsStunned = true;
        // Player.IsInvincible = true;

        Player.RemainingInvincibilityFrames = Player.StunInvincibilityFrames;

        Player.IsAffectedByGravity = true;
        Player.StopShooting();
        Player.ChooseSpriteAnimation();


        var velocity = Player.Velocity;
        velocity.X = Player.KnockbackVelocity.X * Player.Direction.X;

        if (Player.IsSliding)
        {
            Player.StopSliding();
        }

        if (Player.IsClimbing)
        {
            velocity.X = 0;
        }

        velocity.Y = velocity.Y * Player.GravityDirection <= 0 ? -1.5f * Player.GravityDirection : 0f;

        Player.Velocity = velocity;

        // TODO: Emit signal
    }

    public override void Exit()
    {
        Player.IsStunned = false;
        Player.Velocity = new Vector2(0, Player.Velocity.Y);
    }

    public override StateChange Update(float delta)
    {
        m_StunFrameCounter++;

        if (m_StunFrameCounter > Player.StunFrames)
        {
            return StateChange.Next;
        }

        return StateChange.None;
    }
}
