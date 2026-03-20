using UnityEngine;

public class AddEnemyPoolItemModificator : AbstractModificator
{
    public EnemySpawnInfo AddedEnemyItem;

    private EnemySpawnInfo _addedEnemyItem;

    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

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