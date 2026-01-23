using System.Collections.Generic;
using UnityEngine;

public class CharacterMultiplierableCounterEffectsOnApplierModificator : AbstractCharactersModificator
{
    public List<AbstractEffect> CounterEffectsOnApplier;

    protected override void OnCharacterAffected(CharacterComponentsManager character)
    {
        character.CharacterEffectsReceiver.CounterEffectsOnApplier.AddRange(CounterEffectsOnApplier);
    }

    protected override void OnCharacterRemovedAffect(CharacterComponentsManager character)
    {
        foreach (AbstractEffect effect in CounterEffectsOnApplier)
        {
            character.CharacterEffectsReceiver.CounterEffectsOnApplier.Remove(effect);
        }
    }
}