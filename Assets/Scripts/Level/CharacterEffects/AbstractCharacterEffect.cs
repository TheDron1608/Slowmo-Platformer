using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class AbstractCharacterEffect : AbstractEffect, ICharacterEffect
{
    private CharacterComponentsManager _affectedCharacter;

    public CharacterComponentsManager AffectedCharacter
    {
        get => _affectedCharacter;
        private set => _affectedCharacter = value;
    }

    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return 
            base.ApplyCondition(affectWho, sender) && 
            affectWho.GetComponent<AbstractCharacterComponent>() != null;
    }

    protected override void OnApply()
    {
        base.OnApply();
        _affectedCharacter = GetComponent<AbstractCharacterComponent>().CharComponents;
    }
}
