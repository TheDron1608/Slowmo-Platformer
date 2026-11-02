
using Unity.VisualScripting;
using UnityEngine;

public class ForceGiveLastHoldable : AbstractCharacterEffect, IEntireCharacterEffect
{
    protected override void OnApply()
    {
        base.OnApply();

        AffectedCharacter.CharacterHolding.ForceGrab(AffectedCharacter.CharacterHolding.LastHoldObject);
        RemoveSelf();
    }

    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return 
            base.ApplyCondition(affectWho, sender) && 
            affectWho.TryGetComponent(out AbstractCharacterComponent character) &&
            character.CharComponents.CharacterHolding.CurrentHoldObject == null &&
            !character.CharComponents.CharacterHolding.LastHoldObject.IsDestroyed();
    }
}