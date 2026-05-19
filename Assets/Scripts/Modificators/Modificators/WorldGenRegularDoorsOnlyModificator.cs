using UnityEngine;

public class WorldGenRegularDoorsOnlyModificator : AbstractModificator
{
    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

        WorldGenerationManager.Instance.RegularExitsOnly = true;
    }

    public override void OnModificatorRemoved()
    {
        if (WorldGenerationManager.Instance != null)
        {
            WorldGenerationManager.Instance.RegularExitsOnly = false;
        }
    }
}