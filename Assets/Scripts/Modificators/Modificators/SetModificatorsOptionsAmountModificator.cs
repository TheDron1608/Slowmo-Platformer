using Unity.Mathematics;

public class SetModificatorsOptionsAmountModificator : AbstractMultiplierableModificator
{
    public int AddOptions = 0;

    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

        ModificatorsManager.Instance.MaxModificatorOptions += (int)math.round(AddOptions * ModificatorMultiplier);
    }

    public override void OnModificatorRemoved()
    {
        base.OnModificatorRemoved();

        if (ModificatorsManager.Instance != null)
        {
            ModificatorsManager.Instance.MaxModificatorOptions -= (int)math.round(AddOptions * ModificatorMultiplier);
        }
    }
}