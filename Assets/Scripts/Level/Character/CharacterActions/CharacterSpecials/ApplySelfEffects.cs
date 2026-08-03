using NUnit.Framework;
using System.Collections.Generic;

public class ApplySelfEffects : AbstractCharacterSpecial
{
    public List<AbstractEffect> ApplyEffects = new();

    public bool TryApplySelfEffects()
    {
        if (!IsAbleToDoSpecial) return false;

        if (!GetHasEnoughForCost()) return false;

        if (CharComponents.CharacterEffectsReceiver.GetHasEffect(ApplyEffects)) return false;

        CharComponents.CharacterEffectsReceiver.ApplyEffect(ApplyEffects, this);

        SpendCost();
        InvokeUse();

        return true;
    }
}