using UnityEngine;

public static class VectorExtensions
{
    public static Vector3 ToVector3(this Vector2Int v2Int, float z = 0f)
    {
        return new Vector3(v2Int.x, v2Int.y, z);
    }
}