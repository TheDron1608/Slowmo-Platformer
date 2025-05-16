using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CharacterEffectsReceiver : ObjectEffectsReceiver
{
    private CharacterComponentsManager _charComponents;

    protected override void OnAwake()
    {
        base.OnAwake();
        if (!TryGetComponent(out _charComponents)) throw new UnityException("CharacterComponentsManager component not found at " + gameObject.name);
    }

    public void ApplyEffect(AbstractEffect effect, MonoBehaviour sender, CharacterPart affectedLimb)
    {
        if (affectedLimb != null && effect is ICharacterPartEffect)
        {
            if (LimbApplyCondition(effect, sender, affectedLimb))
            {
                affectedLimb.CharPartEffectsReceiver.ApplyEffect(effect, sender);
            }
        }
        else
        {
            ApplyEffect(effect, sender);
        }
    }

    private bool LimbApplyCondition(AbstractEffect effect, MonoBehaviour sender, CharacterPart affectedLimb)
    {
        return 
            ApplyCondition(effect, sender) &&
            affectedLimb.CharPartEffectsReceiver.ApplyCondition(effect, sender) &&
            NumberMath.GetAllListItemsAreValidByCondition(
                _charComponents.CharacterPartsManager.GetCharacterPartEquipment(affectedLimb),
                (equpmentPart) => equpmentPart.CharPartEffectsReceiver.ApplyCondition(effect, sender)
                );
    }

    public void ApplyEffect(List<AbstractEffect> effects, MonoBehaviour sender, CharacterPart affectedLimb)
    {
        effects.Sort();

        for (int i = 0; i < effects.Count; i++)
        {
            ApplyEffect(effects[i], sender, affectedLimb);
        }
    }


    public void RemoveEffect<T>(CharacterPart affectedLimb) where T : AbstractEffect
    {
        affectedLimb.CharPartEffectsReceiver.RemoveEffect<T>();
    }

    public void RemoveEffect(AbstractEffect effect, CharacterPart affectedLimb)
    {
        affectedLimb.CharPartEffectsReceiver.RemoveEffect(effect);
    }

    public void RemoveEffect(List<AbstractEffect> effects, CharacterPart affectedLimb)
    {
        affectedLimb.CharPartEffectsReceiver.RemoveEffect(effects);
    }

    public bool GetHasEffect<T>(CharacterPart affectedLimb) where T : AbstractEffect
    {
        return
            GetHasEffect<T>() ||
            affectedLimb.CharPartEffectsReceiver.GetHasEffect<T>() ||
            NumberMath.GetAnyListItemsIsValidByCondition(
                _charComponents.CharacterPartsManager.GetCharacterPartEquipment(affectedLimb),
                (equpmentPart) => equpmentPart.CharPartEffectsReceiver.GetHasEffect<T>()
                );
    }

    public T GetEffect<T>(CharacterPart affectedLimb) where T : AbstractEffect
    {
        return 
            GetEffect<T>() ?? 
            affectedLimb.CharPartEffectsReceiver.GetEffect<T>() ??
            NumberMath.GetListCallbackReturnValueOfListItemsTilNotNull(
                _charComponents.CharacterPartsManager.GetCharacterPartEquipment(affectedLimb),
                (equpmentPart) => equpmentPart.CharPartEffectsReceiver.GetEffect<T>()
                );
    }

    public bool TryGetEffect<T>(out T effect, CharacterPart affectedLimb) where T : AbstractEffect
    {
        if (
            TryGetEffect(out effect) ||
            affectedLimb.CharPartEffectsReceiver.TryGetEffect(out effect)
            )
        {
            return true;
        }
        else
        {
            foreach (CharacterEquipmentPart charPartManager in _charComponents.CharacterPartsManager.GetCharacterPartEquipment(affectedLimb))
            {
                if (charPartManager.CharPartEffectsReceiver.TryGetEffect(out effect)) return true;
            }
            return false;
        }
    }

    public List<T> GetEffects<T>(CharacterPart affectedLimb) where T : AbstractEffect
    {
        List<T> result = GetEffects<T>();
        result.AddRange(affectedLimb.CharPartEffectsReceiver.GetEffects<T>());
        foreach (CharacterEquipmentPart charPartManager in _charComponents.CharacterPartsManager.GetCharacterPartEquipment(affectedLimb))
        {
            result.AddRange(charPartManager.CharPartEffectsReceiver.GetEffects<T>());
        }

        return result;
    }

    public bool GetHasImmuneToEffect(AbstractEffect effect, CharacterPart affectedLimb)
    {
        return 
            GetHasImmuneToEffect(effect) && 
            affectedLimb.CharPartEffectsReceiver.GetHasImmuneToEffect(effect) &&
            NumberMath.GetAnyListItemsIsValidByCondition(
                _charComponents.CharacterPartsManager.GetCharacterPartEquipment(affectedLimb),
                (equpmentPart) => equpmentPart.CharPartEffectsReceiver.GetHasImmuneToEffect(effect)
                );
    }
}
