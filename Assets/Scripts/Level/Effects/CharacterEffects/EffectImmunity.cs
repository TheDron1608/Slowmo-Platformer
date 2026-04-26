using Unity.VisualScripting;

public class EffectImmunity : AbstractEffect
{
    public AbstractEffect ImmuneTo;

    public virtual bool GetIsImmuneTo(AbstractEffect effect)
    {
        return !gameObject.IsDestroyed() && ImmuneTo.Equals(effect);
    }
}
