using System.Collections.Generic;
using UnityEngine;

public class CharacterExtraProjectilesEffectsModificator : AbstractCharactersModificator
{
    public List<AbstractEffect> ExtraProjectileEffects = new();

    protected override void OnCharacterAffected(CharacterComponentsManager character)
    {
        character.CharacterAttacking.ExtraProjectileEffects.AddRange(ExtraProjectileEffects);
    }

    protected override void OnCharacterRemovedAffect(CharacterComponentsManager character)
    {
        foreach (AbstractEffect effect in ExtraProjectileEffects)
        {
            character.CharacterAttacking.ExtraProjectileEffects.Remove(effect);
        }
    }
}