using UnityEngine;
using System;
using Unity.Mathematics;

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

    public static Vector2 Vec3ToVec2(Vector3 vector3)
    {
        return new Vector2(vector3.x, vector3.y);
    }

    public static Vector3 Vec2ToVec3(Vector2 xy)
    {
        return new Vector3(
            xy.x,
            xy.y,
            0f
            );
    }
    public static Vector3 Vec2ToVec3(Vector2 xy, float z)
    {
        return new Vector3(
            xy.x,
            xy.y,
            z
            );
    }

    public static Quaternion Vec3ToQuarterninon2D(Vector3 target)
    {
        Quaternion result = new();
        result.eulerAngles = new Vector3(
            0f,
            target.x < 0f ? 180f : 0f,
            target.y * 90f
            );

        return result;
    }

    public static float RigidBodyVelocityToSpeed(Rigidbody2D rb)
    {
        return (math.abs(rb.linearVelocityX) + math.abs(rb.linearVelocityY)) / 2;
    }
}
