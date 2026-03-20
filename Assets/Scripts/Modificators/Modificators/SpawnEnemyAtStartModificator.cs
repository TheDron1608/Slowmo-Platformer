using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class SpawnEnemyAtStartModificator : AbstractModificator
{
    public EnemySpawnInfo Enemy;
    public float SpawnDelaySeconds = 5f;

    private List<CharacterComponentsManager> _addedEnemies = new();

    public override void OnLevelGenerated()
    {
        base.OnLevelGenerated();

        StartCoroutine(AwaitDelayThenSpawnEnemy());
    }

    private IEnumerator AwaitDelayThenSpawnEnemy()
    {
        yield return new WaitForSeconds(SpawnDelaySeconds);

        if (WorldGenerationManager.Instance.GeneratedBuildings.Count > 0)
        {
            for (int i = 0; i < math.max(1f, ModificatorMultiplier); i++)
            {
                CharacterComponentsManager newEnemy = Enemy.SpawnAt(
                    WorldGenerationManager.Instance.GeneratedBuildings.First().Enter.GetSpawnPosition(),
                    WorldGenerationManager.Instance.GeneratedBuildings.First().Layer
                    );

                _addedEnemies.Add(newEnemy);
            }
        }
    }

    public override void OnModificatorRemoved()
    {
        base.OnModificatorRemoved();

        foreach (CharacterComponentsManager addedEnemy in _addedEnemies)
        {
            if (addedEnemy != null && !addedEnemy.IsDestroyed())
            {
                addedEnemy.CharacterHealth?.Gib(null);
            }
        }
        _addedEnemies = new();
    }

    public override void OnLevelFinished()
    {
        base.OnLevelFinished();

        _addedEnemies = new();
    }
}