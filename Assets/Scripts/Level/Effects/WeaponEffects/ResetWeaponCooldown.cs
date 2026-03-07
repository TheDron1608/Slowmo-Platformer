using System.Collections.Generic;
using UnityEngine;

public class ResetWeaponCooldown : AbstractWeaponEffect
{
    protected override void OnApply()
    {
        base.OnApply();

        Weapon.ResetAttackCooldown();

        RemoveSelf();
    }
}
