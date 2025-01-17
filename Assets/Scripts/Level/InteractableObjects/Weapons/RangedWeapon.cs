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

    public const string ANIMATOR_RELOAD_TRIGGER_NAME = "Reload";
    public const string ANIMATOR_UNLOADED_PROP_NAME = "Unloaded";
    public const string ANIMATOR_ISTHROWN_PROP_NAME = "IsThrown";
    public const string ANIMATOR_RELOAD_SPEED_PROP_NAME = "ReloadSpeed";

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

    private bool _unloaded = false;
    private Transform _projectileSpawnPosition;
    private ParticleSpawner _particleSpawner;

    protected override void OnAwake()
    {
        base.OnAwake();

        _projectileSpawnPosition = transform.Find(PROJECTILE_SPAWN_POSITION_GAMEOBJECT_NAME);
        _particleSpawner = transform.GetComponentInChildren<ParticleSpawner>(true);
    }

    public bool Unloaded
    {
        get => _unloaded;
        set
        {
            _animator.SetBool(ANIMATOR_UNLOADED_PROP_NAME, value);
            _unloaded = value;
        }
    }

    public bool TryReload()
    {
        if (ReloadCondition())
        {
            _unloaded = false;
            OnReload();
            return true;
        }
        else if (UnloadCondition())
        {
            TryUnload();
        }
        return false;
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
        _particleSpawner.SpawnParticle(amount);
    }

    protected virtual void OnReload()
    {
        Unloaded = true;
        _animator.SetTrigger(ANIMATOR_RELOAD_TRIGGER_NAME);
    }

    public void SetReloadSpeed(float value)
    {
        _animator.SetFloat(ANIMATOR_RELOAD_SPEED_PROP_NAME, value);
    }

    protected override void OnThrow()
    {
        base.OnThrow();

        _animator.SetBool(ANIMATOR_ISTHROWN_PROP_NAME, true);
    }

    protected override void OnPickedUp()
    {
        base.OnPickedUp();

        _animator.SetBool(ANIMATOR_ISTHROWN_PROP_NAME, false);
    }

    protected override void OnAttack()
    {
        base.OnAttack();

        if (!AttackCondition()) return;

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
    }

    protected virtual bool AttackCondition()
    {
        return !Unloaded;
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
