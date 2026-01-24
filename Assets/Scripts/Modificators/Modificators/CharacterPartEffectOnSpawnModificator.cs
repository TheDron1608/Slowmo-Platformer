using System.Collections.Generic;
using UnityEngine;

public class CharacterPartEffectOnSpawnModificator : AbstractCharactersModificator
{
    public List<AbstractEffect> Effects;
    public CharacterPart.PartTypes AffectedPart;

    protected override void OnCharacterAffected(CharacterComponentsManager character)
    {
        CharacterPart part = character.CharacterPartsManager.GetCharacterPart(AffectedPart);

        if (part != null)
        {
            character.CharacterEffectsReceiver.ApplyEffect(Effects, null, part, ModificatorMultiplier, true);
        }
    }

    protected override void OnCharacterRemovedAffect(CharacterComponentsManager character)
    {
        CharacterPart part = character.CharacterPartsManager.GetCharacterPart(AffectedPart);

        if (part != null)
        {
            foreach (AbstractEffect effect in Effects)
            {
                part.CharPartEffectsReceiver.RemoveEffect(effect);
            }
        }
    }
}