
using System.Linq;
using UnityEngine;

public class SelfApplyEffectOnKill : AbstractSelfApplyEffectOnAnotherEffectSended
{
    protected override bool EffectIsValidToTriggerCondition(AbstractEffect effect)
    {
        return effect.GetSelfIncludeIncomingEffects().Any(e => e is ILethalEffect);
    }
}