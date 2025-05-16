using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CharacterEffectsReceiver : ObjectEffectsReceiver
{
    public void ApplyEffect(AbstractEffect effect, MonoBehaviour sender, CharacterPart affectedLimb)
    {
        if (affectedLimb != null && effect is ICharacterLimbEffect)
        {
            affectedLimb.CharPartEffectsReceiver.ApplyEffect(effect, sender);
        }
        else
        {
            ApplyEffect(effect, sender);
        }
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
        return GetHasEffect<T>() || affectedLimb.CharPartEffectsReceiver.GetHasEffect<T>();
    }

    public T GetEffect<T>(CharacterPart affectedLimb) where T : AbstractEffect
    {
        return GetEffect<T>() ?? affectedLimb.CharPartEffectsReceiver.GetEffect<T>();
    }

    public bool TryGetEffect<T>(out T effect, CharacterPart affectedLimb) where T : AbstractEffect
    {
        return TryGetEffect(out effect) || affectedLimb.CharPartEffectsReceiver.TryGetEffect(out effect);
    }

    public List<T> GetEffects<T>(CharacterPart affectedLimb) where T : AbstractEffect
    {
        List<T> result = GetEffects<T>();
        result.AddRange(affectedLimb.CharPartEffectsReceiver.GetEffects<T>());

        return result;
    }

    public bool GetHasImmuneToEffect(AbstractEffect effect, CharacterPart affectedLimb)
    {
        return GetHasImmuneToEffect(effect) && affectedLimb.CharPartEffectsReceiver.GetHasImmuneToEffect(effect);
    }
}
