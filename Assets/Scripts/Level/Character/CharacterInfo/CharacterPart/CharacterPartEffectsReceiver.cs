using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class CharacterPartEffectsReceiver : ObjectEffectsReceiver
{
    protected override bool ApplyCondition(AbstractEffect effect, MonoBehaviour sender)
    {
        return 
            base.ApplyCondition(effect, sender) && 
            !GetComponent<AbstractCharacterComponent>().CharComponents.CharacterEffectsReceiver.GetHasImmuneToEffect(effect, GetComponent<CharacterPart>());
    }
}
