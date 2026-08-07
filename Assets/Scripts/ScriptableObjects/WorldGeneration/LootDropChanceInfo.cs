using System.Collections.Generic;
using Unity.Mathematics;
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
        RANGED_WEAPON,
        SHIELD,
        ARMOR,
        LYING_DROP
    }

    public float AnyDropChance = 1f;
    public List<LootSpawnerTypes> Spawners = new();
    public List<GameObject> PossibleLoot = new();

    public GameObject GetRandomLoot()
    {
        if (AnyDropChance < 1f && UnityEngine.Random.value > AnyDropChance) return null;
        return NumberMath.PickRandomItem(PossibleLoot);
    }
}
