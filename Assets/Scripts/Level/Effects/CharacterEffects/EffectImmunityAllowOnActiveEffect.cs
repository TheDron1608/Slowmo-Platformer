using System.Collections.Generic;
using System.Linq;

public class EffectImmunityAllowOnActiveEffect : EffectImmunity
{
    public List<AbstractEffect> AllowOnActiveEffects = new();
    public bool IncludeIncomingEffects = true;

    public override bool GetIsImmuneTo(AbstractEffect effect)
    {
        return
            base.GetIsImmuneTo(effect) &&
            !NumberMath.GetListContainsAnyItemOfAnotherList(AffectedObject.GetEffects<AbstractEffect>(IncludeIncomingEffects), AllowOnActiveEffects);
    }
}
