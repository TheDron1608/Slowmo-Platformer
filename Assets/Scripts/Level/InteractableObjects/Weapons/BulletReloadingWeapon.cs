using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// used for barrel shotgun and revelovers
/// </summary>
public class BulletReloadingWeapon : RangedWeapon
{
    const float EMPTYNESS_LOAD_PITCH_MULT = 1.5f;

    [Header("Bullet reloading weapon")]
    public int AmmoAmountPerReload = 1;
    public int AmmoAmountPerUnload = 1;
    public int MaxLoadedAmmo = 1;
    public AbstractSoundPlayer SoundOnLoadBullet;

    private bool _isUnloadingAllBullets = false;

    public override bool IsThrown 
    { 
        get => base.IsThrown;
        set
        {
            base.IsThrown = value;
            if (value) _isUnloadingAllBullets = false;
        }
    }

    protected override void OnTryAttackFail(Vector2 direction)
    {
        if (IsReloading)
        {
            TryFinishReload();
        }

        base.OnTryAttackFail(direction);
    }

    protected override void VirtualOnEnable()
    {
        base.VirtualOnEnable();
        _isUnloadingAllBullets = false;
    }

    protected override bool AttackCondition()
    {
        return !_isUnloadingAllBullets && base.AttackCondition();
    }

    protected override bool ReloadCondition()
    {
        return base.ReloadCondition() && LoadedLivingAmmoLeft < MaxLoadedAmmo;
    }

    protected override bool UnloadCondition()
    {
        return (LoadedLivingAmmoLeft <= 0 && LoadedSpentAmmoLeft > 0) || (_isUnloadingAllBullets && !Unloaded);
    }

    public override void TryUnloadAllBullets()
    {
        base.TryUnloadAllBullets();

        _isUnloadingAllBullets = true;

        TryUnload();
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

        if (LoadedLivingAmmoLeft >= MaxLoadedAmmo || AmmoLeft <= 0)
        {
            TryFinishReload();
        }
    }

    public override void OnUnloadFinish()
    {
        if (_isUnloadingAllBullets)
        {
            SpawnBulletParticles(math.min(AmmoAmountPerUnload, LoadedSpentAmmoLeft + LoadedLivingAmmoLeft));

            LoadedSpentAmmoLeft -= math.max(AmmoAmountPerUnload - LoadedLivingAmmoLeft, 0);
            LoadedLivingAmmoLeft -= AmmoAmountPerUnload;
            if (LoadedSpentAmmoLeft < 0) LoadedSpentAmmoLeft = 0;
            if (LoadedLivingAmmoLeft < 0) LoadedLivingAmmoLeft = 0;

            if (LoadedLivingAmmoLeft + LoadedSpentAmmoLeft == 0)
            {
                _isUnloadingAllBullets = false;
            }
            else
            {
                IsUnloading = false;
                TryCloseMag();
            }
        }
        else
        {
            SpawnBulletParticles(math.min(AmmoAmountPerUnload, LoadedSpentAmmoLeft));

            LoadedSpentAmmoLeft -= AmmoAmountPerUnload;
            if (LoadedSpentAmmoLeft < 0) LoadedSpentAmmoLeft = 0;
        }

        base.OnUnloadFinish();
    }

    public override int GetAmmoCapacity()
    {
        return MaxLoadedAmmo;
    }

    public void Animator_PlayLoadBulletSound()
    {
        SoundOnLoadBullet.Pitch = Mathf.Lerp(1f / EMPTYNESS_LOAD_PITCH_MULT, EMPTYNESS_LOAD_PITCH_MULT, NumberMath.LimitFloatBetweenZeroAndOne((float)(LoadedLivingAmmoLeft + 1) / MaxLoadedAmmo));
        SoundOnLoadBullet.PlaySound();
    }
}
