using UnityEngine;

public class RangedWeapon : Weapon
{
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
    public BulletProjectile BulletProjectile;

    private bool _unloaded = false;
    private Transform _projectileSpawnPosition;

    protected override void OnAwake()
    {
        base.OnAwake();

        _projectileSpawnPosition = transform.Find(PROJECTILE_SPAWN_POSITION_GAMEOBJECT_NAME);
    }

    public bool TryReload()
    {
        if (AmmoLeft > 0 && LoadedLivingAmmoLeft < MaxAmmo)
        {
            _unloaded = false;
            OnReload();
            return true;
        }
        else
        {
            TryUnload();
            return false;
        }
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

        BulletProjectile projectile = Instantiate(BulletProjectile, _projectileSpawnPosition);
        projectile.MoveAlign = transform.right;
        projectile.transform.parent = LayerManager.Instance.GetZLayerOfGameObject(projectile.gameObject).transform;

        LoadedLivingAmmoLeft--;
        LoadedSpentAmmoLeft++;
    }
}
