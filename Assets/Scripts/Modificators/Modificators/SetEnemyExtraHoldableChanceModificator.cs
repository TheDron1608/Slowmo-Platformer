
public class SetEnemyExtraHoldableChanceModificator : AbstractModificator
{
    public float ExtraHoldableChance;

    private float _oldExtraHoldableChance;

    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

        _oldExtraHoldableChance = SpawnManager.Instance.ChanceToGiveCharacterAnyExtraHoldable;
        SpawnManager.Instance.ChanceToGiveCharacterAnyExtraHoldable = ExtraHoldableChance;
    }

    public override void OnModificatorRemoved()
    {
        base.OnModificatorRemoved();

        if (SpawnManager.Instance != null)
        {
            SpawnManager.Instance.ChanceToGiveCharacterAnyExtraHoldable = _oldExtraHoldableChance;
        }
    }
}
