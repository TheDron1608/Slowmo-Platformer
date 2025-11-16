using System.Collections.Generic;

public class EffectImmunityAllowOnActiveEffect : EffectImmunity
{
    public List<AbstractEffect> AllowOnActiveEffects = new();

    public override bool GetIsImmuneTo(AbstractEffect effect)
    {
        return base.GetIsImmuneTo(effect) && !NumberMath.GetListContainsAnyItemOfAnotherList(AffectedObject.CurrentEffects, AllowOnActiveEffects);
    }
}
