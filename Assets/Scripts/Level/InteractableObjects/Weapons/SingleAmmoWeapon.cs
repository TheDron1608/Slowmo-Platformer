using UnityEngine;

public class SingleAmmoWeapon : RangedWeapon
{
    public override int GetAmmoCapacity()
    {
        return 1;
    }

    public override bool GetIsNeedReload()
    {
        return LoadedLivingAmmoLeft <= 0;
    }

    protected override void OnReloadFinish()
    {
        base.OnReloadFinish();

        Unloaded = false;
        LoadedLivingAmmoLeft++;
        AmmoLeft--;
    }

    protected override bool OnTryAttackSuccess(Vector2 direction)
    {
        Unloaded = true;

        return base.OnTryAttackSuccess(direction);
    }

    public override void SpendAmmo(int spendAmount = 1)
    {
        base.SpendAmmo(spendAmount);
        LoadedSpentAmmoLeft = 0;
    }
}