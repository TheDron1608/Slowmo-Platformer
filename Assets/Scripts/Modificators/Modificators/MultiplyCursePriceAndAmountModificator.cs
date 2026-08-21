
public class MultiplyCursePriceAndAmountModificator : AbstractModificator
{
    public float CurseAmountMult = 1f;
    public float CursePriceMult = 1f;

    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.Instance.ModificatorsAmountMult *= CurseAmountMult;
            DifficultyManager.Instance.ModificatorsPriceMult *= CursePriceMult;
        }
    }

    public override void OnModificatorRemoved()
    {
        base.OnModificatorRemoved();

        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.Instance.ModificatorsAmountMult /= CurseAmountMult;
            DifficultyManager.Instance.ModificatorsPriceMult /= CursePriceMult;
        }
    }
}