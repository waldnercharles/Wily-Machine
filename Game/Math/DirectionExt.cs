using Godot;

namespace Spaghetti;

public static class DirectionExt
{
    public static Direction Opposite(this Direction direction)
    {
        return direction switch
        {
            Direction.Up => Direction.Down,
            Direction.Right => Direction.Left,
            Direction.Down => Direction.Up,
            Direction.Left => Direction.Right,
            _ => Direction.None
        };
    }

    public static Vector2 ToVector2(this Direction direction)
    {
        return direction switch
        {
            Direction.Up => Vector2.Up,
            Direction.Right => Vector2.Right,
            Direction.Down => Vector2.Down,
            Direction.Left => Vector2.Left,
            _ => Vector2.Zero
        };
    }
}
