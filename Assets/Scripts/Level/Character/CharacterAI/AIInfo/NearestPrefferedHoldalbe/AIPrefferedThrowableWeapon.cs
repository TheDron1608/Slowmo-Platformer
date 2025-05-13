using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class AIPrefferedThrowableWeapon : AIPrefferedHoldableOrderByDistance
{
    protected override bool PickUpCondition(Holdable holdable)
    {
        return base.PickUpCondition(holdable) && holdable.GetIsDangerousAsThrowable(CharComponents.CharacterHolding);
    }
}
