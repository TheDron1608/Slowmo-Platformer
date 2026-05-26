using UnityEngine;

public abstract class AbstractCharacterEffect : AbstractEffect, ICharacterEffect
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
            affectWho.TryGetComponent(out AbstractCharacterComponent character);
    }

    protected override void OnApply()
    {
        base.OnApply();
        if (transform.parent.TryGetComponent(out AbstractCharacterComponent character))
        {
            _affectedCharacter = character.CharComponents;
        }
        else
        {
            throw new UnityException("not found AbstractCharacterComponent for character effect");
        }
    }
}
