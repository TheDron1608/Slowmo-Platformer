public class MultiplyTradeProfitModificator : AbstractModificator
{
    public float TradeCurseProfitMult = 1f;
    public float TradeBlessProfitMult = 1f;

    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

        ModificatorsManager.Instance.TradeCurseProfitMult *= TradeCurseProfitMult;
        ModificatorsManager.Instance.TradeBlessProfitMult *= TradeBlessProfitMult;
    }

    public override void OnModificatorRemoved()
    {
        base.OnModificatorRemoved();

        if (ModificatorsManager.Instance != null)
        {
            ModificatorsManager.Instance.TradeCurseProfitMult /= TradeCurseProfitMult;
            ModificatorsManager.Instance.TradeBlessProfitMult /= TradeBlessProfitMult;
        }
    }
}