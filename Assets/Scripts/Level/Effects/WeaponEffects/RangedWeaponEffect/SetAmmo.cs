
using System;

public class SetAmmo : AbstractRangedWeaponEffect, IMultiplierableEffect
{
    public float Ammo = 1f;

    private float _effectMultiplier = 1f;

    public float EffectMultiplier
    {
        get => _effectMultiplier;
        set => _effectMultiplier = value;
    }

    protected override void OnApply()
    {
        base.OnApply();

        RangedWeapon.AmmoLeft = (int)Math.Round(Ammo * EffectMultiplier);

        RemoveSelf();
    }
}