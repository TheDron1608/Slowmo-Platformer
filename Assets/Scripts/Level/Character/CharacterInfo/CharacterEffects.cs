using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Build.Pipeline;
using UnityEngine;
using UnityEngine.U2D.IK;

public class CharacterEffects : AbstractCharacterComponent
{
    [SerializeField] private List<AbstractCharacterEffect> _currentEffects = new();
    private AbstractCharacterComponent _lastHitter = null;
    private List<AbstractCharacterComponent> _lastOneSecondHitters = new();

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

    protected override void OnAwake()
    {
        base.OnAwake();
        List<AbstractCharacterEffect> reapplyEffects = _currentEffects.ToList();
        _currentEffects.Clear();
        foreach (AbstractCharacterEffect effect in reapplyEffects)
        {
            ApplyEffect(effect, null, null);
        }
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
        if (effect.ApplyCondition(CharComponents, sender, receiverPart))
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
        else if (effect.AlternativeCharacterEffectIfResisted != null && !effect.AlternativeCharacterEffectIfResisted.Equals(effect))
        {
            ApplyEffect(effect.AlternativeCharacterEffectIfResisted, sender, receiverPart);
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

    public void RemoveEffect<T>(CharacterLimbPart limb = null) where T : AbstractCharacterEffect
    {
        for (int i = 0; i < _currentEffects.Count; i++)
        {
            if (_currentEffects[i] is T && (limb == null || (_currentEffects[i] is AbstractCharacterLimbEffect limbEffect && limbEffect.AffectedLimbPart == limb)))
            {
                if (_currentEffects[i].IsDestroyed()) continue;

                OnEffectRemoved?.Invoke(this, _currentEffects[i]);
                GameObject.Destroy(_currentEffects[i].gameObject);
                _currentEffects.RemoveAt(i);
                i--;
            }
        }
    }

    public void RemoveEffect(AbstractCharacterEffect effect, CharacterLimbPart limb = null)
    {
        if (effect.IsDestroyed()) return;

        for (int i = 0; i < _currentEffects.Count; i++)
        {
            if (_currentEffects[i].Equals(effect) && (limb == null || (_currentEffects[i] is AbstractCharacterLimbEffect limbEffect && limbEffect.AffectedLimbPart == limb)))
            {
                OnEffectRemoved?.Invoke(this, _currentEffects[i]);
                GameObject.Destroy(_currentEffects[i].gameObject);
                _currentEffects.RemoveAt(i);
                i--;
            }
        }
    }

    public void RemoveEffect(List<AbstractCharacterEffect> effects, CharacterLimbPart limb = null)
    {
        if (effects.Count == 0) return;
        for (int i = 0; i < _currentEffects.Count; i++)
        {
            RemoveEffect(effects[i], limb);
        }
    }

    public bool GetHasEffect<T>(CharacterLimbPart limb = null) where T : AbstractCharacterEffect
    {
        for (int i = 0; i < _currentEffects.Count;i++)
        {
            if (_currentEffects[i] is T && (limb == null || (_currentEffects[i] is AbstractCharacterLimbEffect limbEffect && limbEffect.AffectedLimbPart == limb)))
            {
                return true;
            }
        }
        return false;
    }

    public T GetEffect<T>(CharacterLimbPart limb = null) where T : AbstractCharacterEffect
    {
        for (int i = 0; i < _currentEffects.Count; i++)
        {
            if (_currentEffects[i] is T && (limb == null || (_currentEffects[i] is AbstractCharacterLimbEffect limbEffect && limbEffect.AffectedLimbPart == limb)))
            {
                return _currentEffects[i] as T;
            }
        }
        return null;
    }

    public bool TryGetEffect<T>(out T effect, CharacterLimbPart limb = null) where T : AbstractCharacterEffect
    {
        for (int i = 0; i < _currentEffects.Count; i++)
        {
            if (_currentEffects[i] is T outEffect && (limb == null || (_currentEffects[i] is AbstractCharacterLimbEffect limbEffect && limbEffect.AffectedLimbPart == limb)))
            {
                effect = outEffect;
                return true;
            }
        }
        effect = default;
        return false;
    }

    public List<T> GetEffects<T>(CharacterLimbPart limb = null) where T : AbstractCharacterEffect
    {
        List<T> result = new();

        for (int i = 0; i < _currentEffects.Count; i++)
        {
            if (_currentEffects[i] is T tEffect && (limb == null || (_currentEffects[i] is AbstractCharacterLimbEffect limbEffect && limbEffect.AffectedLimbPart == limb)))
            {
                result.Add(tEffect);
            }
        }
        return result;
    }
}
