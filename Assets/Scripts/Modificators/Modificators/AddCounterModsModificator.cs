
public class AddCounterModsModificator : AbstractModificator
{
    public float AddBlessPickCounterMods = 0f;
    public float AddCursePickCounterMods = 0f;
    public float AddDifficultyCursePickCounterMods = 0f;

    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

        ModificatorsManager.Instance.BlessPickCounterMods += AddBlessPickCounterMods;
        ModificatorsManager.Instance.CursePickCounterMods += AddCursePickCounterMods;
        ModificatorsManager.Instance.DifficultyCursePickCounterMods += AddDifficultyCursePickCounterMods;
    }

    public override void OnModificatorRemoved()
    {
        base.OnModificatorRemoved();

        if (ModificatorsManager.Instance != null)
        {
            ModificatorsManager.Instance.BlessPickCounterMods -= AddBlessPickCounterMods;
            ModificatorsManager.Instance.CursePickCounterMods -= AddCursePickCounterMods;
            ModificatorsManager.Instance.DifficultyCursePickCounterMods -= AddDifficultyCursePickCounterMods;
        }
    }
}