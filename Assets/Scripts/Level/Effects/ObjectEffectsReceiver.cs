using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ObjectEffectsReceiver : MonoBehaviour
{
    public class EffectAddedEventArgs
    {
        public AbstractEffect Effect;
        public MonoBehaviour Sender;

        public EffectAddedEventArgs(AbstractEffect effect, MonoBehaviour sender)
        {
            Effect = effect;
            Sender = sender;
        }
    }

    //oh god there are so many isdestroyed checks
    public static AbstractCharacterComponent TryGetCharacterFromSender(MonoBehaviour sender)
    {
        if (sender == null || sender.IsDestroyed() || sender.gameObject.IsDestroyed())
        {
            return null;
        }
        else if (sender.TryGetComponent(out AbstractCharacterComponent senderCharacter) && !senderCharacter.IsDestroyed())
        {
            return senderCharacter;
        }
        else if (sender.TryGetComponent(out Holdable senderHoldable) && !senderHoldable.IsDestroyed())
        {
            return senderHoldable.CurrentHolder;
        }
        else if (
            sender.TryGetComponent(out AbstractProjectile senderProjectile) && !senderProjectile.IsDestroyed()
            )
        {
            if (
                senderProjectile.Deflector != null &&
                !senderProjectile.Deflector.IsDestroyed()
                )
            {
                return senderProjectile.Deflector;
            }
            else if (senderProjectile.Weapon != null && !senderProjectile.Weapon.IsDestroyed())
            {
                if (
                    senderProjectile.Weapon.TryGetComponent(out Holdable holdableWeapon) &&
                    holdableWeapon != null && !holdableWeapon.IsDestroyed())
                {
                    return holdableWeapon?.CurrentHolder;
                }
                else if (
                    senderProjectile.Weapon.TryGetComponent(out UnarmedWeapon unarmedWeapon) &&
                    unarmedWeapon != null && !unarmedWeapon.IsDestroyed()
                    )
                {
                    return unarmedWeapon?.CharComponents.CharacterAttacking;
                }
            }
        }
        return null;
    }

    public List<AbstractEffect> CounterEffectsOnApplier = new();
    [SerializeField] private List<AbstractEffect> _currentEffects = new();

    private bool _wasKilledBefore = false;


    public event EventHandler<EffectAddedEventArgs> OnEffectAdded;
    public event EventHandler<AbstractEffect> OnEffectRemoved;

    public List<AbstractEffect> CurrentEffects
    {
        get => _currentEffects;
    }

    public bool WasKilledBefore
    {
        get => _wasKilledBefore;
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

    /// <returns>returns actual applied effect including AlternativeCharacterEffectIfResisted</returns>
    public virtual List<AbstractEffect> ApplyEffect(List<AbstractEffect> effects, MonoBehaviour sender, float effectMultiplier = 1f, bool ignoreDeflection = false)
    {
        if (
            !ignoreDeflection &&
            sender != null &&
            sender.TryGetComponent(out ObjectEffectsReceiver senderEffectsReceiver) &&
            CounterEffectsOnApplier.Count > 0
            )
        {
            senderEffectsReceiver.ApplyEffect(CounterEffectsOnApplier, this);
            if (
                sender == null ||
                sender.IsDestroyed() || 
                (sender.TryGetComponent(out AbstractProjectile projectileSender) && projectileSender.WasDeflectedThisFrame)
                )
            {
                return new List<AbstractEffect>(0);
            }
        } 

        List<AbstractEffect> result = new();
        effects.Sort();

        for (int i = 0; i < effects.Count; i++)
        {
            if (effects[i] != null)
            {
                result.Add(ApplyEffect(effects[i], sender, effectMultiplier, true));
            }
        }

        return result;
    }

    /// <returns>returns actual applied effect including AlternativeCharacterEffectIfResisted</returns>
    public virtual AbstractEffect ApplyEffect(AbstractEffect effect, MonoBehaviour sender, float effectMultiplier = 1f, bool ignoreDeflection = false)
    {
        if (
            effect == null ||
            gameObject.IsDestroyed() ||
            (sender != null && sender.gameObject.IsDestroyed() && effect is AbstractEffectWithSender)
            )
        {
            return null;
        }

        if (
            !ignoreDeflection &&
            sender != null &&
            sender.TryGetComponent(out ObjectEffectsReceiver senderEffectsReceiver) &&
            CounterEffectsOnApplier.Count > 0
            )
        {
            senderEffectsReceiver.ApplyEffect(CounterEffectsOnApplier, this);
            if (
                sender == null ||
                sender.IsDestroyed() ||
                (sender.TryGetComponent(out AbstractProjectile projectileSender) && projectileSender.WasDeflectedThisFrame)
                )
            {
                return null;
            }
        }

        if (ApplyCondition(effect, sender) && effect.ApplyCondition(this, sender))
        {
            AbstractEffect newEffect = Instantiate(effect, transform);
            _currentEffects.Add(newEffect);
            if (newEffect is IMultiplierableEffect multiplierableEffect)
            {
                multiplierableEffect.EffectMultiplier = effectMultiplier;
            }
            if (newEffect is AbstractEffectWithSender effectWithsender)
            {
                effectWithsender.ApplySender(sender);
            }

            if (sender != null)
            {
                if (sender.TryGetComponent(out IEffectApplier effectApplier))
                {
                    effectApplier.InvokeOnEffectApllied(effect, this);
                }
            }

            if (effect.EffectMaterial != null)
            {
                EffectMaterial = effect.EffectMaterial;
            }

            OnEffectAdded?.Invoke(this, new(newEffect, sender));

            if (effect is ILethalEffect)
            {
                _wasKilledBefore = true;
            }

            return newEffect;
        }
        else if (effect.AlternativeCharacterEffectIfResisted != null && !effect.AlternativeCharacterEffectIfResisted.Equals(effect))
        {
            return ApplyEffect(effect.AlternativeCharacterEffectIfResisted, sender);
        }
        else
        {
            return null;
        }
    }

    public virtual Material EffectMaterial
    {
        get
        {
            return GetComponent<DynamicMaterial>()?.GetCurrentMaterial();
        }
        protected set
        {
            if (TryGetComponent(out DynamicMaterial dynamicMaterial))
            {
                dynamicMaterial.OverrideMaterial = value;
            }
        }
    }

    public virtual bool ApplyCondition(AbstractEffect effect, MonoBehaviour sender)
    {
        return !GetHasImmuneToEffect(effect);
    }

    public void RemoveAllEffects()
    {
        while (_currentEffects.Count > 0)
        {
            AbstractEffect removedEffect = _currentEffects.Last();
            if (removedEffect.IsDestroyed()) continue;

            OnEffectRemoved?.Invoke(this, removedEffect);
            _currentEffects.RemoveAt(_currentEffects.Count - 1);
            removedEffect.OnRemovedEffect();
        }
        UpdateEffectMaterial();
    }

    public virtual void RemoveEffect<T>()
    {
        for (int i = 0; i < _currentEffects.Count; i++)
        {
            if (_currentEffects[i] is T)
            {
                AbstractEffect removedEffect = _currentEffects[i];
                if (removedEffect.IsDestroyed()) continue;

                OnEffectRemoved?.Invoke(this, removedEffect);
                _currentEffects.RemoveAt(i);
                removedEffect.OnRemovedEffect();
                break;
            }
        }
        UpdateEffectMaterial();
    }

    public void RemoveEffect(AbstractEffect effect)
    {
        if (effect == null || effect.IsDestroyed()) return;

        for (int i = 0; i < _currentEffects.Count; i++)
        {
            foreach (AbstractEffect incomingEffect in effect.GetSelfIncludeIncomingEffects())
            {
                if (ReferenceEquals(_currentEffects[i], incomingEffect) || _currentEffects[i].Equals(incomingEffect))
                {
                    AbstractEffect removedEffect = _currentEffects[i];
                    if (removedEffect.IsDestroyed()) continue;
                    OnEffectRemoved?.Invoke(this, removedEffect);
                    _currentEffects.RemoveAt(i);
                    removedEffect.OnRemovedEffect();

                    if (i > 0) i--;
                    if (_currentEffects.Count == 0)
                    {
                        UpdateEffectMaterial();
                        return;
                    }
                }
            }
        }
        UpdateEffectMaterial();
    }

    public void RemoveEffect(List<AbstractEffect> effects)
    {
        if (effects == null) return;
        foreach (AbstractEffect effect in effects)
        {
            RemoveEffect(effect);
        }
    }

    public virtual bool GetHasEffect<T>(bool includeIncomingEffects = false)
    {
        for (int i = 0; i < _currentEffects.Count; i++)
        {
            if (GetEffectEqual<T>(_currentEffects[i], includeIncomingEffects))
            {
                return true;
            }
        }
        return false;
    }

    public virtual bool GetHasEffect(AbstractEffect effect, bool includeIncomingEffects = false)
    {
        for (int i = 0; i < _currentEffects.Count; i++)
        {
            if (GetEffectEqual(_currentEffects[i], effect, includeIncomingEffects))
            {
                return true;
            }
        }
        return false;
    }

    public bool GetHasEffect(List<AbstractEffect> effects, bool includeIncomingEffects = false)
    {
        foreach (AbstractEffect effect in effects)
        {
            if (GetHasEffect(effect, includeIncomingEffects))
            {
                return true;
            }
        }
        return false;
    }

    public virtual T GetEffect<T>(bool includeIncomingEffects = false)
    {
        for (int i = 0; i < _currentEffects.Count; i++)
        {
            if (GetEffectEqual<T>(_currentEffects[i], includeIncomingEffects, out T result))
            {
                return result;
            }
        }
        return default;
    }

    public virtual bool TryGetEffect<T>(out T effect)
    {
        for (int i = 0; i < _currentEffects.Count; i++)
        {
            if (GetEffectEqual(_currentEffects[i], false, out T tEffect))
            {
                effect = tEffect;
                return true;
            }
        }
        effect = default;
        return false;
    }

    public virtual bool TryGetEffect<T>(out T effect, out AbstractEffect incomingEffectOwner, bool includeIncomingEffects = false)
    {
        for (int i = 0; i < _currentEffects.Count; i++)
        {
            if (GetEffectEqual(_currentEffects[i], includeIncomingEffects, out T tEffect))
            {
                effect = tEffect;
                incomingEffectOwner = _currentEffects[i];
                return true;
            }
        }
        effect = default;
        incomingEffectOwner = default;
        return false;
    }

    public virtual List<T> GetEffects<T>(bool includeIncomingEffects = false)
    {
        List<T> result = new();

        for (int i = 0; i < _currentEffects.Count; i++)
        {
            if (includeIncomingEffects)
            {
                foreach(AbstractEffect incomingEffect in _currentEffects[i].GetSelfIncludeIncomingEffects())
                {
                    if (GetEffectEqual<T>(incomingEffect, true, out T tEffect))
                    {
                        result.Add(tEffect);
                    }
                }
            }
            else
            {
                if (GetEffectEqual<T>(_currentEffects[i], false, out T tEffect))
                {
                    result.Add(tEffect);
                }
            }
        }
        return result;
    }

    public bool GetHasImmuneToEffect(AbstractEffect effect)
    {
        List<EffectImmunity> immunities = GetEffects<EffectImmunity>();
        foreach (EffectImmunity immunity in immunities)
        {
            if (immunity.GetIsImmuneTo(effect))
            {
                return true;
            }
        }

        return false;
    }

    private bool GetEffectEqual(AbstractEffect effect1, AbstractEffect effect2, bool includeIncomingEffects)
    {
        if (includeIncomingEffects)
        {
            foreach (AbstractEffect incomingEffect1 in effect1.GetSelfIncludeIncomingEffects())
            {
                foreach (AbstractEffect incomingEffect2 in effect2.GetSelfIncludeIncomingEffects())
                {
                    if (incomingEffect1.Equals(incomingEffect2)) return true;
                }
            }
            return false;
        }
        else
        {
            return effect1.Equals(effect2);
        }
    }

    private bool GetEffectEqual<T>(AbstractEffect effect, bool includeIncomingEffects)
    {
        if (includeIncomingEffects)
        {
            foreach (AbstractEffect incomingEffect in effect.GetSelfIncludeIncomingEffects())
            {
                if (incomingEffect is T) return true;
            }
            return false;
        }
        else
        {
            return effect is T;
        }
    }

    private bool GetEffectEqual<T>(AbstractEffect effect, bool includeIncomingEffects, out T tEffect)
    {
        if (includeIncomingEffects)
        {
            foreach (AbstractEffect incomingEffect in effect.GetSelfIncludeIncomingEffects())
            {
                if (incomingEffect is T result)
                {
                    tEffect = result;
                    return true;
                }
            }
            tEffect = default;
            return false;
        }
        else
        {
            if (effect is T result)
            {
                tEffect = result;
                return true;
            }
            else
            {
                tEffect = default;
                return false;
            }
        }
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
        EffectMaterial = null;
    }
}
