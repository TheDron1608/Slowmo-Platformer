using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;

    public List<LootDropChanceInfo> LootDrops = new();
    public List<EnemySpawnInfo> EnemyPool = new();
    public CharacterComponentsManager PlayerCharacter;

    public List<GameObject> GetLootDropsByType(LootDropChanceInfo.LootSpawnerTypes type)
    {
        List<GameObject> result = new();
        foreach (LootDropChanceInfo lootDrop in LootDrops)
        {
            if (lootDrop.AnyDropChance < 1f && UnityEngine.Random.value > lootDrop.AnyDropChance) continue;

            if (lootDrop.Spawners.Contains(type))
            {
                result.AddRange(lootDrop.GetRandomLoot());
            }
        }
        return result;
    }

    public EnemySpawnInfo PickRandomEnemy()
    {
        if (EnemyPool.Count == 0) return null;

        float enemyKey = 0;
        EnemyPool.ForEach((enemy) => enemyKey += enemy.Rarity);
        enemyKey *= UnityEngine.Random.value;

        foreach (EnemySpawnInfo enemy in EnemyPool)
        {
            if (enemyKey <= enemy.Rarity) return enemy;
            enemyKey -= enemy.Rarity;
        }
        throw new UnityException("enemy key out of enemy pool range");
    }

    public CharacterComponentsManager SpawnPlayerCharacterAtStartPosition()
    {
        return 
            WorldGenerationManager.Instance.GeneratedBuildings.First()?.Layer.TrySpawnObject(
                PlayerCharacter.gameObject,
                VectorMath.Vec3ToVec3Int(WorldGenerationManager.Instance.GeneratedBuildings.First().Enter.GetSpawnPosition()),
                null,
                null
                ).First().GetComponent<AbstractCharacterComponent>().CharComponents;
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
