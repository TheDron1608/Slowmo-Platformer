
public class RestrictSellCursesThisDifficulty : AbstractModificator
{
    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

        DifficultyManager.Instance.OnDifficultyIncreased += Instance_OnDifficultyIncreased;

        ModificatorsManager.Instance.CanSellCurses = false;
    }

    private void Instance_OnDifficultyIncreased(object sender, DifficultyManager.DifficultyStage e)
    {
        DisabledModificator = true;
    }

    public override void OnModificatorRemoved()
    {
        base.OnModificatorRemoved();

        if (ModificatorsManager.Instance != null)
        {
            ModificatorsManager.Instance.CanSellCurses = true;
        }

        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.Instance.OnDifficultyIncreased -= Instance_OnDifficultyIncreased;
        }
    }
}