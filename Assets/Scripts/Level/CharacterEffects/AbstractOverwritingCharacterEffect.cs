using UnityEngine;

/// <summary>
/// removes all effects of same type when applied
/// </summary>
public abstract class AbstractOverwritingCharacterEffect : AbstractCharacterEffect
{
    protected override void OnApply()
    {
        base.OnApply();
        AffectedCharacter.CharacterEffects.RemoveEffect(GetType());
    }
}
