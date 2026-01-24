using System.Collections.Generic;
using System.Linq;
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

    public override List<AbstractEffect> ApplyEffect(List<AbstractEffect> effects, MonoBehaviour sender, float effectMultiplier = 1, bool ignoreDeflection = false)
    {
        if (
            !ignoreDeflection &&
            sender != null && 
            sender.TryGetComponent(out ObjectEffectsReceiver senderEffectsReceiver) && 
            CharComponents.CharacterEffectsReceiver.CounterEffectsOnApplier.Count > 0)
        {
            senderEffectsReceiver.ApplyEffect(CharComponents.CharacterEffectsReceiver.CounterEffectsOnApplier, this, 1f, true);
            if (
                sender == null ||
                sender.IsDestroyed() ||
                (sender.TryGetComponent(out AbstractProjectile projectileSender) && projectileSender.WasDeflectedThisFrame)
                )
            {
                return new List<AbstractEffect>(0);
            }
        }
        return base.ApplyEffect(effects, sender, effectMultiplier);
    }

    public override AbstractEffect ApplyEffect(AbstractEffect effect, MonoBehaviour sender, float effectMultiplier = 1f, bool ignoreDeflection = false)
    {
        if (effect == null) return null;

        if (effect.GetSelfIncludeIncomingEffects().All(subEffect => subEffect is IEntireCharacterEffect || subEffect is IDelayedEffect))
        {
            return GetComponent<CharacterPart>().CharComponents.CharacterEffectsReceiver.ApplyEffect(effect, sender, effectMultiplier, ignoreDeflection);
        }
        else
        {
            return base.ApplyEffect(effect, sender, effectMultiplier, ignoreDeflection);
        }
    }

    public override bool ApplyCondition(AbstractEffect effect, MonoBehaviour sender)
    {
        return
            base.ApplyCondition(effect, sender) &&
            (
                !TryGetComponent(out CharacterLimbPart limbPart) ||
                limbPart.GetEquipedAtParts().All(
                    (equpmentPart) => equpmentPart.CharPartEffectsReceiver.ApplyCondition(effect, sender)
                )
            );
    }

    public override void RemoveEffect<T>()
    {
        if (typeof(T).IsSubclassOf(typeof(IEntireCharacterEffect)))
        {
            CharComponents.CharacterEffectsReceiver.RemoveEffect<T>();
        }
        else
        {
            base.RemoveEffect<T>();
        }
    }

    public override T GetEffect<T>(bool includeIncomingEffects = false)
    {
        return base.GetEffect<T>(includeIncomingEffects) ?? CharComponents.CharacterEffectsReceiver.GetSelfEffect<T>(includeIncomingEffects);
    }
    public T GetSelfEffect<T>(bool includeIncomingEffects = false)
    {
        return base.GetEffect<T>(includeIncomingEffects);
    }

    public override List<T> GetEffects<T>(bool includeIncomingEffects = false)
    {
        return NumberMath.MergeLists(base.GetEffects<T>(includeIncomingEffects), CharComponents.CharacterEffectsReceiver.GetSelfEffects<T>(includeIncomingEffects));
    }
    public List<T> GetSelfEffects<T>(bool includeIncomingEffects = false)
    {
        return base.GetEffects<T>(includeIncomingEffects);
    }

    public override bool GetHasEffect<T>(bool includeIncomingEffects = false)
    {
        return base.GetHasEffect<T>() || CharComponents.CharacterEffectsReceiver.GetHasSelfEffect<T>();
    }
    public bool GetHasSelfEffect<T>(bool includeIncomingEffects = false)
    {
        return base.GetHasEffect<T>();
    }

    public override bool GetHasEffect(AbstractEffect effect, bool includeIncomingEffects = false)
    {
        return base.GetHasEffect(effect, includeIncomingEffects) || CharComponents.CharacterEffectsReceiver.GetHasSelfEffect(effect, includeIncomingEffects);
    }
    public bool GetHasSelfEffect(AbstractEffect effect, bool includeIncomingEffects = false)
    {
        return base.GetHasEffect(effect, includeIncomingEffects);
    }

    public override bool TryGetEffect<T>(out T effect)
    {
        return base.TryGetEffect(out effect) || CharComponents.CharacterEffectsReceiver.TryGetSelfEffect<T>(out effect);
    }
    public bool TryGetSelfEffect<T>(out T effect)
    {
        return base.TryGetEffect(out effect);
    }

    public override bool TryGetEffect<T>(out T effect, out AbstractEffect incomingEffectOwner, bool includeIncomingEffects = false)
    {
        return 
            base.TryGetEffect(out effect, out incomingEffectOwner, includeIncomingEffects) || 
            CharComponents.CharacterEffectsReceiver.TryGetSelfEffect<T>(out effect, out incomingEffectOwner, includeIncomingEffects);
    }
    public bool TryGetSelfEffect<T>(out T effect, out AbstractEffect incomingEffectOwner, bool includeIncomingEffects = false)
    {
        return base.TryGetEffect(out effect, out incomingEffectOwner, includeIncomingEffects);
    }
}
