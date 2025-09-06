using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LootDropChanceInfo", menuName = "WorldGeneration/LootDropChanceInfo")]
public class LootDropChanceInfo : ScriptableObject
{
    public enum LootSpawnerTypes
    {
        BOX,
        CLOSET,
        DOOR,
        DECORATIVE_FURNITURE,
        MELEE_WEAPON,
        RANGED_WEAPON
    }

    public float AnyDropChance = 1f;
    public int MinLootAmount = 1;
    public int MaxLootAmount = 1;
    public List<LootSpawnerTypes> Spawners = new();
    public List<GameObject> PossibleLoot = new();

    public List<GameObject> GetRandomLoot()
    {
        int randomAmount = NumberMath.PickRandomInRangeNoSeed(MinLootAmount, MaxLootAmount);
        List<GameObject> result = new(randomAmount);
        for (int i = 0; i < randomAmount; i++)
        {
            result.Insert(i, NumberMath.PickRandomItem(PossibleLoot));
        }
        return result;
    }
}
