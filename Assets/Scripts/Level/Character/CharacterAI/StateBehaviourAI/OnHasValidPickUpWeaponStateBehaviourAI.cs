using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnHasValidPickUpWeaponStateBehaviourAI : AbstractCharacterStateBehaviourAI
{
    public override bool StateBehaviourCondition()
    {
        return PrefferedHoldable.NearestPrefferedHoldable != null;
    }
}
