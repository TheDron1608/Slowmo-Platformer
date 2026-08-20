using System.Collections.Generic;
using UnityEngine;

public class CharacterMultiplierableEffectsOnDeathReplaceDefaultModificator : CharacterMultiplierableEffectsOnDeathModificator
{
    private AbstractEffect oldEffect = null;

    protected override void OnCharacterAffected(CharacterComponentsManager character)
    {
        if (character.CharacterHealth.EffectsOnLethal.Count > 0)
        {
            oldEffect = character.CharacterHealth.EffectsOnLethal[0];
            character.CharacterHealth.EffectsOnLethal.Remove(character.CharacterHealth.EffectsOnLethal[0]);
        }

        base.OnCharacterAffected(character);
    }

    protected override void OnCharacterRemovedAffect(CharacterComponentsManager character)
    {
        character.CharacterHealth.EffectsOnLethal.Insert(0, oldEffect);

        base.OnCharacterRemovedAffect(character);
    }
}