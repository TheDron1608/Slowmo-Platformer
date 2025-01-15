using System.Collections;
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
    public int MaxLoadedAmmo = 1;
    public int LoadedLivingAmmoLeft = 1;
    public int LoadedSpentAmmoLeft = 0;
    public int AmmoAmountPerReload = 1;
    public int AmmoAmountPerUnload = 1;
    public bool MagReload = false;
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

    protected override void OnAwake()
    {
        base.OnAwake();

        _projectileSpawnPosition = transform.Find(PROJECTILE_SPAWN_POSITION_GAMEOBJECT_NAME);
    }

    public bool TryReload()
    {
        if (LoadedLivingAmmoLeft < MaxLoadedAmmo && AmmoLeft > 0 && LoadedLivingAmmoLeft < MaxAmmo)
        {
            _unloaded = false;
            OnReload();
            return true;
        }
        else if (LoadedLivingAmmoLeft <= 0)
        {
            TryUnload();
        }
        return false;
    }

    public bool TryUnload()
    {
        if (_unloaded) return false;

        _animator.SetBool(ANIMATOR_UNLOADED_PROP_NAME, true);
        _unloaded = true;
        return true;
    }

    protected virtual void OnReload()
    {
        _animator.SetBool(ANIMATOR_UNLOADED_PROP_NAME, true);
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

        if (LoadedLivingAmmoLeft <= 0) return;

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

    private void SpawnProjectile(float accuracity)
    {
        BulletProjectile projectile = Instantiate(BulletProjectile, _projectileSpawnPosition);
        projectile.MoveAlign = VectorMath.RandomizeQuarternion(projectile.transform.rotation, accuracity);
        projectile.transform.parent = LayerManager.Instance.GetZLayerOfGameObject(projectile.gameObject).transform;
    }

    private void SpawnBullet()
    {
        LoadedLivingAmmoLeft--;
        LoadedSpentAmmoLeft++;
        SpawnProjectile(BurstAccuracy);
    }

    private void SpawnBuckshot()
    {
        LoadedLivingAmmoLeft--;
        LoadedSpentAmmoLeft++;
        for (int i = 0; i < BuckshotProjectilesAmount; i++)
        {
            SpawnProjectile(BuckshotAccuracy);
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
