namespace Spaghetti.Godot;

public partial class WeaponStateMachine : StateMachine
{
    public new WeaponState? State => base.State as WeaponState;
}
