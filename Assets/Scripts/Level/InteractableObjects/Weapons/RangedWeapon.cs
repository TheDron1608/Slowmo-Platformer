using UnityEngine;

public class RangedWeapon : Weapon
{
    public const string ANIMATOR_RELOAD_TRIGGER_NAME = "Reload";
    public const string ANIMATOR_RELOAD_SPEED_PROP_NAME = "ReloadSpeed";

    public int AmmoLeft;
    public int MaxAmmo;
    public int LoadedAmmoLeft;

    public bool TryReload()
    {
        if (AmmoLeft > 0 && LoadedAmmoLeft < MaxAmmo)
        {
            OnReload();
            return true;
        }
        else
        {
            return false;
        }
    }

    protected virtual void OnReload()
    {
        _animator.SetTrigger(ANIMATOR_RELOAD_TRIGGER_NAME);
    }

    public void SetReloadSpeed(float value)
    {
        _animator.SetFloat(ANIMATOR_RELOAD_SPEED_PROP_NAME, value);
    }
}
