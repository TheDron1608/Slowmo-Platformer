
using System;

public class MultiplyAmmo : AbstractRangedWeaponEffect, IMultiplierableEffect
{
    public float AmmoMultiplier = 1f;

    private float _effectMultiplier = 1f;

    public float EffectMultiplier
    {
        get => _effectMultiplier;
        set => _effectMultiplier = value;
    }

    protected override void OnApply()
    {
        base.OnApply();

        RangedWeapon.AmmoLeft = (int)Math.Round(RangedWeapon.AmmoLeft * AmmoMultiplier * EffectMultiplier);

        RemoveSelf();
    }
}