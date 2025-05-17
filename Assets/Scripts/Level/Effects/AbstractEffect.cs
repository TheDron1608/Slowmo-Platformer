using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class AbstractEffect : MonoBehaviour, IComparable<AbstractEffect>, IEquatable<AbstractEffect>
{
    /// <summary>
    /// multiple effects will be applied sorted by EffectPriority descending
    /// </summary>
    public int EffectPriority = 100;
    public AbstractEffect AlternativeCharacterEffectIfResisted = null;

    private ObjectEffectsReceiver _affectedObject;

    public event EventHandler OnRemoved;

    public ObjectEffectsReceiver AffectedObject
    {
        get => _affectedObject;
        protected set => _affectedObject = value;
    }


    public virtual void RemoveSelf()
    {
        AffectedObject.RemoveEffect(this);
    }

    private void Awake()
    {
        if (transform.parent.TryGetComponent(out _affectedObject))
        {
            OnApply();
        }
        else
        {
            throw new UnityException("ObjectEffects components not found at parent in: " + gameObject.name);
        }
    }

    private void OnDestroy()
    {
        OnRemove();
    }

    public virtual bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return true;
    }

    protected virtual void OnApply()
    {

    }

    protected virtual void OnRemove()
    {
        OnRemoved?.Invoke(this, EventArgs.Empty);
    }

    public int CompareTo(AbstractEffect other)
    {
        return EffectPriority.CompareTo(other.EffectPriority);
    }

    public virtual bool Equals(AbstractEffect other)
    {
        return this.GetType() == other.GetType();
    }
}
