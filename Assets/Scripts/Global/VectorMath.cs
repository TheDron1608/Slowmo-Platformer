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

    public static bool GetNormalizedVectorsEqual(Vector2 vec1, Vector2 vec2, float delta)
    {
        return vec2 == Vector2.MoveTowards(vec1, vec2, delta);
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

    public static Quaternion Vec2ToQuarterninon2D(Vector2 target)
    {
        Quaternion result = new();
        result.eulerAngles = new Vector3(
            0f,
            target.x < 0f ? 180f : 0f,
            Mathf.Atan2(target.x, target.y) * Mathf.Rad2Deg * (target.x > 0f ? -1 : 1) + 90f
            );

        return result;
    }

    public static Vector3 Quartenion2DToVec3(Quaternion target)
    {
        return math.mul(target, Vector3.right);
    }

    public static Vector2 Quartenion2DToVec2(Quaternion target)
    {
        return Vec3ToVec2(math.mul(target, Vector3.right));
    }

    public static float Vec2ToDistance(Vector2 vector2)
    {
        return (math.abs(vector2.x) + math.abs(vector2.y)) / 2;
    }

    /// <summary>
    /// Relatively randomized quaternion eulaterAngles.z rotation
    /// </summary>
    /// <param name="quaternion">randomizes quaternion</param>
    /// <param name="accuracy">Value between 1 and 0, where 0 is perfect accuracy and 1 is 360 deg spread</param>
    /// <returns></returns>
    public static Quaternion RandomizeQuarternion(Quaternion quaternion, float accuracy)
    {
        Vector3 eulerAngles =  quaternion.eulerAngles;

        quaternion.eulerAngles = new Vector3(
            eulerAngles.x,
            eulerAngles.y,
            eulerAngles.z + 360f * (UnityEngine.Random.value - 0.5f) * (1 - accuracy)
            );
        return quaternion;
    }

    public static Vector2 GetAngleToAsNormalizedVec2(Vector2 from, Vector2 to)
    {
        return (to - from).normalized;
    }
}
