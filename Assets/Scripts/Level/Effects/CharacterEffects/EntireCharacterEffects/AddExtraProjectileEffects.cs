using System.Collections.Generic;
using UnityEngine.TextCore.Text;

public class AddExtraProjectileEffects : AbstractCharacterEffect, IEntireCharacterEffect
{
    public List<AbstractEffect> ExtraProjectileEffects = new();

    protected override void OnApply()
    {
        base.OnApply();

        AffectedCharacter.CharacterAttacking.ExtraProjectileEffects.AddRange(ExtraProjectileEffects);
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        foreach (AbstractEffect effect in ExtraProjectileEffects)
        {
            AffectedCharacter.CharacterAttacking.ExtraProjectileEffects.Remove(effect);
        }
    }
}