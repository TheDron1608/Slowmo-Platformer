
public class MidCurseOnlyModificator : AbstractModificator
{
    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.Instance.MidCursesOnly = true;
        }
    }

    public override void OnModificatorRemoved()
    {
        base.OnModificatorRemoved();

        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.Instance.MidCursesOnly = false;
        }
    }
}