using System;
using UnityEngine;

public abstract class RangedWeapon : ThrowableWeapon
{
    protected const string BULLET_PARTICLE_SPAWNER_GAMEOBJECT_NAME = "BulletParticleSpawner";
    protected const string CLOUD_PARTICLE_SPAWNER_GAMEOBJECT_NAME = "CloudParticleSpawner";
    protected const string SHOOT_LIGHT_PARTICLE_SPAWNER_GAMEOBJECT_NAME = "ShootLightParticleSpawner";
    protected const string ANIMATOR_IS_RELOADING_PROP_NAME = "IsReloading";
    protected const string ANIMATOR_UNLOADED_PROP_NAME = "Unloaded";
    protected const string ANIMATOR_RELOAD_SPEED_PROP_NAME = "ReloadSpeed";

    [Header("Ranged weapon")]
    public int AmmoLeft = 10;
    public int MaxAmmo = 10;
    public float JamChance = 0f;
    public int LoadedLivingAmmoLeft = 1;
    public int LoadedSpentAmmoLeft = 0;
    public AbstractSoundPlayer SoundOnOutOfAmmo;
    public AbstractSoundPlayer SoundOnLoad;
    public AbstractSoundPlayer SoundOnUnload;

    private bool _isReloading = false;
    private bool _isUnloading = false;
    private bool _unloaded = false;
    private ParticleSpawner _bulletParticleSpawner;
    private ParticleSpawner _cloudParticleSpawner;
    private ParticleSpawner _shootLightParticleSpawner;

    public event EventHandler OnLoaded;
    public event EventHandler OnUnloaded;

    //INITIALIZER
    protected override void OnAwake()
    {
        base.OnAwake();

        _bulletParticleSpawner = transform.Find(BULLET_PARTICLE_SPAWNER_GAMEOBJECT_NAME).GetComponent<ParticleSpawner>();
        _cloudParticleSpawner = transform.Find(CLOUD_PARTICLE_SPAWNER_GAMEOBJECT_NAME).GetComponent<ParticleSpawner>();
        _shootLightParticleSpawner = transform.Find(PROJECTILE_SPAWN_POSITION_GAMEOBJECT_NAME).Find(SHOOT_LIGHT_PARTICLE_SPAWNER_GAMEOBJECT_NAME).GetComponent<ParticleSpawner>();  
    }

    //PUBLIC PROPERTIES
    public bool Unloaded
    {
        get => _unloaded;
        set
        {
            if (_unloaded == value) return;

            _animator.SetBool(ANIMATOR_UNLOADED_PROP_NAME, value);
            _unloaded = value;
        }
    }

    public bool IsReloading
    {
        get => _isReloading;
        protected set => _isReloading = value;
    }

    public bool IsUnloading
    {
        get => _isUnloading;
        protected set => _isUnloading = value;
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

    public override bool GetIsAbleToAttack()
    {
        return base.GetIsAbleToAttack() && !GetIsOutOfAmmo();
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

        TryCloseMag();

        return true;
    }

    public bool TryCloseMag()
    {
        if (!_unloaded || IsUnloading) return false;

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
        IsUnloading = true; 
    }

    public virtual void TryUnloadAllBullets()
    {
        AmmoLeft = 0;
    }

    public void SpawnBulletParticles(int amount)
    {
        if (Projectile.TryGetComponent(out RangedProjectile rangedProjectile))
        {
            _bulletParticleSpawner.SpawnMultipleParticles(rangedProjectile.BulletCasingParticle, amount, 0.05f);
        }
    }

    public void SetReloadSpeed(float value)
    {
        _animator.SetFloat(ANIMATOR_RELOAD_SPEED_PROP_NAME, value);
    }

    //OVERRIDES
    protected override void VirtualOnEnable()
    {
        base.VirtualOnEnable();

        IsReloading = false;
        IsUnloading = false;
    }

    protected override bool AttackCondition()
    {
        return base.AttackCondition() && LoadedLivingAmmoLeft > 0 && !IsReloading && !Unloaded && (!RandomManager.Instance?.ProcRandomBadChance(JamChance) ?? false);
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
        _animator.SetBool(ANIMATOR_IS_RELOADING_PROP_NAME, true);
    }

    protected virtual void OnReloadFinish()
    {
        IsReloading = false;
        _animator.SetBool(ANIMATOR_IS_RELOADING_PROP_NAME, false);
    }

    protected override bool OnTryAttackSuccess(Vector2 direction)
    {
        SpendAmmo();
        _shootLightParticleSpawner?.SpawnParticle();
        return base.OnTryAttackSuccess(direction);
    }

    protected override void OnTryAttackFail(Vector2 direction)
    {
        base.OnTryAttackFail(direction);

        if (SpawnParticleOnUnableToAttackCondition())
        {
            SoundOnOutOfAmmo.PlaySound();
            _cloudParticleSpawner.SpawnParticle();
        }
    }

    protected virtual bool SpawnParticleOnUnableToAttackCondition()
    {
        return !IsReloading && !IsInCooldown;
    }

    /// <summary>
    /// must be called only from animation controllers
    /// </summary>
    public virtual void OnLoadFinish()
    {
        OnLoaded?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// must be called only from animation controllers
    /// </summary>
    public virtual void OnUnloadFinish()
    {
        IsUnloading = false;
        OnUnloaded?.Invoke(this, EventArgs.Empty);
    }

    public abstract int GetAmmoCapacity();
}
