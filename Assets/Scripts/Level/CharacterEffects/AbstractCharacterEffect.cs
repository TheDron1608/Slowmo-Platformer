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
    public AbstractCharacterEffect AlternativeCharacterEffectIfResisted = null;

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

    public virtual bool ApplyCondition(CharacterComponentsManager affectWho, MonoBehaviour sender, CharacterPart receiverPart)
    {
        if (receiverPart != null)
        {
            if (receiverPart is CharacterEquipmentPart equipmentPart)
            {
                throw new UnityException("receiverPart is not null and is not CharacteLimbPart and is equipAt part is not CharacterLimbPart");
            }

            CharacterLimbPart affectedLimb = null;
            if (receiverPart is CharacterLimbPart limbPart)
            {
                affectedLimb = limbPart;
            }

            List<LimbEffectImmunity> affectedLimbImmunityEffects = affectWho.CharacterEffects.GetEffects<LimbEffectImmunity>(affectedLimb);
            for (int i = 0; i < affectedLimbImmunityEffects.Count; i++)
            {
                if (affectedLimbImmunityEffects[i].ImmuneTo.Equals(this))
                {
                    return false;
                }
            }
        }


        List<EffectImmunity> affectedImmunityEffects = affectWho.CharacterEffects.GetEffects<EffectImmunity>();
        for (int i = 0; i < affectedImmunityEffects.Count; i++)
        {
            if (affectedImmunityEffects[i].ImmuneTo.Equals(this))
            {
                return false;
            }
        }

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
