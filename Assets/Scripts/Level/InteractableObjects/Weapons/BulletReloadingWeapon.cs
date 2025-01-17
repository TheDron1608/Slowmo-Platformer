using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class BulletReloadingWeapon : RangedWeapon
{
    public int LoadedLivingAmmoLeft = 1;
    public int LoadedSpentAmmoLeft = 0;
    public int AmmoAmountPerReload = 1;
    public int AmmoAmountPerUnload = 1;
    public int MaxLoadedAmmo = 1;

    protected override bool ReloadCondition()
    {
        return base.ReloadCondition() && LoadedLivingAmmoLeft < MaxLoadedAmmo && AmmoLeft > 0;
    }

    protected override bool UnloadCondition()
    {
        return LoadedLivingAmmoLeft <= 0 && LoadedSpentAmmoLeft > 0;
    }

    protected override bool AttackCondition()
    {
        return base.AttackCondition() && LoadedLivingAmmoLeft > 0;
    }

    protected override void SpawnBullet()
    {
        LoadedLivingAmmoLeft--;
        LoadedSpentAmmoLeft++;
        base.SpawnBullet();
    }

    protected override void SpawnBuckshot()
    {
        LoadedLivingAmmoLeft--;
        LoadedSpentAmmoLeft++;

        base.SpawnBuckshot();
    }

    protected override IEnumerator SpawnBurst()
    {
        for (int i = 0; i < BuckshotProjectilesAmount; i++)
        {
            if (LoadedLivingAmmoLeft <= 0) break;
            SpawnBullet();
            yield return new WaitForSeconds(DurationBetweenBurstProjectiles);
        }
    }

    protected override IEnumerator SpawnBuckshotBurst()
    {
        for (int i = 0; i < BuckshotProjectilesAmount; i++)
        {
            if (LoadedLivingAmmoLeft <= 0) break;
            SpawnBuckshot();
            yield return new WaitForSeconds(DurationBetweenBurstProjectiles);
        }
    }

    protected override void OnPickedUp()
    {
        base.OnPickedUp();

        if (Unloaded && LoadedLivingAmmoLeft > 0)
        {
            TryCloseMag();
        }
    }

    public override void OnLoadFinish()
    {
        base.OnLoadFinish();

        Unloaded = true;

        int loadAmount = AmmoAmountPerReload;
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
    }

    public override void OnUnloadFinish()
    {
        base.OnUnloadFinish();

        Unloaded = true;
        {
            GetComponent<RangedWeapon>().SpawnBulletParticles(math.min(AmmoAmountPerUnload, LoadedSpentAmmoLeft));
            LoadedSpentAmmoLeft -= AmmoAmountPerUnload;
            if (LoadedSpentAmmoLeft < 0)
            {
               LoadedSpentAmmoLeft = 0;
            }
        }
    }
}
