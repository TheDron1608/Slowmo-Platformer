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
}
