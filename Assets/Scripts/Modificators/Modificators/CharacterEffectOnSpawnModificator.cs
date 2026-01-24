using System.Collections.Generic;
using UnityEngine;

public class CharacterEffectOnSpawnModificator : AbstractCharactersModificator
{
    public List<AbstractEffect> Effects;

    protected override void OnCharacterAffected(CharacterComponentsManager character)
    {
        character.CharacterEffectsReceiver.ApplyEffect(Effects, null, ModificatorMultiplier, true);
    }

    protected override void OnCharacterRemovedAffect(CharacterComponentsManager character)
    {
        foreach (AbstractEffect effect in Effects)
        {
            character.CharacterEffectsReceiver.RemoveEffect(effect);
        }
    }
}