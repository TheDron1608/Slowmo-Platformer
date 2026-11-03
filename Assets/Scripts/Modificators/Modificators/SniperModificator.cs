using Unity.VisualScripting;

public class SniperModificator : AbstractModificator
{
    public Sniper Sniper;

    private Sniper _spawnedSniper = null;

    public override void OnLevelGenerated()
    {
        base.OnLevelGenerated();

        _spawnedSniper = Sniper.SpawnSelf();
    }

    public override void OnModificatorRemoved()
    {
        base.OnModificatorAdded();

        if (_spawnedSniper != null && !_spawnedSniper.IsDestroyed()) Destroy(_spawnedSniper.gameObject);
    }
}
