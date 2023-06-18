namespace Spaghetti;

public sealed partial class Player
{
    public void ChooseSpriteAnimation()
    {
        if (IsStunned)
        {
            ChangeSpriteAnimation(PlayerAnimation.Stagger);
        }
        else if (IsTeleporting)
        {
            ChangeSpriteAnimation(IsMorphing ? PlayerAnimation.Morph : PlayerAnimation.Teleport);
        }
        else if (IsClimbing)
        {
            if (IsShooting)
            {
                ChangeSpriteAnimation(PlayerAnimation.ClimbShoot);
            }
            else
            {
                ChangeSpriteAnimation(IsTouchingLadderTop
                    ? PlayerAnimation.ClimbExit
                    : PlayerAnimation.ClimbMove);
            }
        }
        else if (IsIdle)
        {
            ChangeSpriteAnimation(IsShooting ? PlayerAnimation.IdleShoot : PlayerAnimation.Idle);
        }
        else if (IsWalking)
        {
            if (!IsFullAcceleration)
            {
                ChangeSpriteAnimation(PlayerAnimation.TipToe);
            }
            else
            {
                ChangeSpriteAnimation(IsShooting ? PlayerAnimation.MoveShoot : PlayerAnimation.Move);
            }
        }
        else if (IsSliding)
        {
            ChangeSpriteAnimation(PlayerAnimation.Slide);
        }
        else if (IsAirborn)
        {
            ChangeSpriteAnimation(IsShooting ? PlayerAnimation.JumpShoot : PlayerAnimation.Jump);
        }
    }

    public void ChooseEffectAnimation()
    {
        if (IsStunned)
        {
            ChangeEffectAnimation(PlayerEffectAnimation.Stun);
        }
        else if (IsBlinking)
        {
            ChangeEffectAnimation(PlayerEffectAnimation.Blink);
        }
        else
        {
            ChangeEffectAnimation(PlayerEffectAnimation.RESET);
        }
    }
}
