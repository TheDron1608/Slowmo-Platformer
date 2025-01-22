using System.Collections;
using Unity.Mathematics;
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
    [SerializeField] private bool _bulletLoadedInChamber = true;

    private ParticleSpawner _magsPraticleSpawner;

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
            if (_bulletLoadedInChamber)
            {
                LoadedLivingAmmoLeft--;
                SpawnBulletParticles(1);
            }
            _bulletLoadedInChamber = value;
        }
    }

    public override bool GetIsNeedReload()
    {
        return LoadedLivingAmmoLeft <= 0 && !Unloaded && !IsReloading;
    }

    public override bool GetIsOutOfAmmo()
    {
        return AmmoLeft <= MagSize && LoadedLivingAmmoLeft <= 0;
    }

    public void ReloadBullet()
    {
        _animator.SetTrigger(ANIMATOR_RELOAD_BULLET_TRIGGER_NAME);
    }

    protected override void OnAwake()
    {
        base.OnAwake();

        _magsPraticleSpawner = transform.Find(MAGS_PARTICLE_SPAWNER_GAMEOBJECT_NAME).GetComponent<ParticleSpawner>();
    }

    public void SpawnMagParticle()
    {
        _magsPraticleSpawner.SpawnParticle(1);
    }

    protected override bool AttackCondition()
    {
        return base.AttackCondition() && BulletLoadedInChamber;
    }

    protected override bool ReloadCondition()
    {
        return LoadedLivingAmmoLeft - (BulletLoadedInChamber ? 1 : 0) < MagSize;
    }

    protected override void OnReload()
    {
        if (Mags <= 0 && LoadedLivingAmmoLeft <= 0)
        {
            TryUnload();
            return;
        }
        else if (Mags <=  0)
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

        ReloadBullet();
    }

    protected override bool OnTryAttackSuccess(Vector2 direction)
    {
        base.OnTryAttackSuccess(direction);

        StartCoroutine(SpawnParticleAfterDuration(
            (
                AttackType == ProjectileType.BURST || AttackType == ProjectileType.BUCKSHOT_BURST) ? 
                BurstProjectilesAmount : 1
            ));

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

    private IEnumerator SpawnParticleAfterDuration(int amount)
    {
        yield return new WaitForSeconds(AWAIT_TIME_TO_SPAWN_BULLET_PARTICLE_ON_ATTACK);
        SpawnBulletParticles(amount);
    }
}
