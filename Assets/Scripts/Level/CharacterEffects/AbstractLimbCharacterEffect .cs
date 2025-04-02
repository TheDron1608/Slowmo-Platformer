using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class AbstractCharacterLimbEffect : AbstractCharacterEffectWithSender
{
    private CharacterLimbPart _affectedLimbPart;

    public CharacterLimbPart AffectedLimbPart
    {
        get => _affectedLimbPart;
        private set => _affectedLimbPart = value;
    }

    protected override void OnReceivedSender(MonoBehaviour sender, CharacterPart receiverPart)
    {
        if (receiverPart is CharacterLimbPart charLimb)
        {
            AffectedLimbPart = charLimb;
        }
        else
        {
            throw new UnityException("trying to receiver sender, but receiverPart must be CharacterLimbPart class, received " + receiverPart.GetType().Name + " instead");
        }
    }

    public override bool Equals(AbstractCharacterEffect other)
    {
        return base.Equals(other) && AffectedLimbPart == (other as AbstractCharacterLimbEffect).AffectedLimbPart;
    }

    public override void RemoveSelf()
    {
        AffectedCharacter.CharacterEffects.RemoveEffect(this, AffectedLimbPart);
    }

    public override bool ApplyCondition(CharacterComponentsManager affectWho, MonoBehaviour sender, CharacterPart receiverPart)
    {
        if (receiverPart is CharacterLimbPart receiverLimb)
        {
            List<AbstractCharacterEffect> affectedLimbEffects = receiverPart.CharComponents.CharacterEffects.GetEffects<LimbEffectImmunity>(receiverLimb);
            for (int i = 0; i < affectedLimbEffects.Count; i++)
            {
                if (
                    affectedLimbEffects[i] is LimbEffectImmunity affectedLimbImmunityEffect &&
                    affectedLimbImmunityEffect.ImmuneTo.Equals(this)
                    )
                {
                    return false;
                }
            }

            return base.ApplyCondition(affectWho, sender, receiverPart);
        }
        else
        {
            throw new UnityException("receiverPart must be CharacterLimbPart class, received " + receiverPart.GetType().Name + " instead");
        }
    }
}
