using UnityEngine;

public static class DirectionExtensions
{
    public static DoorDirection ToDoorDirection(this Vector2Int dir)
    {
        if (dir == Vector2Int.up) return DoorDirection.Up;
        if (dir == Vector2Int.down) return DoorDirection.Down;
        if (dir == Vector2Int.left) return DoorDirection.Left;
        return DoorDirection.Right;
    }

    public static Vector2Int ToVector(this DoorDirection dir)
    {
        switch (dir)
        {
            case DoorDirection.Up: return Vector2Int.up;
            case DoorDirection.Down: return Vector2Int.down;
            case DoorDirection.Left: return Vector2Int.left;
            default: return Vector2Int.right;
        }
    }
}
