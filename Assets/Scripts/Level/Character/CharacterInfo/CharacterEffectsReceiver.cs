using System.Collections.Generic;
using Unity.VisualScripting;
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

    public void ApplyEffect(List<AbstractEffect> effects, MonoBehaviour sender, CharacterPart affectedLimb, float effectMultplier = 1f, bool ignoreDeflection = false)
    {
        if (affectedLimb != null && !(effects is IEntireCharacterEffect))
        {
            affectedLimb.CharPartEffectsReceiver.ApplyEffect(effects, sender, effectMultplier, ignoreDeflection);
        }
        else
        {
            ApplyEffect(effects, sender, effectMultplier, ignoreDeflection);
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
            affectedLimb.CharPartEffectsReceiver.GetHasEffect<T>();
    }

    public T GetEffect<T>(CharacterPart affectedLimb) where T : AbstractEffect
    {
        return
            GetEffect<T>() ??
            affectedLimb.CharPartEffectsReceiver.GetEffect<T>();
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
            affectedLimb.CharPartEffectsReceiver.GetHasImmuneToEffect(effect);
    }

    public override T GetEffect<T>(bool includeIncomingEffects = false)
    {
        T result = base.GetEffect<T>(includeIncomingEffects);
        if (result != null) return result;
        foreach (CharacterPart part in _charComponents.CharacterPartsManager.CharacterParts)
        {
            result = part.CharPartEffectsReceiver.GetSelfEffect<T>(includeIncomingEffects);
            if (result != null) return result;
        }
        return default;
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
        return base.GetHasEffect(effect, includeIncomingEffects) || GetAnyPartHasSelfEffect(effect, includeIncomingEffects);
    }
    private bool GetAnyPartHasSelfEffect(AbstractEffect effect, bool includeIncomingEffects = false)
    {
        foreach (var charPart in _charComponents.CharacterPartsManager.CharacterParts)
        {
            if (charPart.CharPartEffectsReceiver.GetHasSelfEffect(effect, includeIncomingEffects)) return true;
        }
        return false;
    }

    public bool GetHasSelfEffect(AbstractEffect effect, bool includeIncomingEffects = false)
    {
        return base.GetHasEffect(effect, includeIncomingEffects);
    }

    public override bool GetHasEffect<T>(bool includeIncomingEffects = false)
    {
        return base.GetHasEffect<T>(includeIncomingEffects) || GetAnyPartHasSelfEffect<T>(includeIncomingEffects);
    }
    private bool GetAnyPartHasSelfEffect<T>(bool includeIncomingEffects = false)
    {
        foreach (var charPart in _charComponents.CharacterPartsManager.CharacterParts)
        {
            if (charPart.CharPartEffectsReceiver.GetHasSelfEffect<T>(includeIncomingEffects)) return true;
        }
        return false;
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
                if (charPart.TryGetComponent(out CharacterLimbPart clp) && charPart.TryGetComponent(out SpriteRenderer charPartRenderer))
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
                if (!charPart.IsDestroyed() && charPart.TryGetComponent(out CharacterLimbPart clp) && charPart.TryGetComponent(out DynamicMaterial dynamicMaterial))
                {
                    dynamicMaterial.OverrideMaterial = value;
                }
            }
        }
    }
}
