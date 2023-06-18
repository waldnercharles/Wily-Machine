﻿using Godot;

namespace Spaghetti;

public sealed class JumpMovementState : MovementState
{
    public static int JumpCount = 0;

    public JumpMovementState(Player player) : base(player) { }

    public override void Enter(State? previousState)
    {
        Player.IsAirborn = true;

        var controller = Player.Controller;

        if (controller.ShouldJump() && !Player.IsFalling)
        {
            Jump();
        }
        else if (!Player.IsOnFloor())
        {
            Player.IsFalling = true;
        }

        Player.ChooseSpriteAnimation();
    }

    public override void Exit()
    {
        Player.IsAirborn = false;
        Player.IsFalling = false;
        Player.IsJumping = false;
    }

    public override StateChange Update(float delta)
    {
        var controller = Player.Controller;
        var velocity = Player.Velocity;

        if (controller.ShouldMoveLeft())
        {
            Player.Direction = Vector2.Left;
            velocity.X = -Player.WalkVelocity.X;
        }
        else if (controller.ShouldMoveRight())
        {
            Player.Direction = Vector2.Right;
            velocity.X = Player.WalkVelocity.X;
        }
        else
        {
            velocity.X = 0;
        }

        if (Player.IsJumping && controller.ShouldStopJumping() &&
            Mathf.Sign(Player.Velocity.Y) == Mathf.Sign(Player.UpDirection.Y))
        {
            Player.IsJumping = false;
            Player.IsFalling = true;
            velocity.Y = 0f;
        }

        var isOnFloorBuffered = Time.GetTicksMsec() - Player.IsOnFloorTimestamp <
                                FrameTime.ToMilliseconds(Player.JumpBufferFrames);

        if (controller.ShouldJump() && isOnFloorBuffered)
        {
            Player.IsOnFloorTimestamp = 0;
            Player.IsFalling = false;


            if (JumpCount > 10)
            {
                Player.OnHit(1);
                JumpCount = 0;
            }
            else
            {
                Player.EnqueueMovementState<JumpMovementState>(); // Exit and re-enter
                JumpCount++;
            }

            return StateChange.Next;
        }

        if (Player.IsOnFloor())
        {
            if (controller.ShouldMoveLeft() || controller.ShouldMoveRight())
            {
                Player.IsFullAcceleration = true;
                Player.EnqueueMovementState<WalkMovementState>();
            }
            else
            {
                Player.EnqueueMovementState<IdleMovementState>();
            }

            return StateChange.Next;
        }

        Player.Velocity = velocity;

        return StateChange.None;
    }

    private void Jump()
    {
        Player.IsOnFloorTimestamp = 0;
        Player.Controller.ResetJumpBuffer();
        var velocity = Player.Velocity;

        if (Player.IsClimbing)
        {
            Player.IsClimbing = false;
            velocity.Y = 0f; // TODO: Re-evaluate? Should we jump at JumpVelocity / 2f?
        }
        else
        {
            velocity.Y = Player.JumpVelocity.Y;
        }

        Player.Velocity = velocity;
        Player.IsJumping = true;
    }
}
