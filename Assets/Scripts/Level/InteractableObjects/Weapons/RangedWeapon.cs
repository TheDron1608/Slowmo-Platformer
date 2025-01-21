using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class RangedWeapon : Weapon
{
    public enum ProjectileType
    {
        BULLET,
        BUCKSHOT,
        BURST,
        BUCKSHOT_BURST
    }

    const string PROJECTILE_SPAWN_POSITION_GAMEOBJECT_NAME = "ProjectileSpawnPosition";
    const string BULLET_PARTICLE_SPAWNER_GAMEOBJECT_NAME = "BulletParticleSpawner";
    const string CLOUD_PARTICLE_SPAWNER_GAMEOBJECT_NAME = "CloudParticleSpawner";
    const string ANIMATOR_FINISH_RELOAD_TRIGGER_NAME = "FinishReload";

    public const string ANIMATOR_RELOAD_TRIGGER_NAME = "Reload";
    public const string ANIMATOR_UNLOADED_PROP_NAME = "Unloaded";
    public const string ANIMATOR_ISTHROWN_PROP_NAME = "IsThrown";
    public const string ANIMATOR_RELOAD_SPEED_PROP_NAME = "ReloadSpeed";

    [Header("Ranged weapon")]
    public int AmmoLeft = 10;
    public int MaxAmmo = 10;
    public int LoadedLivingAmmoLeft = 1;
    public int LoadedSpentAmmoLeft = 0;

    public BulletProjectile BulletProjectile;
    public ProjectileType AttackType = ProjectileType.BULLET;
    /// <summary>
    /// 0 is perfect accuracy, 1 is 360deg spread
    /// </summary>
    public float BulletAccuracy = 1;
    /// <summary>
    /// 0 is perfect accuracy, 1 is 360deg spread
    /// </summary>
    public int BuckshotProjectilesAmount = 6;
    public float BuckshotAccuracy = 0.75f;
    /// <summary>
    /// if higher than 0, each projectile will spawn DurationBetweenBurstProjectiles seconds after previous spawned projectile
    /// </summary>
    public float DurationBetweenBurstProjectiles = 0.167f;
    public int BurstProjectilesAmount = 3;
    public float BurstAccuracy = 0.9f;

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
            IsAbleToAttack = !value && !IsReloading;
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

        IsAbleToAttack = true;
        IsReloading = false;
        _animator.SetTrigger(ANIMATOR_FINISH_RELOAD_TRIGGER_NAME);

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
        _bulletParticleSpawner.SpawnParticle(amount);
    }

    public void SetReloadSpeed(float value)
    {
        _animator.SetFloat(ANIMATOR_RELOAD_SPEED_PROP_NAME, value);
    }



    //OVERRIDES
    protected override bool AttackCondition()
    {
        return base.AttackCondition() && LoadedLivingAmmoLeft > 0;
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
        IsAbleToAttack = false;
        Unloaded = true;
        _animator.SetTrigger(ANIMATOR_RELOAD_TRIGGER_NAME);
    }

    protected virtual void OnReloadFinish()
    {
        IsReloading = false;
        IsAbleToAttack = true;
    }

    protected override void OnThrow()
    {
        base.OnThrow();

        _animator.SetBool(ANIMATOR_ISTHROWN_PROP_NAME, true);
        SetReloadSpeed(1f);
    }

    protected override void OnPickedUp()
    {
        base.OnPickedUp();

        _animator.SetBool(ANIMATOR_ISTHROWN_PROP_NAME, false);

        if (CurrentHolder.TryGetComponent(out CharacterReloading currentHolderReloadingComponent))
        {
            SetReloadSpeed(currentHolderReloadingComponent.ReloadSpeed);
        }
    }

    protected override bool OnTryAttackSuccess()
    {
        base.OnTryAttackSuccess();

        //spawning projectiles
        switch (AttackType)
        {
            case ProjectileType.BULLET:
                SpawnBullet();
                break;
            case ProjectileType.BUCKSHOT:
                SpawnBuckshot();
                break;
            case ProjectileType.BURST:
                SpawnBurst();
                break;
            case ProjectileType.BUCKSHOT_BURST:
                SpawnBuckshotBurst();
                break;
        }

        //knockback
        if (CurrentHolder.TryGetComponent(out Rigidbody2D rigidBody))
        {
            Vector2 aimDirection = transform.right;

            rigidBody.linearVelocity += aimDirection * KnockBack;

            if (CurrentHolder.TryGetComponent(out CharacterVisual charVisual))
            {
                charVisual.SpritesFlipped = aimDirection.x < 0f;
            }
        }

        return true;
    }

    protected override void OnTryAttackFail()
    {
        base.OnTryAttackFail();

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
        IsAbleToAttack = !IsReloading;
        Unloaded = false;
    }

    /// <summary>
    /// must be called only from animation controllers
    /// </summary>
    public virtual void OnUnloadFinish()
    {
        IsAbleToAttack = false;
        Unloaded = true;
    }

    //PROJECTILE SPAWNER METHODS
    private void SpawnProjectile(float accuracity)
    {
        BulletProjectile projectile = Instantiate(BulletProjectile, _projectileSpawnPosition);
        projectile.MoveAlign = VectorMath.RandomizeQuarternion(projectile.transform.rotation, accuracity);
        projectile.transform.parent = LayerManager.Instance.GetZLayerOfGameObject(projectile.gameObject).transform;
        projectile.InitializeOwner(this);
    }

    private void SpawnBullet()
    {
        LoadedLivingAmmoLeft--;
        LoadedSpentAmmoLeft++;
        SpawnProjectile(BulletAccuracy);
    }

    private void SpawnBuckshot()
    {
        LoadedLivingAmmoLeft--;
        LoadedSpentAmmoLeft++;
        for (int i = 0; i < BuckshotProjectilesAmount; i++)
        {
            SpawnProjectile(BuckshotAccuracy + (1 - BuckshotAccuracy) * i / BuckshotProjectilesAmount);
        }
    }

    private IEnumerator SpawnBurst()
    {
        for (int i = 0; i < BuckshotProjectilesAmount; i++)
        {
            if (LoadedLivingAmmoLeft <= 0) break;
            SpawnBullet();
            yield return new WaitForSeconds(DurationBetweenBurstProjectiles);
        }
    }

    private IEnumerator SpawnBuckshotBurst()
    {
        for (int i = 0; i < BuckshotProjectilesAmount; i++)
        {
            if (LoadedLivingAmmoLeft <= 0) break;
            SpawnBuckshot();
            yield return new WaitForSeconds(DurationBetweenBurstProjectiles);
        }
    }
}
