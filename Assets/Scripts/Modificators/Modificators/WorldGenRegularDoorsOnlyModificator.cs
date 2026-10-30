using UnityEngine;

public class WorldGenRegularDoorsOnlyModificator : AbstractModificator
{
    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

        WorldGenerationManager.Instance.RegularExitsOnly = true;
        ScoreManager.Instance.ResetComboDelay *= 99999f;
    }

    public override void OnModificatorRemoved()
    {
        if (WorldGenerationManager.Instance != null)
        {
            WorldGenerationManager.Instance.RegularExitsOnly = false;
        }
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetComboDelay /= 99999f;
        }
    }
}