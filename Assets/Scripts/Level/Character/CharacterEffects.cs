using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CharacterEffects : AbstractCharacterComponent
{
    private List<AbstractCharacterEffect> _currentEffects = new();

    public event EventHandler<AbstractCharacterEffect> OnEffectAdded;
    public event EventHandler<AbstractCharacterEffect> OnEffectRemoved;

    public void ApplyEffect(AbstractCharacterEffect effect, MonoBehaviour sender = null)
    {
        if (effect.ApplyCondition(CharComponents))
        {
            AbstractCharacterEffect newEffect = Instantiate(effect, transform);
            _currentEffects.Add(newEffect);
            if (newEffect is AbstractCharacterEffectWithSender effectWithsender)
            {
                effectWithsender.ApplySender(sender);
            }
            OnEffectAdded?.Invoke(this, newEffect);
        }
    }

    public void ApplyEffect(List<AbstractCharacterEffect> effects, MonoBehaviour sender = null)
    {
        for (int i = 0; i < effects.Count; i++)
        {
            ApplyEffect(effects[i], sender);
        }
    }

    public void RemoveEffect<T>()
    {
        for (int i = 0; i < _currentEffects.Count; i++)
        {
            if (_currentEffects[i] is T)
            {
                if (_currentEffects[i].IsDestroyed()) continue;

                OnEffectRemoved?.Invoke(this, _currentEffects[i]);
                GameObject.Destroy(_currentEffects[i].gameObject);
                _currentEffects.RemoveAt(i);
            }
        }
    }

    public void RemoveEffect(AbstractCharacterEffect effect)
    {
        if (effect.IsDestroyed()) return;

        OnEffectRemoved?.Invoke(this, effect);
        _currentEffects.Remove(effect);
        GameObject.Destroy(effect.gameObject);
    }

    public void RemoveEffect(System.Type effectType)
    {
        for (int i = 0; i < _currentEffects.Count; i++)
        {
            if (_currentEffects[i].GetType() == effectType)
            {
                if (_currentEffects[i].IsDestroyed()) continue;

                OnEffectRemoved?.Invoke(this, _currentEffects[i]);
                GameObject.Destroy(_currentEffects[i].gameObject);
                _currentEffects.RemoveAt(i);
            }
        }
    }

    public bool GetHasEffect<T>()
    {
        for (int i = 0; i < _currentEffects.Count;i++)
        {
            if (_currentEffects[i] is T)
            {
                return true;
            }
        }
        return false;
    }
}
