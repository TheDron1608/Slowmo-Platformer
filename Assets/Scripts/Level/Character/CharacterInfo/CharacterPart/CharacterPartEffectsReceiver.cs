using System.Collections.Generic;
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

    public override T GetEffect<T>(bool includeIncomingEffects = false)
    {
        return base.GetEffect<T>(includeIncomingEffects) ?? CharComponents.CharacterEffectsReceiver.GetEffect<T>(includeIncomingEffects);
    }

    public override List<T> GetEffects<T>(bool includeIncomingEffects = false)
    {
        return NumberMath.MergeLists(base.GetEffects<T>(includeIncomingEffects), CharComponents.CharacterEffectsReceiver.GetEffects<T>(includeIncomingEffects));
    }

    public override bool GetHasEffect<T>(bool includeIncomingEffects = false)
    {
        return base.GetHasEffect<T>() || CharComponents.CharacterEffectsReceiver.GetHasEffect<T>();
    }

    public override bool GetHasEffect(AbstractEffect effect, bool includeIncomingEffects = false)
    {
        return base.GetHasEffect(effect, includeIncomingEffects) || CharComponents.CharacterEffectsReceiver.GetHasEffect(effect, includeIncomingEffects);
    }

    public override bool TryGetEffect<T>(out T effect, bool includeIncomingEffects = false)
    {
        return base.TryGetEffect(out effect, includeIncomingEffects) || CharComponents.CharacterEffectsReceiver.TryGetEffect<T>(out effect, includeIncomingEffects);
    }
}
