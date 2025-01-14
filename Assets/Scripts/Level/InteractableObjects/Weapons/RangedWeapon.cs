using UnityEngine;

public class RangedWeapon : Weapon
{
    public const string ANIMATOR_RELOAD_TRIGGER_NAME = "Reload";
    public const string ANIMATOR_UNLOADED_PROP_NAME = "Unloaded";
    public const string ANIMATOR_ISTHROWN_PROP_NAME = "IsThrown";
    public const string ANIMATOR_RELOAD_SPEED_PROP_NAME = "ReloadSpeed";

    public int AmmoLeft;
    public int MaxAmmo;
    public int LoadedAmmoLeft;

    private bool _unloaded = false;

    public bool TryReload()
    {
        if (AmmoLeft > 0 && LoadedAmmoLeft < MaxAmmo)
        {
            _unloaded = false;
            OnReload();
            return true;
        }
        else
        {
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
}
