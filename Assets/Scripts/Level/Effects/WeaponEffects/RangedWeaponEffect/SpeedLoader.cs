
using System;
using UnityEngine;

public class SpeedLoader : AbstractRangedWeaponEffect
{
    const int LOAD_AMMO_MULTIPLIER = 1000;

    protected override void OnApply()
    {
        base.OnApply();

        if (RangedWeapon.TryGetComponent(out BulletReloadingWeapon bulletReloadingWeapon))
        {
            bulletReloadingWeapon.AmmoAmountPerReload *= LOAD_AMMO_MULTIPLIER;
        }
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        if (RangedWeapon.TryGetComponent(out BulletReloadingWeapon bulletReloadingWeapon))
        {
            bulletReloadingWeapon.AmmoAmountPerReload /= LOAD_AMMO_MULTIPLIER;
        }
    }

    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return base.ApplyCondition(affectWho, sender) && affectWho.TryGetComponent(out BulletReloadingWeapon brw);
    }
}