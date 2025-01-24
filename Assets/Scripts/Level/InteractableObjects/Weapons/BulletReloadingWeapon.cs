using System.Collections;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// used for barrel shotgun and revelovers
/// </summary>
public class BulletReloadingWeapon : RangedWeapon
{
    [Header("Bullet reloading weapon")]
    public int AmmoAmountPerReload = 1;
    public int AmmoAmountPerUnload = 1;
    public int MaxLoadedAmmo = 1;

    protected override bool OnTryAttackSuccess(Vector2 direction)
    {
        if (IsReloading)
        {
            TryFinishReload();
            return false;
        }

        base.OnTryAttackSuccess(direction);

        return true;
    }

    protected override bool ReloadCondition()
    {
        return base.ReloadCondition() && LoadedLivingAmmoLeft < MaxLoadedAmmo;
    }

    protected override bool UnloadCondition()
    {
        return LoadedLivingAmmoLeft <= 0 && LoadedSpentAmmoLeft > 0;
    }

    public override bool GetIsNeedReload()
    {
        return LoadedLivingAmmoLeft <= 0;
    }

    public override void OnLoadFinish()
    {
        base.OnLoadFinish();

        int loadAmount = math.min(AmmoAmountPerReload, MaxLoadedAmmo - LoadedLivingAmmoLeft - LoadedSpentAmmoLeft);
        if (loadAmount > 0)
        {
            AmmoLeft -= loadAmount;
            LoadedLivingAmmoLeft += loadAmount;
        }
        else if (LoadedLivingAmmoLeft <= 0)
        {
            TryUnload();
        }

        if (LoadedLivingAmmoLeft > MaxLoadedAmmo)
        {
            LoadedLivingAmmoLeft = MaxLoadedAmmo;
        }

        if (LoadedLivingAmmoLeft >= MaxLoadedAmmo)
        {
            TryFinishReload();
        }
    }

    protected override void OnUnload()
    {
        base.OnUnload();

        TryFinishReload();
    }

    public override void OnUnloadFinish()
    {
        base.OnUnloadFinish();

        SpawnBulletParticles(math.min(AmmoAmountPerUnload, LoadedSpentAmmoLeft));
        LoadedSpentAmmoLeft -= AmmoAmountPerUnload;
        if (LoadedSpentAmmoLeft < 0)
        {
            LoadedSpentAmmoLeft = 0;
        }
    }
}
