using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public static class NumberMath
{
    public static float RelativeRandomizeFloatNoSeed(float value, float randomSpread)
    {
        return value + (Random.value - 0.5f) * 2f * randomSpread;
    }

    public static float PickRandomInRangeNoSeed(float min, float max)
    {
        return min + (Random.value * (max - min));
    }
    public static int PickRandomInRangeNoSeed(int min, int max)
    {
        return min + (int)(Random.value * (max - min));
    }

    public static T PickRandomItemNoSeed<T>(List<T> vector)
    {
        return vector[(int)(Random.value * vector.Count)];
    }
    public static T PickRandomItemNoSeed<T>(List<T> vector, int limit)
    {
        if (limit == -1) return PickRandomItemNoSeed(vector);

        return vector[(int)(Random.value * Mathf.Min(vector.Count, limit))];
    }

    public static float RelativeLerp(float min, float max, float relativeDelta)
    {
        return (max - min) / (relativeDelta - min);
    }

    public static float LimitFloatBetweenZeroAndOne(float value)
    {
        if (value < 0f)
        {
            return 0f;
        }
        else if (value > 1f)
        {
            return 1f;
        }
        else
        {
            return value;
        }
    }

    public static TFind FindElemByType<TFind, TList>(List<TList> list) where TFind : TList
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] is TFind result)
            {
                return result;
            }
        }
        return default;
    }
}