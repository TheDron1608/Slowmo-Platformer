using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Build.Pipeline;
using UnityEngine;
using UnityEngine.U2D.IK;

public class ObjectEffectsReceiver : MonoBehaviour
{
    [SerializeField] private List<AbstractEffect> _currentEffects = new();
    private MonoBehaviour _lastSender = null;
    private List<MonoBehaviour> _lastOneSecondSenders = new();

    public event EventHandler<AbstractEffect> OnEffectAdded;
    public event EventHandler<AbstractEffect> OnEffectRemoved;

    public List<AbstractEffect> CurrentEffects
    {
        get => _currentEffects;
    }

    public MonoBehaviour LastSender
    {
        get => _lastSender;
    }

    public List<MonoBehaviour> LastOneSecondsSenders
    {
        get => _lastOneSecondSenders;
    }

    private void Awake()
    {
        OnAwake();
    }

    protected virtual void OnAwake()
    {
        List<AbstractEffect> reapplyEffects = _currentEffects.ToList();
        _currentEffects.Clear();
        foreach (AbstractEffect effect in reapplyEffects)
        {
            ApplyEffect(effect, null);
        }
    }

    private void AddLastEffectSender(MonoBehaviour effectSender)
    {
        if (effectSender != null)
        {
            _lastSender = effectSender;
            _lastOneSecondSenders.Add(effectSender);
            StartCoroutine(AwaitSecondThenRemoveLastOneSecondHitter(effectSender));
        }
    }

    private IEnumerator AwaitSecondThenRemoveLastOneSecondHitter(MonoBehaviour hitter)
    {
        yield return new WaitForSeconds(1f);
        _lastOneSecondSenders.Remove(hitter);
    }


    public void ApplyEffect(AbstractEffect effect, MonoBehaviour sender)
    {
        if (ApplyCondition(effect, sender))
        {
            AbstractEffect newEffect = Instantiate(effect, transform);
            _currentEffects.Add(newEffect);
            if (newEffect is AbstractEffectWithSender effectWithsender)
            {
                effectWithsender.ApplySender(sender);
            }

            if (sender != null)
            {
                AddLastEffectSender(sender);
            }

            OnEffectAdded?.Invoke(this, newEffect);
        }
        else if (effect.AlternativeCharacterEffectIfResisted != null && !effect.AlternativeCharacterEffectIfResisted.Equals(effect))
        {
            ApplyEffect(effect.AlternativeCharacterEffectIfResisted, sender);
        }
    }

    protected virtual bool ApplyCondition(AbstractEffect effect, MonoBehaviour sender)
    {
        return effect.ApplyCondition(this, sender);
    }

    public void ApplyEffect(List<AbstractEffect> effects, MonoBehaviour sender)
    {
        effects.Sort();

        for (int i = 0; i < effects.Count; i++)
        {
            ApplyEffect(effects[i], sender);
        }
    }

    public void RemoveEffect<T>() where T : AbstractEffect
    {
        for (int i = 0; i < _currentEffects.Count; i++)
        {
            if (_currentEffects[i] is T)
            {
                if (_currentEffects[i].IsDestroyed()) continue;

                OnEffectRemoved?.Invoke(this, _currentEffects[i]);
                GameObject.Destroy(_currentEffects[i].gameObject);
                _currentEffects.RemoveAt(i);
                i--;
            }
        }
    }

    public void RemoveEffect(AbstractEffect effect)
    {
        if (effect.IsDestroyed()) return;

        for (int i = 0; i < _currentEffects.Count; i++)
        {
            if (_currentEffects[i].Equals(effect))
            {
                OnEffectRemoved?.Invoke(this, _currentEffects[i]);
                GameObject.Destroy(_currentEffects[i].gameObject);
                _currentEffects.RemoveAt(i);
                i--;
            }
        }
    }

    public void RemoveEffect(List<AbstractEffect> effects)
    {
        if (effects.Count == 0) return;
        for (int i = 0; i < _currentEffects.Count; i++)
        {
            RemoveEffect(effects[i]);
        }
    }

    public bool GetHasEffect<T>() where T : AbstractEffect
    {
        for (int i = 0; i < _currentEffects.Count; i++)
        {
            if (_currentEffects[i] is T)
            {
                return true;
            }
        }
        return false;
    }

    public T GetEffect<T>() where T : AbstractEffect
    {
        for (int i = 0; i < _currentEffects.Count; i++)
        {
            if (_currentEffects[i] is T)
            {
                return _currentEffects[i] as T;
            }
        }
        return null;
    }

    public bool TryGetEffect<T>(out T effect) where T : AbstractEffect
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

    public List<T> GetEffects<T>() where T : AbstractEffect
    {
        List<T> result = new();

        for (int i = 0; i < _currentEffects.Count; i++)
        {
            if (_currentEffects[i] is T tEffect)
            {
                result.Add(tEffect);
            }
        }
        return result;
    }

    public bool GetHasImmuneToEffect(AbstractEffect effect)
    {
        foreach (EffectImmunity immunity in GetEffects<EffectImmunity>())
        {
            if (immunity.ImmuneTo.Equals(effect))
            {
                return true;
            }
        }

        return false;
    }
}
