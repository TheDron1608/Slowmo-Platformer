
using System;

public class SetLoadedAmmo : AbstractRangedWeaponEffect, IMultiplierableEffect
{
    public float LoadedAmmo = 1f;

    private float _effectMultiplier = 1f;

    public float EffectMultiplier
    {
        get => _effectMultiplier;
        set => _effectMultiplier = value;
    }

    protected override void OnApply()
    {
        base.OnApply();

        RangedWeapon.LoadedLivingAmmoLeft = (int)Math.Round(LoadedAmmo * EffectMultiplier);

        RemoveSelf();
    }
}