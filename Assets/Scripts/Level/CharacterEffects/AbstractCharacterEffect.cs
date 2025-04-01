using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class AbstractCharacterEffect : MonoBehaviour, IComparable<AbstractCharacterEffect>, IEquatable<AbstractCharacterEffect>
{
    /// <summary>
    /// multiple effects will be applied sorted by EffectPriority descending
    /// </summary>
    public int EffectPriority = 100;

    private CharacterComponentsManager _affectedCharacter;

    public event EventHandler OnRemoved;

    public CharacterComponentsManager AffectedCharacter
    {
        get => _affectedCharacter;
        protected set => _affectedCharacter = value;
    }


    public virtual void RemoveSelf()
    {
        AffectedCharacter.CharacterEffects.RemoveEffect(this);
    }

    private void Awake()
    {
        if (transform.parent.TryGetComponent(out _affectedCharacter))
        {
            OnApply();
        }
        else
        {
            throw new UnityException("CharacterComponentsManager not found at parent in: " + gameObject.name);
        }
    }

    private void OnDestroy()
    {
        OnRemove();
    }

    public virtual bool ApplyCondition(CharacterComponentsManager affectWho)
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

    public int CompareTo(AbstractCharacterEffect other)
    {
        return EffectPriority.CompareTo(other.EffectPriority);
    }

    public virtual bool Equals(AbstractCharacterEffect other)
    {
        return this.GetType() == other.GetType();
    }
}
