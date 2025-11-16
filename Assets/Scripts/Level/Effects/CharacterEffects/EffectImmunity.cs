using UnityEngine;

public class EffectImmunity : AbstractEffect
{
    public AbstractEffect ImmuneTo;

    public virtual bool GetIsImmuneTo(AbstractEffect effect)
    {
        return ImmuneTo.Equals(effect);
    }
}
