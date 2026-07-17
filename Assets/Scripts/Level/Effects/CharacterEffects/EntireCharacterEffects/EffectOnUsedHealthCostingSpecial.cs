using UnityEngine;

[AllowEffectWithSenderReceiveNull]
public class EffectOnUsedHealthCostingSpecial : EffectOnUsedSpecial
{
    protected override bool SpecialCondition(AbstractCharacterSpecial special)
    {
        return base.SpecialCondition(special) && special?.HealthCost > 0f;
    }
}
