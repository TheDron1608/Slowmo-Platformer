using UnityEngine;
using UnityEngine.SceneManagement;

public class RemoveDefaultEnemyPoolModificator : AbstractModificator
{
    private EnemySpawnInfo _removedPool = null;

    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

        if (SpawnManager.Instance.EnemyPool.Count <= 1) return;

        _removedPool = SpawnManager.Instance.EnemyPool[0];
        SpawnManager.Instance.EnemyPool.RemoveAt(0);

        if (LayerManager.Instance != null && SceneManager.GetActiveScene().name != SceneList.BOSS)
        {
            foreach (ZIndexLayer layer in LayerManager.Instance.ZLayers)
            {
                int spawnedAmount = 0;
                for (int i = 0; i < layer.CharactersContainer.childCount - spawnedAmount; i++)
                {
                    if (
                        layer.CharactersContainer.GetChild(i).TryGetComponent(out AbstractCharacterComponent character) &&
                        character.CharComponents.CharacterTeam.GetIsAllyToAnotherTeam(TeamManager.Teams.DEFAULT_ENEMY)
                        )
                    {
                        Vector2 oldCharacterPosition = layer.CharactersContainer.GetChild(i).position;
                        if (character.CharComponents != null)
                        {
                            character.CharComponents.CharacterHealth.Gib(null);
                        }
                        else
                        {
                            Destroy(character.gameObject);
                        }

                        SpawnManager.Instance.PickRandomEnemy().SpawnAt(oldCharacterPosition, layer);
                        spawnedAmount++;
                    }
                }
            }
        }
    }

    public override void OnLevelPreGenerated()
    {
        base.OnLevelPreGenerated();

        if (_removedPool == null && SpawnManager.Instance.EnemyPool.Count > 1)
        {
            _removedPool = SpawnManager.Instance.EnemyPool[0];
            SpawnManager.Instance.EnemyPool.RemoveAt(0);
        }
    }

    public override void OnModificatorRemoved()
    {
        base.OnModificatorRemoved();

        if (SpawnManager.Instance != null && _removedPool != null)
        {
            SpawnManager.Instance.EnemyPool.Insert(0, _removedPool);
        }
    }
}