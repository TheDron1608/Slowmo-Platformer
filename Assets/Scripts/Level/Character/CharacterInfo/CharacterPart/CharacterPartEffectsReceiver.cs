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

    public override T GetEffect<T>()
    {
        return base.GetEffect<T>() ?? CharComponents.CharacterEffectsReceiver.GetEffect<T>();
    }

    public override List<T> GetEffects<T>()
    {
        return NumberMath.MergeLists(base.GetEffects<T>(), CharComponents.CharacterEffectsReceiver.GetEffects<T>());
    }

    public override bool GetHasEffect<T>()
    {
        return base.GetHasEffect<T>() || CharComponents.CharacterEffectsReceiver.GetHasEffect<T>();
    }

    public override bool TryGetEffect<T>(out T effect)
    {
        return base.TryGetEffect(out effect) || CharComponents.CharacterEffectsReceiver.TryGetEffect<T>(out effect);
    }
}
