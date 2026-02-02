using UnityEngine;

public class AddEnemyPoolItemModificator : AbstractModificator
{
    EnemySpawnInfo AddedEnemyItem;

    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

        SpawnManager.Instance.EnemyPool.Add(AddedEnemyItem);
    }

    public override void OnModificatorRemoved()
    {
        base.OnModificatorRemoved();

        SpawnManager.Instance.EnemyPool.Remove(AddedEnemyItem);
    }
}