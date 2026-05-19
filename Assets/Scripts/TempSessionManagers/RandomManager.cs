using System;
using UnityEngine;

public class RandomManager : MonoBehaviour
{
    const int RANDOM_SEEDS_AMOUNT = 100;

    public enum ProcChanceTypes
    {
        GOOD,
        BAD
    }

    public static RandomManager Instance;

    public float RandomChanceProcMultiplier = 1f;
    public float GoodRandomChanceProcMultiplier = 1f;
    public float BadRandomChanceProcMultiplier = 1f;

    public event EventHandler OnBadRandomChanceProcd;
    public event EventHandler OnGoodRandomChanceProcd;

    private int[] _randomSeeds = new int[RANDOM_SEEDS_AMOUNT];
    private int _seedIteration = 0;

    public bool ProcRandomChance(float baseChance, ProcChanceTypes type)
    {
        switch (type)
        {
            case ProcChanceTypes.GOOD:
                return ProcRandomGoodChance(baseChance);
            case ProcChanceTypes.BAD:
                return ProcRandomBadChance(baseChance);
        }
        throw new UnityException(type + " is not valid for ProcRandomChance arg");
    }
    public bool ProcRandomBadChance(float baseChance)
    {
        bool result = (UnityEngine.Random.value * RandomChanceProcMultiplier * BadRandomChanceProcMultiplier) < baseChance;
        if (result) OnBadRandomChanceProcd?.Invoke(this, EventArgs.Empty);
        return result;
    }
    public bool ProcRandomGoodChance(float baseChance)
    {
        bool result = (UnityEngine.Random.value * RandomChanceProcMultiplier * GoodRandomChanceProcMultiplier) < baseChance;
        if (result) OnGoodRandomChanceProcd?.Invoke(this, EventArgs.Empty);
        return result;
    }

    public bool ProcRandomChanceNoTrigger(float baseChance, ProcChanceTypes type)
    {
        switch (type)
        {
            case ProcChanceTypes.GOOD:
                return ProcRandomGoodChanceNoTrigger(baseChance);
            case ProcChanceTypes.BAD:
                return ProcRandomBadChanceNoTrigger(baseChance);
        }
        throw new UnityException(type + " is not valid for ProcRandomChance arg");
    }
    public bool ProcRandomBadChanceNoTrigger(float baseChance)
    {
        return (UnityEngine.Random.value * RandomChanceProcMultiplier * BadRandomChanceProcMultiplier) < baseChance;
    }
    public bool ProcRandomGoodChanceNoTrigger(float baseChance)
    {
        return (UnityEngine.Random.value * RandomChanceProcMultiplier * GoodRandomChanceProcMultiplier) < baseChance;
    }

    public void InitNewSeed()
    {
        _seedIteration = (_seedIteration + 1) % _randomSeeds.Length;
        UnityEngine.Random.InitState(_randomSeeds[_seedIteration]);
    }

    private void Awake()
    {
        if (Instance != null) throw new UnityException("maximum of 1 RandomManager instance");
        Instance = this;
        DontDestroyOnLoad(gameObject);

        UnityEngine.Random.InitState((int)DateTime.Now.Ticks);

        for (int i = 0; i < _randomSeeds.Length; i++)
        {
            _randomSeeds[i] = (int)(UnityEngine.Random.value * int.MaxValue);
        }
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}
