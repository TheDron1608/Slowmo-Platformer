
public class SetEnemyExtraHoldableChanceModificator : AbstractModificator
{
    public float ExtraHoldableChance;

    private float _oldExtraHoldableChance;

    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

        _oldExtraHoldableChance = SpawnManager.Instance.ChanceToGiveCharacterExtraHoldable;
        SpawnManager.Instance.ChanceToGiveCharacterExtraHoldable = ExtraHoldableChance;
    }

    public override void OnModificatorRemoved()
    {
        base.OnModificatorRemoved();

        if (SpawnManager.Instance != null)
        {
            SpawnManager.Instance.ChanceToGiveCharacterExtraHoldable = _oldExtraHoldableChance;
        }
    }
}
