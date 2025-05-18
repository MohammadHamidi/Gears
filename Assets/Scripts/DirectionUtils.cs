using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
public static class DirectionUtils
{
    public static readonly Vector2Int[] CardinalDirections = new[]
    {
        Vector2Int.up,
        Vector2Int.right,
        Vector2Int.down,
        Vector2Int.left
    };

    public static Vector2Int Opposite(Vector2Int dir)
    {
        return -dir;
    }
}


