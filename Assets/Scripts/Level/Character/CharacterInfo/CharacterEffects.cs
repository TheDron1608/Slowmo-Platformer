using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Build.Pipeline;
using UnityEngine;

public class CharacterEffects : AbstractCharacterComponent
{
    private List<AbstractCharacterEffect> _currentEffects = new();
    private AbstractCharacterComponent _lastHitter = null;
    private List<AbstractCharacterComponent> _lastOneSecondHitters = new();

    public event EventHandler OnRemoved;

    public event EventHandler<AbstractCharacterEffect> OnEffectAdded;
    public event EventHandler<AbstractCharacterEffect> OnEffectRemoved;

    public AbstractCharacterComponent LastHitter
    {
        get => _lastHitter;
    }

    public List<AbstractCharacterComponent> LastOneSecondHitters
    {
        get => _lastOneSecondHitters;
    }

    public bool GetLastHitterIsCharacter(AbstractCharacterComponent character)
    {
        return AbstractCharacterComponent.GetCharacterComponentsEqual(character, _lastHitter);
    }

    public bool GetLastOneSecondHittersContainsCharacter(AbstractCharacterComponent character)
    {
        for (int i = 0; i < _lastOneSecondHitters.Count; i++)
        {
            if (AbstractCharacterComponent.GetCharacterComponentsEqual(_lastOneSecondHitters[i], character))
            {
                return true;
            }
        }
        return false;
    }

    private void AddLastEffectSender(MonoBehaviour effectSender)
    {
        AbstractCharacterComponent filteredSender = null;

        if (effectSender.TryGetComponent(out AbstractProjectile projectile) && projectile.Owner != null)
        {
            filteredSender = projectile.Owner;
        }
        else if (effectSender.TryGetComponent(out AbstractCharacterComponent character))
        {
            filteredSender = character;
        }

        if (filteredSender != null)
        {
            _lastHitter = filteredSender;
            _lastOneSecondHitters.Add(filteredSender);
            StartCoroutine(AwaitSecondThenRemoveLastOneSecondHitter(filteredSender));
        }
    }

    private IEnumerator AwaitSecondThenRemoveLastOneSecondHitter(AbstractCharacterComponent hitter)
    {
        yield return new WaitForSeconds(1f);
        _lastOneSecondHitters.Remove(hitter);
    }


    public void ApplyEffect(AbstractCharacterEffect effect, MonoBehaviour sender, CharacterPart receiverPart)
    {
        if (effect.ApplyCondition(CharComponents))
        {
            AbstractCharacterEffect newEffect = Instantiate(effect, transform);
            _currentEffects.Add(newEffect);
            if (newEffect is AbstractCharacterEffectWithSender effectWithsender)
            {
                effectWithsender.ApplySender(sender, receiverPart);
            }

            if (sender != null)
            {
                AddLastEffectSender(sender);
            }

            OnEffectAdded?.Invoke(this, newEffect);
        }
    }

    public void ApplyEffect(List<AbstractCharacterEffect> effects, MonoBehaviour sender, CharacterPart receiverPart)
    {
        effects.Sort();

        for (int i = 0; i < effects.Count; i++)
        {
            ApplyEffect(effects[i], sender, receiverPart);
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

        for (int i = 0; i < _currentEffects.Count; i++)
        {
            if (_currentEffects[i].GetType() == effect.GetType())
            {
                OnEffectRemoved?.Invoke(this, _currentEffects[i]);
                GameObject.Destroy(_currentEffects[i].gameObject);
                _currentEffects.RemoveAt(i);
                i--;
            }
        }
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

    public void RemoveEffect(List<AbstractCharacterEffect> effects)
    {
        if (effects.Count == 0) return;
        for (int i = 0; i < _currentEffects.Count; i++)
        {
            RemoveEffect(effects[i]);
        }
    }

    public void RemoveEffect(List<System.Type> effects)
    {
        if (effects.Count == 0) return;
        for (int i = 0; i < _currentEffects.Count; i++)
        {
            RemoveEffect(effects[i]);
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

    public AbstractCharacterEffect GetEffect<T>()
    {
        for (int i = 0; i < _currentEffects.Count; i++)
        {
            if (_currentEffects[i] is T)
            {
                return _currentEffects[i];
            }
        }
        return null;
    }

    public bool TryGetEffect<T>(out T effect)
    {
        for (int i = 0; i < _currentEffects.Count; i++)
        {
            if (_currentEffects[i] is T outEffect)
            {
                effect = outEffect;
                return true;
            }
        }
        effect = default;
        return false;
    }
}
