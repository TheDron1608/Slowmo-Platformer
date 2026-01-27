using Unity.Mathematics;

public class MultiplyEnemiesAmountModificator : AbstractMultiplierableModificator
{
    public float EnemyAmountMultiplier;

    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

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