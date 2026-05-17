public class SetCanSkipTradesModificator : AbstractModificator
{
    public bool CanSkipBlessPick = true;
    public bool CanSkipCursePick = true;

    private bool _oldCanSkipBlessPick;
    private bool _oldCanSkipCursePick;

    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

        _oldCanSkipBlessPick = ModificatorsManager.Instance.CanSkipBlessPick;
        _oldCanSkipCursePick = ModificatorsManager.Instance.CanSkipCursePick;

        ModificatorsManager.Instance.CanSkipBlessPick = CanSkipBlessPick;
        ModificatorsManager.Instance.CanSkipCursePick = CanSkipCursePick;   
    }

    public override void OnModificatorRemoved()
    {
        base.OnModificatorRemoved();

        if (ModificatorsManager.Instance != null)
        {
            ModificatorsManager.Instance.CanSkipBlessPick = _oldCanSkipBlessPick;
            ModificatorsManager.Instance.CanSkipCursePick = _oldCanSkipCursePick;
        }
    }
}