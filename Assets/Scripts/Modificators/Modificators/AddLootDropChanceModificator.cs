using System.Collections.Generic;

public class AddLootDropChanceModificator : AbstractModificator
{
    public List<LootDropChanceInfo> LootDropChances;

    private List<LootDropChanceInfo> _addedLootDropChance = null;

    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

        _addedLootDropChance = new();
        foreach (LootDropChanceInfo dropChanceInfo in LootDropChances)
        {
            LootDropChanceInfo newDropChanceInfo = Instantiate(dropChanceInfo);
            SpawnManager.Instance.LootDrops.Add(newDropChanceInfo);
            _addedLootDropChance.Add(newDropChanceInfo);
        }
    }

    public override void OnModificatorRemoved()
    {
        base.OnModificatorRemoved();

        if (SpawnManager.Instance != null)
        {
            foreach (LootDropChanceInfo addedLootDropChanceInfo in _addedLootDropChance)
            {
                SpawnManager.Instance.LootDrops.Remove(addedLootDropChanceInfo);
            }
        }
    }
}