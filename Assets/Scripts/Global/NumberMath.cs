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
        if (vector == null || vector.Count == 0) return default;
        return vector[(int)math.round(UnityEngine.Random.value * (vector.Count - 1))];
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
        return array[math.min((int)(UnityEngine.Random.value * array.Length), array.Length - 1)];
    }
    public static T PickRandomItem<T>(T[] array, int limit)
    {
        if (limit == -1) return PickRandomItem(array);

        return array[math.min((int)(UnityEngine.Random.value * Mathf.Min(array.Length, limit)), array.Length - 1)];
    }

    public static T PickRandomItem<T>(List<T> vector, T excludeObject)
    {
        if (vector.Count == 0) return default;
        int randomIndex = (int)(UnityEngine.Random.value * (vector.Count - 1));
        if (randomIndex >= vector.IndexOf(excludeObject) && vector.Count - 1 > randomIndex) randomIndex++;

        return vector[randomIndex];
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

    public static float LimitFloatBetweenMinusOneAndOne(float value)
    {
        if (value < -1f)
        {
            return -1f;
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
            for (int j = 0; j < findIn.Count; j++)
            {
                if (findIn[j].Equals(findWhat[i])) return true;
            }
        }
        return false;
    }

    public static bool GetListContainsComponent<T, lT>(List<lT> list) where lT : MonoBehaviour where T : MonoBehaviour
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].TryGetComponent(out T t))
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

    public static List<T> MergeLists<T>(params List<T>[] lists)
    {
        List<T> result = new List<T>();
        foreach (var list in lists)
        {
            if (list != null) result.AddRange(list);
        }
        return result;
    }

    public static void FillArray<T>(T[] array, T value)
    {
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = value;
        }
    }

    public static T PickMiddleItem<T>(List<T> list)
    {
        if (list.Count == 0) return default;
        return list[(int)math.floor(list.Count / 2f)];
    }

    public static List<T> CreateCopyOfListOfInstantiatableObjs<T>(List<T> copy) where T : Object
    {
        List<T> result = new List<T>(copy.Count);
        for (int i = 0; i < copy.Count; i++)
        {
            result.Insert(i, GameObject.Instantiate(copy[i]));
        }
        return result;
    }

    public static void RemoveListMultiItems<T>(List<T> removeFrom, List<T> remove)
    {
        foreach (T removeItem in remove)
        {
            removeFrom.Remove(removeItem);
        }
    }

    public static LinkedList<T> ArrayToLinkedList<T>(T[] array)
    {
        LinkedList<T> result = new();
        foreach (var arrayVal in array) result.AddLast(arrayVal);
        return result;
    }
}