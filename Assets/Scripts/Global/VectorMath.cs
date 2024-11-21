using UnityEngine;
using System;

public static class VectorMath
{
    public static bool GetVectorsEqual(Vector3 v1, Vector3 v2, float delta)
    {
        return (
            Math.Abs(v1.x - v2.x) <= delta &&
            Math.Abs(v1.y - v2.y) <= delta &&
            Math.Abs(v1.z - v2.z) <= delta
            );
    }

    public static bool GetVectorsEqual(Vector2 v1, Vector2 v2, float delta)
    {
        return (
            Math.Abs(v1.x - v2.x) <= delta &&
            Math.Abs(v1.y - v2.y) <= delta
            );
    }
}
