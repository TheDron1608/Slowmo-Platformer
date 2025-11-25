using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(6)]
public class CharacterEffectsReceiver : ObjectEffectsReceiver
{
    private CharacterComponentsManager _charComponents;

    protected override void OnAwake()
    {
        if (!TryGetComponent(out _charComponents)) throw new UnityException("CharacterComponentsManager component not found at " + gameObject.name);
        base.OnAwake();
    }

    public void ApplyEffect(AbstractEffect effect, MonoBehaviour sender, CharacterPart affectedLimb)
    {
        if (affectedLimb != null && !(effect is IEntireCharacterEffect))
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

    public override T GetEffect<T>(bool includeIncomingEffects = false)
    {
        return base.GetEffect<T>(includeIncomingEffects) ??
            NumberMath.GetListCallbackReturnValueOfListItemsTilNotNull<T, CharacterPart>(
                _charComponents.CharacterPartsManager.CharacterParts, 
                (CharacterPart part) => part.CharPartEffectsReceiver.GetSelfEffect<T>(includeIncomingEffects)
            );
    }
    public T GetSelfEffect<T>(bool includeIncomingEffects = false)
    {
        return base.GetEffect<T>(includeIncomingEffects);
    }

    public override List<T> GetEffects<T>(bool includeIncomingEffects = false)
    {
        List<T> result = base.GetEffects<T>(includeIncomingEffects);
        foreach (CharacterPart part in _charComponents.CharacterPartsManager.CharacterParts)
        {
            result.AddRange(part.CharPartEffectsReceiver.GetSelfEffects<T>(includeIncomingEffects));
        }
        return result;
    }
    public List<T> GetSelfEffects<T>(bool includeIncomingEffects = false)
    {
        return base.GetEffects<T>(includeIncomingEffects);
    }

    public override bool GetHasEffect(AbstractEffect effect, bool includeIncomingEffects = false)
    {
        return base.GetHasEffect(effect, includeIncomingEffects) ||
            NumberMath.GetAnyListItemsIsValidByCondition(
                _charComponents.CharacterPartsManager.CharacterParts,
                (CharacterPart part) => part.CharPartEffectsReceiver.GetHasSelfEffect(effect, includeIncomingEffects)
            );
    }
    public bool GetHasSelfEffect(AbstractEffect effect, bool includeIncomingEffects = false)
    {
        return base.GetHasEffect(effect, includeIncomingEffects);
    }

    public override bool GetHasEffect<T>(bool includeIncomingEffects = false)
    {
        return base.GetHasEffect<T>(includeIncomingEffects) ||
            NumberMath.GetAnyListItemsIsValidByCondition(
                _charComponents.CharacterPartsManager.CharacterParts,
                (CharacterPart part) => part.CharPartEffectsReceiver.GetHasSelfEffect<T>()
            );
    }
    public bool GetHasSelfEffect<T>(bool includeIncomingEffects = false)
    {
        return base.GetHasEffect<T>(includeIncomingEffects);
    }

    public override bool TryGetEffect<T>(out T effect)
    {
        if (base.TryGetEffect(out effect)) return true;
        foreach (CharacterPart part in _charComponents.CharacterPartsManager.CharacterParts)
        {
            if (part.CharPartEffectsReceiver.TryGetSelfEffect<T>(out effect)) return true;
        }
        effect = default;
        return false;
    }
    public bool TryGetSelfEffect<T>(out T effect)
    {
        return base.TryGetEffect(out effect);
    }

    public override bool TryGetEffect<T>(out T effect, out AbstractEffect incomingEffectOwner, bool includeIncomingEffects = false)
    {
        if (base.TryGetEffect(out effect, out incomingEffectOwner, includeIncomingEffects)) return true;
        foreach (CharacterPart part in _charComponents.CharacterPartsManager.CharacterParts)
        {
            if (part.CharPartEffectsReceiver.TryGetSelfEffect<T>(out effect, out incomingEffectOwner, includeIncomingEffects)) return true;
        }
        effect = default;
        return false;
    }
    public bool TryGetSelfEffect<T>(out T effect, out AbstractEffect incomingEffectOwner, bool includeIncomingEffects = false)
    {
        return base.TryGetEffect(out effect, out incomingEffectOwner, includeIncomingEffects);
    }

    public override Material EffectMaterial
    {
        get
        {
            foreach (var charPart in _charComponents.CharacterPartsManager.CharacterParts)
            {
                if (charPart.GetComponent<CharacterLimbPart>() != null && charPart.TryGetComponent(out SpriteRenderer charPartRenderer))
                {
                    return charPartRenderer.sharedMaterial;
                }
            }

            return null;
        }
        protected set
        {
            foreach (var charPart in _charComponents.CharacterPartsManager.CharacterParts)
            {
                if (charPart.GetComponent<CharacterLimbPart>() != null && charPart.TryGetComponent(out DynamicMaterial dynamicMaterial))
                {
                    dynamicMaterial.OverrideMaterial = value;
                }
            }
        }
    }
}
