using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CharacterEffects : AbstractCharacterComponent
{
    private List<AbstractCharacterEffect> _currentEffects = new();

    public void ApplyEffect(AbstractCharacterEffect effect)
    {
        if (effect.ApplyCondition(CharComponents))
        {
            AbstractCharacterEffect newEffect = Instantiate(effect, transform);
            _currentEffects.Add(newEffect);
        }
    }

    public void ApplyEffect(List<AbstractCharacterEffect> effects)
    {
        for (int i = 0; i < effects.Count; i++)
        {
            ApplyEffect(effects[i]);
        }
    }

    public void RemoveEffect<T>()
    {
        for (int i = 0; i < _currentEffects.Count; i++)
        {
            if (_currentEffects[i] is T)
            {
                GameObject.Destroy(_currentEffects[i].gameObject);
                _currentEffects.RemoveAt(i);
            }
        }
    }

    public void RemoveEffect(AbstractCharacterEffect effect)
    {
        if (effect.IsDestroyed()) return;

        _currentEffects.Remove(effect);
        GameObject.Destroy(effect.gameObject);
    }

    public void RemoveEffect(System.Type effectType)
    {
        for (int i = 0; i < _currentEffects.Count; i++)
        {
            if (_currentEffects[i].GetType() == effectType)
            {
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
