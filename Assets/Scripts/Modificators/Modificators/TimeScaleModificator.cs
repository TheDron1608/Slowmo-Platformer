using UnityEngine;

public class TimeScaleModificator : AbstractModificator
{
    public float TimeScaleMultiplier = 1f;

    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

        TimeManager.Instance.CurrentTimeScale *= TimeScaleMultiplier;
    }

    public override void OnModificatorRemoved()
    {
        base.OnModificatorRemoved();

        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.CurrentTimeScale /= TimeScaleMultiplier;
        }
    }
}