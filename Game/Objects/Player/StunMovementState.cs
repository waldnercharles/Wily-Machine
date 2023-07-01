using Godot;

namespace Spaghetti;

public sealed class StunMovementState : MovementState
{
    private int m_RemainingStunFrames;

    public StunMovementState(Player player) : base(player) { }

    public override void Enter(State? previousState)
    {
        m_RemainingStunFrames = Player.StunFrames;
        Player.IsStunned = true;

        Player.IsAffectedByGravity = true;
        Player.StopShooting();
        Player.ChooseSpriteAnimation();


        var velocity = Player.Velocity;
        velocity.X = Player.KnockbackVelocity.X * Player.Direction.X;

        if (Player.IsSliding)
        {
            Player.StopSliding();
            Player.IsSliding = false;
        }

        if (Player.IsClimbing)
        {
            velocity.X = 0;
            Player.IsClimbing = false;
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
        m_RemainingStunFrames--;

        if (m_RemainingStunFrames <= 0)
        {
            if (Player.IsOnFloor())
            {
                Player.SetNextMovementState<WalkMovementState>();
            }
            else
            {
                Player.SetNextMovementState<JumpMovementState>();
            }

            return StateChange.Next;
        }

        return StateChange.None;
    }
}
