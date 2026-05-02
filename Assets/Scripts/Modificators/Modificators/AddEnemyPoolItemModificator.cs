using UnityEngine;

public class AddEnemyPoolItemModificator : AbstractModificator
{
    public EnemySpawnInfo AddedEnemyItem;

    private EnemySpawnInfo _addedEnemyItem;

    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

        if (LayerManager.Instance != null)
        {
            foreach (ZIndexLayer layer in LayerManager.Instance.ZLayers)
            {
                foreach (Transform characterT in layer.CharactersContainer)
                {
                    if (
                        UnityEngine.Random.value < AddedEnemyItem.Rarity &&
                        characterT.TryGetComponent(out AbstractCharacterComponent character) &&
                        character.CharComponents.CharacterTeam.GetIsAllyToAnotherTeam(TeamManager.Teams.DEFAULT_ENEMY)
                        ) 
                    {
                        Vector2 oldCharacterPosition = characterT.position;
                        if (character.CharComponents != null)
                        {
                            character.CharComponents.CharacterHealth.Gib(null);
                        }
                        else
                        {
                            Destroy(character.gameObject);
                        }
                        AddedEnemyItem.SpawnAt(oldCharacterPosition, layer);
                    }
                }
            }
        }

        _addedEnemyItem = Instantiate(AddedEnemyItem);
        _addedEnemyItem.Rarity *= ModificatorMultiplier;

        SpawnManager.Instance.EnemyPool.Add(_addedEnemyItem);
    }

    public override void OnModificatorRemoved()
    {
        base.OnModificatorRemoved();

        if (SpawnManager.Instance != null)
        {
            SpawnManager.Instance.EnemyPool.Remove(_addedEnemyItem);
        }
    }
}