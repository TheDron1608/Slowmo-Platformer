using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class AbstractCharacterLimbEffect : AbstractEffectWithSender
{
    private CharacterLimbPart _affectedLimbPart;

    public CharacterLimbPart AffectedLimbPart
    {
        get => _affectedLimbPart;
    }

    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return
            base.ApplyCondition(affectWho, sender) &&
            affectWho.TryGetComponent(out CharacterLimbPart limbPart) &&
            limbPart.CharComponents.EntireCharacterEffectsReceiver.ApplyCondition(affectWho, sender);
    }

    protected override void OnApply()
    {
        base.OnApply();
        _affectedLimbPart = AffectedObject.GetComponent<CharacterLimbPart>();
    }

    public override bool Equals(AbstractEffect other)
    {
        return base.Equals(other) && AffectedLimbPart == (other as AbstractCharacterLimbEffect).AffectedLimbPart;
    }
}
