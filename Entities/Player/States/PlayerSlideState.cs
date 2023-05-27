using Godot;

namespace Spaghetti.Godot;

public partial class PlayerSlideState : PlayerOnGroundState
{
    [Export] public int LockedFrameCount { get; set; } = 8;
    [Export] public int SlideFrameCount { get; set; } = 25;
}
