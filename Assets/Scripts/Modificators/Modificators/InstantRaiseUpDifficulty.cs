
public class InstantRaiseUpDifficulty : AbstractModificator
{
    public DifficultyManager.DifficultyStage AddStageIfSkipLast;

    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

        if (DifficultyManager.Instance.CurrentDifficulty.Value != AddStageIfSkipLast)
        {
            if (DifficultyManager.Instance.CurrentDifficulty.Next?.Next == null)
            {
                DifficultyManager.Instance.Difficulties.AddBefore(DifficultyManager.Instance.Difficulties.Last, AddStageIfSkipLast);
            }

            DifficultyManager.Instance.ForceRaiseUpDifficulty();
        }
    }

    public override void OnModificatorRemoved()
    {
        base.OnModificatorRemoved();

        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.Instance.Difficulties.Remove(AddStageIfSkipLast);
        }
    }
}