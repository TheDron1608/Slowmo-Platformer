using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ObjectEffectsReceiver : MonoBehaviour
{
    [SerializeField] private List<AbstractEffect> _currentEffects = new();
    private MonoBehaviour _lastSender = null;
    protected Material _defaultEffectMaterial = null;

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
        _defaultEffectMaterial = GetComponentInChildren<SpriteRenderer>()?.sharedMaterial;
    }

    private void AddLastEffectSender(MonoBehaviour effectSender)
    {
        if (effectSender != null)
        {
            _lastSender = effectSender;
        }
    }

    public bool GetCharacterIsLastSender(AbstractCharacterComponent character)
    {
        return
            LastSender != null &&
            (
                (LastSender.TryGetComponent(out AbstractCharacterComponent characterSender) && characterSender.CharComponents == character.CharComponents) ||
                (LastSender.TryGetComponent(out AbstractProjectile projectileSender) && projectileSender.Owner != null && projectileSender.Owner.CharComponents == character.CharComponents) ||
                (LastSender.TryGetComponent(out Holdable holdableSender) && holdableSender.CurrentHolder != null && holdableSender.CurrentHolder.CharComponents == character.CharComponents)
            );
    }

    public void UpdateDefaultEffectMaterialToCurrent()
    {
        _defaultEffectMaterial = GetComponentInChildren<SpriteRenderer>()?.sharedMaterial;
    }

    public virtual void ApplyEffect(AbstractEffect effect, MonoBehaviour sender)
    {
        if (ApplyCondition(effect, sender) && effect.ApplyCondition(this, sender))
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

            if (effect.EffectMaterial != null)
            {
                EffectMaterial = effect.EffectMaterial;
            }

            OnEffectAdded?.Invoke(this, newEffect);
        }
        else if (effect.AlternativeCharacterEffectIfResisted != null && !effect.AlternativeCharacterEffectIfResisted.Equals(effect))
        {
            ApplyEffect(effect.AlternativeCharacterEffectIfResisted, sender);
        }
    }

    public virtual Material EffectMaterial
    {
        get
        {
            return GetComponentInChildren<SpriteRenderer>()?.sharedMaterial;
        }
        protected set
        {
            var spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sharedMaterial = value;
            }
        }
    }

    public virtual bool ApplyCondition(AbstractEffect effect, MonoBehaviour sender)
    {
        return !GetHasImmuneToEffect(effect);
    }

    public void ApplyEffect(List<AbstractEffect> effects, MonoBehaviour sender)
    {
        effects.Sort();

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
                break;
            }
        }
        UpdateEffectMaterial();
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
                break;
            }
        }
        UpdateEffectMaterial();
    }

    public void RemoveEffect(List<AbstractEffect> effects)
    {
        if (effects.Count == 0) return;
        for (int i = 0; i < _currentEffects.Count; i++)
        {
            RemoveEffect(effects[i]);
            break;
        }
    }

    public bool GetHasEffect<T>()
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

    public T GetEffect<T>()
    {
        for (int i = 0; i < _currentEffects.Count; i++)
        {
            if (_currentEffects[i] is T result)
            {
                return result;
            }
        }
        return default(T);
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

    public List<T> GetEffects<T>()
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
        List<EffectImmunity> immunities = GetEffects<EffectImmunity>();
        foreach (EffectImmunity immunity in immunities)
        {
            if (immunity.ImmuneTo.Equals(effect))
            {
                return true;
            }
        }

        return false;
    }

    private void UpdateEffectMaterial()
    {
        foreach (AbstractEffect effect in _currentEffects)
        {
            if (effect.EffectMaterial != null)
            {
                EffectMaterial = effect.EffectMaterial;
                return;
            }
        }
        EffectMaterial = _defaultEffectMaterial;
    }
}
