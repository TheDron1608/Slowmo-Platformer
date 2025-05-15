using UnityEngine;

/// <summary>
/// will await ApplySender invoke to apply effects
/// </summary>
public abstract class AbstractCharacterEffectWithSender : AbstractEffectWithSender, ICharacterEffect
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
        _affectedCharacter = GetComponent<AbstractCharacterComponent>().CharComponents;
    }
}
