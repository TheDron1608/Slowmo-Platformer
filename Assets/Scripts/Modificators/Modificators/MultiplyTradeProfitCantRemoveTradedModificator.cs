public class MultiplyTradeProfitCantRemoveTradedModificator : MultiplyTradeProfitModificator
{
    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

        ModificatorsManager.Instance.RemoveModifictorsOnSell = false;
    }

    public override void OnModificatorRemoved()
    {
        if (ModificatorsManager.Instance != null)
        {
            ModificatorsManager.Instance.RemoveModifictorsOnSell = true;
        }
    }
}