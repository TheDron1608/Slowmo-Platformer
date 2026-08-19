using System.Collections.Generic;
using UnityEngine;

public class CharacterMultiplierableCounterEffectsOnApplierModificator : AbstractCharactersModificator
{
    public List<AbstractEffect> CounterEffectsOnApplier;
    public List<AbstractEffect> AltCountEffectOnApplierOnInvertTeam;

    protected override void OnCharacterAffected(CharacterComponentsManager character)
    {
        character.CharacterEffectsReceiver.CounterEffectsOnApplier
            .AddRange(InvertTeam && AltCountEffectOnApplierOnInvertTeam.Count > 0 ? AltCountEffectOnApplierOnInvertTeam : CounterEffectsOnApplier);
    }

    protected override void OnCharacterRemovedAffect(CharacterComponentsManager character)
    {
        foreach (AbstractEffect effect in InvertTeam && AltCountEffectOnApplierOnInvertTeam.Count > 0 ? AltCountEffectOnApplierOnInvertTeam : CounterEffectsOnApplier)
        {
            character.CharacterEffectsReceiver.CounterEffectsOnApplier.Remove(effect);
        }
    }
}