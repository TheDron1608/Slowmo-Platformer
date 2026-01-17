using System.Collections.Generic;
using UnityEngine;

public class CharacterMultiplierableEffectsOnDeathReplaceDefaultModificator : CharacterMultiplierableEffectsOnDeathModificator
{
    protected override void OnCharacterAffected(CharacterComponentsManager character)
    {
        for (int i = 0; i < character.CharacterHealth.DefaultEffectsOnLethal.Count; i++)
        {   
            character.CharacterHealth.EffectsOnLethal.Remove(character.CharacterHealth.DefaultEffectsOnLethal[i]);
        }

        base.OnCharacterAffected(character);
    }

    protected override void OnCharacterRemovedAffect(CharacterComponentsManager character)
    {
        foreach (AbstractEffect defaultEffect in character.CharacterHealth.DefaultEffectsOnLethal)
        {
            if (!character.CharacterHealth.EffectsOnLethal.Contains(defaultEffect))
            {
                character.CharacterHealth.EffectsOnLethal.Add(defaultEffect);
            }
        }

        base.OnCharacterRemovedAffect(character);
    }
}