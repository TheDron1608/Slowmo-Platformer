
using System.Linq;

public class SelfApplyEffectOnBlock : AbstractSelfApplyEffectOnAnotherEffectSended
{
    protected override bool EffectIsValidToTriggerCondition(AbstractEffect effect)
    {
        return effect.GetSelfIncludeIncomingEffects().Any(e => e is AbstractMeleeProjectileDeflection || e is AbstractRangedProjectileDeflection);
    }
}