using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// used for weapon with manazines link assault rifle or auto shotgun
/// </summary>
public class MagReloadingWeapon : RangedWeapon
{
    const string ANIMATOR_RELOAD_BULLET_TRIGGER_NAME = "ReloadBullet";
    const string MAGS_PARTICLE_SPAWNER_GAMEOBJECT_NAME = "MagParticleSpawner";
    const float AWAIT_TIME_TO_SPAWN_BULLET_PARTICLE_ON_ATTACK = 0.1f; //in seconds

    [Header("Mag reloading weapon")]
    [SerializeField] private int _magSize = 16;
    public Sprite GameplayUIMagSprite;
    public AbstractSoundPlayer ReloadBulletSound;
    public AbstractSoundPlayer UnloadBulletSound;

    private ParticleSpawner _magsPraticleSpawner;
    private bool _bulletLoadedInChamber = true;
    private bool _isReloadingBullet = false;

    public event EventHandler OnReloadedBullet;

    public int MagSize
    {
        get => _magSize;
        set
        {
            MaxAmmo = MaxAmmo / _magSize * value;
            _magSize = value;
        }
    }

    public int MaxMags
    {
        get => MaxAmmo / MagSize;
        set
        {
            MaxAmmo = value * MagSize;
        }
    }
    public int Mags
    {
        get => AmmoLeft / MagSize;
        set
        {
            AmmoLeft = MagSize * value + (AmmoLeft > 0 ? 1 : 0);
        }
    }

    public bool BulletLoadedInChamber
    {
        get => _bulletLoadedInChamber;
        set
        {
            if (_bulletLoadedInChamber && value)
            {
                SpendAmmo();
            }
            _bulletLoadedInChamber = value;
            OnReloadedBullet?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool IsReloadingBullet
    {
        get => _isReloadingBullet;
    }

    public override bool GetIsNeedReload()
    {
        return LoadedLivingAmmoLeft <= 0 && !Unloaded && !IsReloading;
    }

    public override bool GetIsOutOfAmmo()
    {
        return AmmoLeft < MagSize && LoadedLivingAmmoLeft <= 0;
    }

    public override void SpendAmmo(int spendAmount = 1)
    {
        base.SpendAmmo(spendAmount);

        SpawnBulletParticles(1);
    }

    public void ReloadBullet()
    {
        _isReloadingBullet = true;
        _animator.SetTrigger(ANIMATOR_RELOAD_BULLET_TRIGGER_NAME);
    }

    protected override void OnAwake()
    {
        base.OnAwake();

        _magsPraticleSpawner = transform.Find(MAGS_PARTICLE_SPAWNER_GAMEOBJECT_NAME).GetComponent<ParticleSpawner>();
    }

    public void SpawnMagParticle()
    {
        _magsPraticleSpawner.SpawnParticle();
    }

    protected override bool AttackCondition()
    {
        return base.AttackCondition() && BulletLoadedInChamber;
    }

    protected override bool ReloadCondition()
    {
        return LoadedLivingAmmoLeft <= MagSize;
    }

    protected override bool SpawnParticleOnUnableToAttackCondition()
    {
        return base.SpawnParticleOnUnableToAttackCondition() && !IsReloadingBullet;
    }

    protected override void OnReload()
    {
        if (Mags <= 0 && LoadedLivingAmmoLeft <= 0)
        {
            TryUnload();
            return;
        }
        else if (Mags <= 0)
        {
            return;
        }

        base.OnReload();

        if (!BulletLoadedInChamber)
        {
            ReloadBullet();
        }
    }

    protected override void OnUnload()
    {
        base.OnUnload();

        LoadedLivingAmmoLeft = BulletLoadedInChamber ? 1 : 0;
        LoadedSpentAmmoLeft = 0;

        if (LoadedLivingAmmoLeft + LoadedSpentAmmoLeft > 0)
        {
            ReloadBullet();
        }
    }

    public override void TryUnloadAllBullets()
    {
        base.TryUnloadAllBullets();

        TryUnload();
    }

    protected override bool OnTryAttackSuccess(Vector2 direction)
    {
        base.OnTryAttackSuccess(direction);

        if (LoadedLivingAmmoLeft <= 0)
        {
            BulletLoadedInChamber = false;
        }

        return true;
    }

    public override void OnUnloadFinish()
    {
        base.OnUnloadFinish();

        SpawnMagParticle();
    }

    public override void OnLoadFinish()
    {
        base.OnLoadFinish();

        LoadedLivingAmmoLeft = MagSize + (BulletLoadedInChamber ? 1 : 0);
        LoadedSpentAmmoLeft = 0;
        Mags--;
    }

    public virtual void OnReloadBulletFinish()
    {
        BulletLoadedInChamber = true;
        _isReloadingBullet = false;
    }
}
