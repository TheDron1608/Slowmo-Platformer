
using UnityEngine;

public class ExtraBulletBulletReloadingWeapon : BulletReloadingWeapon
{
    const string RELOAD_EXTRA_BULLET_PROP_NAME = "ReloadExtraBullet";

    public int ExtraAmmoPerReload = 1;

    private bool _isReloadingExtraBullet = false;

    protected override void OnReload()
    {
        UpdateReloadExtraBullet();

        base.OnReload();
    }

    public override void OnLoadFinish()
    {
        base.OnLoadFinish();

        UpdateReloadExtraBullet();
    }

    protected override int CalculateAmmoAmountPerReload()
    {
        return base.CalculateAmmoAmountPerReload() + (_isReloadingExtraBullet ? ExtraAmmoPerReload : 0);
    }

    protected override bool OnTryAttackSuccess(Vector2 direction)
    {
        bool result = base.OnTryAttackSuccess(direction);

        SpawnBulletParticles(LoadedSpentAmmoLeft);
        LoadedSpentAmmoLeft = 0;

        return result;
    }

    private void UpdateReloadExtraBullet()
    {
        if (MaxLoadedAmmo >= LoadedLivingAmmoLeft + LoadedSpentAmmoLeft + AmmoAmountPerReload + ExtraAmmoPerReload)
        {
            if (_isReloadingExtraBullet == false) _animator.SetBool(RELOAD_EXTRA_BULLET_PROP_NAME, true);
            _isReloadingExtraBullet = true;
        }
        else
        {
            if (_isReloadingExtraBullet == true) _animator.SetBool(RELOAD_EXTRA_BULLET_PROP_NAME, false);
            _isReloadingExtraBullet = false;
        }
    }
}