using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class RangedWeapon : ThrowableWeapon
{
    public enum ProjectileType
    {
        BULLET,
        BUCKSHOT,
        BURST,
        BUCKSHOT_BURST
    }

    protected const string PROJECTILE_SPAWN_POSITION_GAMEOBJECT_NAME = "ProjectileSpawnPosition";
    protected const string BULLET_PARTICLE_SPAWNER_GAMEOBJECT_NAME = "BulletParticleSpawner";
    protected const string CLOUD_PARTICLE_SPAWNER_GAMEOBJECT_NAME = "CloudParticleSpawner";
    protected const string ANIMATOR_IS_RELOADING_PROP_NAME = "IsReloading";
    protected const string ANIMATOR_UNLOADED_PROP_NAME = "Unloaded";
    protected const string ANIMATOR_RELOAD_SPEED_PROP_NAME = "ReloadSpeed";

    [Header("Ranged weapon")]
    public int AmmoLeft = 10;
    public int MaxAmmo = 10;
    public int LoadedLivingAmmoLeft = 1;
    public int LoadedSpentAmmoLeft = 0;

    private bool _isReloading = false;
    private bool _unloaded = false;
    private Transform _projectileSpawnPosition;
    private ParticleSpawner _bulletParticleSpawner;
    private ParticleSpawner _cloudParticleSpawner;

    //INITIALIZER
    protected override void OnAwake()
    {
        base.OnAwake();

        _projectileSpawnPosition = transform.Find(PROJECTILE_SPAWN_POSITION_GAMEOBJECT_NAME);
        _bulletParticleSpawner = transform.Find(BULLET_PARTICLE_SPAWNER_GAMEOBJECT_NAME).GetComponent<ParticleSpawner>();
        _cloudParticleSpawner = transform.Find(CLOUD_PARTICLE_SPAWNER_GAMEOBJECT_NAME).GetComponent<ParticleSpawner>();
    }

    //PUBLIC PROPERTIES
    public bool Unloaded
    {
        get => _unloaded;
        set
        {
            _animator.SetBool(ANIMATOR_UNLOADED_PROP_NAME, value);
            _unloaded = value;
        }
    }

    public bool IsReloading
    {
        get => _isReloading;
        set => _isReloading = value;
    }


    //PUBLIC METHODS
    public virtual bool GetIsNeedReload()
    {
        return false;
    }

    public virtual bool GetIsOutOfAmmo()
    {
        return AmmoLeft <= 0 && LoadedLivingAmmoLeft <= 0;
    }

    public virtual void SpendAmmo(int spendAmount = 1)
    {
        LoadedLivingAmmoLeft -= spendAmount;
        LoadedSpentAmmoLeft += spendAmount;
    }

    public bool TryReload()
    {
        if (ReloadCondition())
        {
            OnReload();
            return true;
        }
        else if (UnloadCondition())
        {
            TryUnload();
        }
        return false;
    }

    public bool TryFinishReload()
    {
        if (!IsReloading) return false;

        OnReloadFinish();

        return true;
    }

    public bool TryCloseMag()
    {
        if (!_unloaded) return false;

        Unloaded = false;
        return true;
    }

    public bool TryUnload()
    {
        if (_unloaded) return false;

        OnUnload();
        return true;
    }

    protected virtual void OnUnload()
    {
        Unloaded = true;
    }

    public void SpawnBulletParticles(int amount)
    {
        if (Projectile is AbstractRangedProjectile rangedProjectile)
        {
            _bulletParticleSpawner.SpawnParticle(amount, 0.05f, rangedProjectile.BulletCasingParticle.gameObject);
        }
        else
        {
            _bulletParticleSpawner.SpawnParticle(amount);
        }
    }

    public void SetReloadSpeed(float value)
    {
        _animator.SetFloat(ANIMATOR_RELOAD_SPEED_PROP_NAME, value);
    }

    //OVERRIDES
    protected override bool AttackCondition()
    {
        return base.AttackCondition() && LoadedLivingAmmoLeft > 0 && !IsReloading && !Unloaded;
    }

    protected virtual bool ReloadCondition()
    {
        return AmmoLeft > 0;
    }

    protected virtual bool UnloadCondition()
    {
        return false;
    }

    protected virtual void OnReload()
    {
        IsReloading = true;
        Unloaded = true;
        _animator.SetBool(ANIMATOR_IS_RELOADING_PROP_NAME, true);
    }

    protected virtual void OnReloadFinish()
    {
        IsReloading = false;
        _animator.SetBool(ANIMATOR_IS_RELOADING_PROP_NAME, false);
    }

    protected override void OnTryAttackFail(Vector2 direction)
    {
        base.OnTryAttackFail(direction);

        if (GetIsOutOfAmmo())
        {
            _cloudParticleSpawner.SpawnParticle(1);
        }
    }

    /// <summary>
    /// must be called only from animation controllers
    /// </summary>
    public virtual void OnLoadFinish()
    {
        Unloaded = false;
    }

    /// <summary>
    /// must be called only from animation controllers
    /// </summary>
    public virtual void OnUnloadFinish()
    {
        Unloaded = true;
    }
}
