using Unity.Mathematics;
using UnityEngine;

public class MultiplyEnemiesAmountModificator : AbstractModificator
{
    const float SPAWN_NEAR_DISTANCE = 0.25f;
    const TeamManager.Teams CLONE_TEAM_MEMBERS_ONLY = TeamManager.Teams.DEFAULT_ENEMY;

    public float EnemyAmountMultiplier;

    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

        if (LayerManager.Instance != null)
        {
            float addEnemiesAmount = (EnemyAmountMultiplier / SpawnManager.Instance.ActualEnemyAmountPerSpawner) - 1;
            if (addEnemiesAmount > 0f)
            {
                foreach (ZIndexLayer layer in LayerManager.Instance.ZLayers)
                {
                    int baseCharactersAmount = layer.CharactersContainer.childCount;
                    for (int i = 0; i < baseCharactersAmount; i++)
                    {
                        //will not affect on dead characters
                        if (
                            layer.CharactersContainer.GetChild(i).TryGetComponent(out AbstractCharacterComponent character) &&
                            character.CharComponents.CharacterTeam.Team == CLONE_TEAM_MEMBERS_ONLY &&
                            !character.CharComponents.CharacterEffectsReceiver.GetHasEffect<ILethalEffect>()
                            )
                        {
                            for (float j = addEnemiesAmount; j > 0; j--)
                            {
                                if (1 > 0 || UnityEngine.Random.value < j)
                                {
                                    SpawnManager.Instance.PickRandomEnemy().SpawnAt(
                                        layer.CharactersContainer.GetChild(i).position + (Vector3.right * (UnityEngine.Random.value * 2 - 1) * SPAWN_NEAR_DISTANCE), 
                                        layer
                                        );
                                }
                            }
                        }
                    }
                }
            }
        }

        SpawnManager.Instance.EnemyAmountPerSpawner *= EnemyAmountMultiplier * ModificatorMultiplier;
    }

    public override void OnModificatorRemoved()
    {
        base.OnModificatorRemoved();

        if (SpawnManager.Instance != null)
        {
            SpawnManager.Instance.EnemyAmountPerSpawner /= EnemyAmountMultiplier * ModificatorMultiplier;
        }
    }
}