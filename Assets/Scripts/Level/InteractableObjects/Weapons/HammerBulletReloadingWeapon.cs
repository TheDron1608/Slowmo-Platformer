using UnityEngine;

public class HammerBulletReloadingWeapon : BulletReloadingWeapon
{
    const string ANIMATOR_IS_HAMMERRED_PROP_NAME = "IsHammerred";

    [Header("Hammer Bullet Reloading weapon")]
    public float UnhammeredAttackAttcuracyMultplier = .8f;
    public SoundPlayer SoundOnHammer;

    private bool _hammered = false;
    private bool _isHammerring = false;

    public bool Hammered
    {
        get => _hammered;
        private set 
        {
            if (value && value != _hammered)
            {
                AccuracyMultiplier /= UnhammeredAttackAttcuracyMultplier;
            }
            else if (!value && value != _hammered)
            {
                AccuracyMultiplier *= UnhammeredAttackAttcuracyMultplier;
            }

            IsHammerring = false;

            _animator.SetBool(ANIMATOR_IS_HAMMERRED_PROP_NAME, value);
            _hammered = value;
        }
    }

    public bool TrySetHammered(bool value)
    {
        if (Hammered || IsReloading) return false;

        _animator.SetBool(ANIMATOR_IS_HAMMERRED_PROP_NAME, value);
        return true;
    }

    public void ForceSetHammered(bool value)
    {
        Hammered = value;
    }

    protected override void OnAwake()
    {
        base.OnAwake();

        AccuracyMultiplier *= UnhammeredAttackAttcuracyMultplier;
    }

    public bool IsHammerring
    {
        get => _isHammerring;
        set => _isHammerring = value;
    }

    /// <summary>
    /// must be called only from animator
    /// </summary>
    public virtual void OnFinishHammerring()
    {
        IsHammerring = false;
        ForceSetHammered(true);
    }

    protected override bool ReloadCondition()
    {
        return base.ReloadCondition() && !IsHammerring;
    }

    protected override void OnReload()
    {
        TrySetHammered(false);
        base.OnReload();
    }
}
