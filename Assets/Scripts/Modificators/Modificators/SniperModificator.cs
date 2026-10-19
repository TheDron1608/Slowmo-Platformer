using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

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

        Debug.Log(_spawnedSniper);
        if (_spawnedSniper != null && !_spawnedSniper.IsDestroyed()) Destroy(_spawnedSniper.gameObject);
    }
}
