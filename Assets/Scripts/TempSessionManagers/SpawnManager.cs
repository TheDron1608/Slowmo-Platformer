using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;

    public List<LootDropChanceInfo> LootDropsInstance = new();
    public List<EnemySpawnInfo> EnemyPoolInstance = new();
    public CharacterComponentsManager PlayerCharacter;
    public float EnemyAmountPerSpawner = 1f;

    private List<LootDropChanceInfo> _lootDrops;
    private List<EnemySpawnInfo> _enemyPool;

    public List<LootDropChanceInfo> LootDrops
    {
        get => _lootDrops;
        set => _lootDrops = value;
    }
    public List<EnemySpawnInfo> EnemyPool
    {
        get => _enemyPool;
        set => _enemyPool = value;
    }

    public List<GameObject> GetLootDropsByType(LootDropChanceInfo.LootSpawnerTypes type)
    {
        List<GameObject> result = new();
        foreach (LootDropChanceInfo lootDrop in LootDrops)
        {
            if (lootDrop.Spawners.Contains(type))
            {
                GameObject newLoot = lootDrop.GetRandomLoot();
                if (newLoot != null)
                {
                    result.Add(newLoot);
                }
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
        return SpawnPlayerCharacterAt(
            VectorMath.Vec3ToVec3Int(WorldGenerationManager.Instance.GeneratedBuildings.First().Enter.GetSpawnPosition()),
            WorldGenerationManager.Instance.GeneratedBuildings.First().Layer
            );
    }

    public CharacterComponentsManager SpawnPlayerCharacterAt(Vector3 position, ZIndexLayer layer)
    {
        if (PlayerCharacter != null)
        {
            return layer.TrySpawnObject(
                PlayerCharacter.gameObject,
                position,
                null,
                null
                ).First().GetComponent<AbstractCharacterComponent>().CharComponents;
        }
        else
        {
            return null;
        }
    }

    private void Awake()
    {
        if (Instance != null) throw new UnityException("maximum of 1 SpawnManager instance");
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LootDrops = NumberMath.CreateCopyOfListOfInstantiatableObjs(LootDropsInstance);
        EnemyPool = NumberMath.CreateCopyOfListOfInstantiatableObjs(EnemyPoolInstance);
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}
