using UnityEngine;

/// <summary>
/// removes all effects of same type when applied
/// </summary>
public abstract class AbstractOverwritingCharacterEffect : AbstractOverwritingEffect, ICharacterEffect
{
    private CharacterComponentsManager _affectedCharacter;

    public CharacterComponentsManager AffectedCharacter
    {
        get => _affectedCharacter;
        private set => _affectedCharacter = value;
    }

    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return
            base.ApplyCondition(affectWho, sender) &&
            affectWho.GetComponent<AbstractCharacterComponent>() != null;
    }

    protected override void OnApply()
    {
        base.OnApply();
        _affectedCharacter = transform.parent.GetComponent<AbstractCharacterComponent>().CharComponents;
    }
}