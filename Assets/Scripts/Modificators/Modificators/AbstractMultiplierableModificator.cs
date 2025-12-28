using UnityEngine;

public abstract class AbstractMultiplierableModificator : AbstractModificator
{
    public float ModificatorMultiplier = 1f;

    public override bool GetEqualType(AbstractModificator other)
    {
        return base.GetEqualType(other) && ModificatorMultiplier == (other as AbstractMultiplierableModificator).ModificatorMultiplier;
    }
}