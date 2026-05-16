
public class SetDifficultyTimeSpeedModificator : AbstractModificator
{
    public float DifficultyTimeSpeedMult = 1f;

    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

        DifficultyManager.Instance.TimeSpeedMultiplier *= DifficultyTimeSpeedMult;
    }

    public override void OnModificatorRemoved()
    {
        base.OnModificatorRemoved();

        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.Instance.TimeSpeedMultiplier /= DifficultyTimeSpeedMult;
        }
    }
}