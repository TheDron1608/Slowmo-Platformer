using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1)]
public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance;

    public bool KeepHoldableOnFinishLevel = true;
    public List<LootDropChanceInfo> LootDropsInstance = new();
    public List<EnemySpawnInfo> EnemyPoolInstance = new();
    public CharacterComponentsManager PlayerCharacter;
    public Holdable PlayerCharacterHoldable = null;

    [SerializeField] private float _enemyAmountPerSpawner = 1f;
    private float _actualEnemyAmountPerSpawner = 1f;
    private List<LootDropChanceInfo> _lootDrops;
    private List<EnemySpawnInfo> _enemyPool;

    public float EnemyAmountPerSpawner
    {
        get => _enemyAmountPerSpawner;
        set
        {
            _actualEnemyAmountPerSpawner = math.max(_enemyAmountPerSpawner, value);
            _enemyAmountPerSpawner = value;
        }
    }
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
    public float ActualEnemyAmountPerSpawner
    {
        get => _actualEnemyAmountPerSpawner;
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

    public void FinishGameplay(AbstractCharacterComponent finishedCharacter, string loadScene)
    {
        Holdable saveHoldable = finishedCharacter?.CharComponents.CharacterHolding.CurrentHoldObject;

        if (KeepHoldableOnFinishLevel && saveHoldable != PlayerCharacterHoldable)
        {
            if (
                PlayerCharacterHoldable != null &&
                PlayerCharacterHoldable.gameObject.scene.name != null
                )
            {
                Destroy(PlayerCharacterHoldable.gameObject);
            }

            PlayerCharacterHoldable = saveHoldable;
            if (PlayerCharacterHoldable?.gameObject.scene.name != null)
            {
                PlayerCharacterHoldable.gameObject.SetActive(false);
            }
            PlayerCharacterHoldable?.transform.SetParent(transform);
        }
        else if (!KeepHoldableOnFinishLevel && PlayerCharacterHoldable != null)
        {
            if (PlayerCharacterHoldable.gameObject.scene.name != null)
            {
                Destroy(PlayerCharacterHoldable.gameObject);
            }
            PlayerCharacterHoldable = null;
        }

        AnalyticsManager.Instance.RecordEvent(new LevelFinishAnalyticsEvent());
        AnalyticsManager.Instance.ResetTrackedInfo();

        UIManager.Instance.LoadSceneWithEffect(loadScene);
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
            CharacterComponentsManager newPlayer = layer.TrySpawnObject(
                PlayerCharacter.gameObject,
                position,
                null,
                null
                ).First().GetComponent<AbstractCharacterComponent>().CharComponents;

            if (KeepHoldableOnFinishLevel)
            {
                PlayerCharacterHoldable?.gameObject.SetActive(true);
                newPlayer.CharacterHolding.GiveNewHoldable(PlayerCharacterHoldable);
            }

            return newPlayer;
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

        SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
    }

    private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
    {
        _actualEnemyAmountPerSpawner = EnemyAmountPerSpawner;
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
        Instance = null;
    }
}
