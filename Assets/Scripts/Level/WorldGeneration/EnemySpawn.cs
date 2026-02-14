using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemySpawn : GenerateOnFinishAllBuildingEnviroment
{
    public override List<GameObject> Generate(PreGeneratedEnviromentTempInfo generationInfo)
    {
        base.Generate(generationInfo);

        if (generationInfo.Chunk != null && generationInfo.Chunk == generationInfo?.Building?.Enter?.Chunk)
        {
            return null;
        }

        List<GameObject> result = new();
        int iter = 0;
        do
        {
            //check if EnemyAmountPerSpawn is enough for spawn
            //for example: if EnemyAmountPerSpawn is 2.5, will be spawned 2 enemies and 1 enemy with 50% chance
            if (
                iter + 1 > SpawnManager.Instance.EnemyAmountPerSpawner &&
                Random.value > SpawnManager.Instance.EnemyAmountPerSpawner % 1
                )
            {
                break;
            }

            EnemySpawnInfo spawnInfo = SpawnManager.Instance.PickRandomEnemy();

            if (spawnInfo == null || spawnInfo.Enemy == null) return null;

            //Debug.Log(generationInfo.GetSpawnPosition() + " : " + gameObject.GetInstanceID());
            //spawn character
            CharacterComponentsManager newEnemy = spawnInfo.SpawnAt(
                generationInfo.GetTileSpawnPosition() + Vector3.right * NumberMath.PickRandomInRangeNoSeed(-0.5f, 0.5f),
                generationInfo.GenerateWhere
                );

            result.Add(newEnemy.gameObject);
            iter++;

        } while (iter < SpawnManager.Instance.EnemyAmountPerSpawner);

        return result;
    }
}
