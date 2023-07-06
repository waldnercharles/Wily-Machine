using Godot;

namespace WilyMachine;

public static class KinematicCollision2DExt
{
    public static bool IsFloorCollision(this KinematicCollision2D collision, Vector2 upDirection, float floorMaxAngle)
    {
        var normal = collision.GetNormal();
        var angle = collision.GetAngle();

        return Mathf.Sign(normal.Y) == Mathf.Sign(upDirection.Y) && angle < floorMaxAngle;
    }
}
