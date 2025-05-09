using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnAbleToFightBehaviourAI : AbstractCharacterStateBehaviourAI
{
    public override bool StateBehaviourCondition()
    {
        return 
            CharComponents.CharacterAttacking.UnarmedAttackProjectile != null ||
            (CharComponents.CharacterHolding.CurrentHoldObject != null && CharComponents.CharacterHolding.CurrentHoldObject.GetComponent<Weapon>() != null);
    }
}
