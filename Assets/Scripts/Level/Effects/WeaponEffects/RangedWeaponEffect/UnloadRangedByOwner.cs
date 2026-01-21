using UnityEngine;

public class UnloadRangedByOwner : AbstractRangedWeaponEffect
{
    const float UPDATES_PER_FRAME = 10f;

    protected override void OnApply()
    {
        base.OnApply();

        RangedWeapon.TryUnloadAllBullets();
        RemoveSelf();
    }
}
