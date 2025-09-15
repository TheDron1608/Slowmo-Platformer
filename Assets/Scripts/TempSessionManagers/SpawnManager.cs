using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;

    public List<LootDropChanceInfo> LootDrops = new();
    public List<EnemySpawnInfo> EnemyPool = new();

    public List<GameObject> GetLootDropsByType(LootDropChanceInfo.LootSpawnerTypes type)
    {
        List<GameObject> result = new();
        foreach (LootDropChanceInfo lootDrop in LootDrops)
        {
            if (lootDrop.AnyDropChance < 1f && Random.value > lootDrop.AnyDropChance) continue;

            if (lootDrop.Spawners.Contains(type))
            {
                result.AddRange(lootDrop.GetRandomLoot());
            }
        }
        return result;
    }

    public EnemySpawnInfo PickRandomEnemy()
    {
        float enemyKey = 0;
        EnemyPool.ForEach((enemy) => enemyKey += enemy.Rarity);
        enemyKey *= Random.value;

        foreach (EnemySpawnInfo enemy in EnemyPool)
        {
            if (enemyKey <= enemy.Rarity) return enemy;
            enemyKey -= enemy.Rarity;
        }
        throw new UnityException("enemy key out of enemy pool range");
    }

    private void Awake()
    {
        if (Instance != null) throw new UnityException("maximum of 1 SpawnManager instance");
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}
