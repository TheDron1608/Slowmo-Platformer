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
    const string ANIMATOR_FINISH_RELOAD_TRIGGER_NAME = "FinishReload";

    public const string ANIMATOR_RELOAD_TRIGGER_NAME = "Reload";
    public const string ANIMATOR_UNLOADED_PROP_NAME = "Unloaded";
    public const string ANIMATOR_ISTHROWN_PROP_NAME = "IsThrown";
    public const string ANIMATOR_RELOAD_SPEED_PROP_NAME = "ReloadSpeed";

    [Header("Ranged weapon")]
    public int AmmoLeft = 10;
    public int MaxAmmo = 10;

    public BulletProjectile BulletProjectile;
    public ProjectileType AttackType = ProjectileType.BULLET;
    /// <summary>
    /// 0 is perfect accuracy, 1 is 360deg spread
    /// </summary>
    public float BulletAccuracy = 0;
    /// <summary>
    /// 0 is perfect accuracy, 1 is 360deg spread
    /// </summary>
    public int BuckshotProjectilesAmount = 6;
    public float BuckshotAccuracy = 0.5f;
    /// <summary>
    /// if higher than 0, each projectile will spawn DurationBetweenBurstProjectiles seconds after previous spawned projectile
    /// </summary>
    public float DurationBetweenBurstProjectiles = 0.167f;
    public int BurstProjectilesAmount = 3;
    public float BurstAccuracy = 0.1f;

    private bool _isReloading = false;
    private bool _unloaded = false;
    private Transform _projectileSpawnPosition;
    private ParticleSpawner _bulletParticleSpawner;

    protected override void OnAwake()
    {
        base.OnAwake();

        _projectileSpawnPosition = transform.Find(PROJECTILE_SPAWN_POSITION_GAMEOBJECT_NAME);
        _bulletParticleSpawner = transform.Find(BULLET_PARTICLE_SPAWNER_GAMEOBJECT_NAME).GetComponent<ParticleSpawner>();
    }

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

    public virtual bool GetIsNeedReload()
    {
        return false;
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

    protected virtual bool ReloadCondition()
    {
        return AmmoLeft > 0;
    }

    protected virtual bool UnloadCondition()
    {
        return false;
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

        Unloaded = true;
        return true;
    }

    public void SpawnBulletParticles(int amount)
    {
        _bulletParticleSpawner.SpawnParticle(amount);
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

    public void SetReloadSpeed(float value)
    {
        _animator.SetFloat(ANIMATOR_RELOAD_SPEED_PROP_NAME, value);
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

    protected override bool OnTryAttack()
    {
        base.OnTryAttack();

        if (!AttackCondition()) return false;

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

    protected override bool AttackCondition()
    {
        return base.AttackCondition();
    }

    protected void SpawnProjectile(float accuracity)
    {
        BulletProjectile projectile = Instantiate(BulletProjectile, _projectileSpawnPosition);
        projectile.MoveAlign = VectorMath.RandomizeQuarternion(projectile.transform.rotation, accuracity);
        projectile.transform.parent = LayerManager.Instance.GetZLayerOfGameObject(projectile.gameObject).transform;
        projectile.InitializeOwner(this);
    }

    protected virtual void SpawnBullet()
    {
        SpawnProjectile(BurstAccuracy);
    }

    protected virtual void SpawnBuckshot()
    {
        float currentBuckshotAccuracystep = BuckshotAccuracy / BuckshotProjectilesAmount;
        for (int i = 1; i <= BuckshotProjectilesAmount; i++)
        {
            SpawnProjectile(currentBuckshotAccuracystep * i);
        }
    }

    protected virtual IEnumerator SpawnBurst()
    {
        for (int i = 0; i < BuckshotProjectilesAmount; i++)
        {
            SpawnBullet();
            yield return new WaitForSeconds(DurationBetweenBurstProjectiles);
        }
    }

    protected virtual IEnumerator SpawnBuckshotBurst()
    {
        for (int i = 0; i < BuckshotProjectilesAmount; i++)
        {
            SpawnBuckshot();
            yield return new WaitForSeconds(DurationBetweenBurstProjectiles);
        }
    }
}
