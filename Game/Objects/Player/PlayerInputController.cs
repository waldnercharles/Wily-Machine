namespace WilyMachine;

public sealed partial class PlayerInputController : DeviceInput, IPlayerController
{
    public Player? Player => Owner as Player;

    public bool ShouldMoveUp()
    {
        return IsActionPressed(PlayerAction.Up);
    }

    public bool ShouldMoveDown()
    {
        return IsActionPressed(PlayerAction.Down);
    }

    public bool ShouldMoveLeft()
    {
        return IsActionPressed(PlayerAction.Left);
    }

    public bool ShouldMoveRight()
    {
        return IsActionPressed(PlayerAction.Right);
    }

    public bool ShouldJump()
    {
        return IsActionBuffered(PlayerAction.Jump, FrameTime.ToMilliseconds(Player?.JumpBufferFrames ?? 0));
    }

    public bool ShouldStopJumping()
    {
        return !IsActionPressed(PlayerAction.Jump);
    }

    public bool ShouldShoot()
    {
        return IsActionJustPressed(PlayerAction.Shoot);
    }

    public bool ShouldChargeWeapon()
    {
        return IsActionHeld(PlayerAction.Shoot);
    }

    public bool ShouldSlide()
    {
        return IsActionPressed(PlayerAction.Down) &&
               IsActionBuffered(PlayerAction.Jump, FrameTime.ToMilliseconds(Player?.JumpBufferFrames ?? 0));
    }

    public bool ShouldStopSliding()
    {
        return !(IsActionHeld(PlayerAction.Down) && IsActionHeld(PlayerAction.Jump));
    }

    public void ResetJumpBuffer()
    {
        ResetActionBuffer(PlayerAction.Jump);
    }

    public void ResetSlideBuffer()
    {
        ResetActionBuffer(PlayerAction.Jump);
    }
}
