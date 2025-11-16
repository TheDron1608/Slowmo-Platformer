using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class CharacterPartEffectsReceiver : ObjectEffectsReceiver
{
    public CharacterComponentsManager CharComponents;

    protected override void OnAwake()
    {
        base.OnAwake();
        CharComponents = GetComponentInParent<CharacterComponentsManager>();
    }

    public override void ApplyEffect(AbstractEffect effect, MonoBehaviour sender)
    {
        if (effect is IEntireCharacterEffect)
        {
            GetComponent<CharacterPart>().CharComponents.CharacterEffectsReceiver.ApplyEffect(effect, sender);
        }
        else
        {
            base.ApplyEffect(effect, sender);
        }
    }

    public override bool ApplyCondition(AbstractEffect effect, MonoBehaviour sender)
    {
        return
            base.ApplyCondition(effect, sender) &&
            (
                !TryGetComponent(out CharacterLimbPart limbPart) ||
                NumberMath.GetAllListItemsAreValidByCondition(
                    limbPart.GetEquipedAtParts(),
                    (equpmentPart) => equpmentPart.CharPartEffectsReceiver.ApplyCondition(effect, sender)
                    )
            ) &&
            CharComponents.CharacterEffectsReceiver.ApplyCondition(effect, sender);
    }
}
