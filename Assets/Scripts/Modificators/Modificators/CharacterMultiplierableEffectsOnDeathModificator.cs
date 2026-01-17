using System.Collections.Generic;
using UnityEngine;

public class CharacterMultiplierableEffectsOnDeathModificator : AbstractCharactersModificator
{
    public List<AbstractEffect> CharacterEffectsOnDeath;

    protected override void OnCharacterAffected(CharacterComponentsManager character)
    {
        character.CharacterHealth.EffectsOnLethal.AddRange(CharacterEffectsOnDeath);
    }

    protected override void OnCharacterRemovedAffect(CharacterComponentsManager character)
    {
        foreach (AbstractEffect effect in CharacterEffectsOnDeath)
        {
            character.CharacterHealth.EffectsOnLethal.Remove(effect);
        }
    }
}