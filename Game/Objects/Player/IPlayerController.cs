namespace WilyMachine;

public interface IPlayerController
{
    bool ShouldMoveUp();
    bool ShouldMoveDown();
    bool ShouldMoveLeft();
    bool ShouldMoveRight();

    bool ShouldJump();
    bool ShouldStopJumping();

    bool ShouldShoot();
    bool ShouldChargeWeapon();

    bool ShouldSlide();
    bool ShouldStopSliding();
}
