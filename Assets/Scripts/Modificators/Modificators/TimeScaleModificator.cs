using UnityEngine;

public class TimeScaleModificator : AbstractModificator
{
    public float TimeScaleMultiplier = 1f;

    public override void OnLevelPreGenerated()
    {
        base.OnLevelPreGenerated();

        Time.timeScale *= TimeScaleMultiplier;
        if (TimeScaleMultiplier < 0f)
        {
            Time.fixedDeltaTime *= TimeScaleMultiplier;
        }
    }

    public override void OnLevelFinished()
    {
        base.OnLevelFinished();

        Time.timeScale /= TimeScaleMultiplier;
        if (TimeScaleMultiplier < 0f)
        {
            Time.fixedDeltaTime /= TimeScaleMultiplier;
        }
    }
}