using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public static class NumberMath
{
    public static float RelativeRandomizeFloatNoSeed(float value, float randomSpread)
    {
        return value + (UnityEngine.Random.value - 0.5f) * 2f * randomSpread;
    }

    public static float PickRandomInRangeNoSeed(float min, float max)
    {
        return min + (UnityEngine.Random.value * (max - min));
    }
    public static int PickRandomInRangeNoSeed(int min, int max)
    {
        if (min == max) return max;
        return min + Mathf.RoundToInt(UnityEngine.Random.value * (max - min));
    }

    public static T PickRandomItem<T>(List<T> vector)
    {
        if (vector.Count == 0) return default;
        return vector[(int)(UnityEngine.Random.value * (vector.Count))];
    }
    public static T PickRandomItem<T>(List<T> vector, int limit)
    {
        if (vector.Count == 0) return default;
        if (limit == -1) return PickRandomItem(vector);

        return vector[(int)(UnityEngine.Random.value * Mathf.Min(vector.Count, limit))];
    }
    public static T PickRandomItem<T>(T[] array)
    {
        if (array.Length == 0) return default;
        return array[(int)(UnityEngine.Random.value * (array.Length))];
    }
    public static T PickRandomItem<T>(T[] array, int limit)
    {
        if (limit == -1) return PickRandomItem(array);

        return array[(int)(UnityEngine.Random.value * Mathf.Min(array.Length, limit))];
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

    public static bool GetListContainsAnyItemOfAnotherList<T>(List<T> findIn, List<T> findWhat)
    {
        for (int i = 0; i < findWhat.Count; i++)
        {
            if (findIn.Contains(findWhat[i])) return true;
        }
        return false;
    }

    public static bool GetAllListItemsAreValidByCondition<T>(List<T> list, System.Func<T, bool> condition)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (!condition.Invoke(list[i]))
            {
                return false;
            }
        }

        return true;
    }

    public static bool GetAnyListItemsIsValidByCondition<T>(List<T> list, System.Func<T, bool> condition)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (condition.Invoke(list[i]))
            {
                return true;
            }
        }

        return false;
    }

    public static rT GetListCallbackReturnValueOfListItemsTilNotNull<rT, lT>(List<lT> list, System.Func<lT, rT> callback)
    {
        for (int i = 0; i < list.Count; i++)
        {
            rT result = callback(list[i]);
            if (result != null) return result;
        }

        return default(rT);
    }

    public static bool GetListContainsComponent<T, lT>(List<lT> list) where lT : MonoBehaviour where T : MonoBehaviour
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].GetComponent<T>() != null)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Return true of false with 50/50 chance
    /// </summary>
    public static bool RandomCoinflip()
    {
        return UnityEngine.Random.value > 0.5f;
    }

    public static T PickMiddleItemFromList<T>(List<T> list)
    {
        return list[list.Count / 2];
    }

    public static Vector3Int Vec3ToVec3Int(Vector3 vec3)
    {
        return new Vector3Int(
            (int)math.floor(vec3.x),
            (int)math.floor(vec3.y),
            (int)math.floor(vec3.z)
            );
    }

    public static float LimitFloatInRange(float value, float min, float max)
    {
        return math.min(max, math.max(min, value));
    }
}