using System.Collections;
using UnityEngine;

/// <summary>
/// used for pump shotguns and sniper rifles
/// </summary>
public class BoltReloadingWeapon : BulletReloadingWeapon
{
    const string ANIMATOR_FINISH_REALOAD_TRIGGER_NAME = "FinishReload";
    const string ANIMATOR_UNLOAD_BULLET_TRIGGER_NAME = "UnloadBullet";
    const string ANIMATOR_OUT_OF_AMMO_PROP_NAME = "OutOfAmmo";
    const float WAIT_DURATION_TO_UNLOAD_BULLET_AFTER_ATTACK = 0.25f; //in seconds

    private bool _outOfAmmo = false;

    public bool OutOfAmmo
    {
        get => _outOfAmmo;
        set
        {
            _animator.SetBool(ANIMATOR_OUT_OF_AMMO_PROP_NAME, value);
            _outOfAmmo = value;
        }
    }

    public void FinishReload()
    {
        _animator.SetTrigger(ANIMATOR_FINISH_REALOAD_TRIGGER_NAME);
    }

    public void UnloadBullet()
    {
        _animator.SetTrigger(ANIMATOR_UNLOAD_BULLET_TRIGGER_NAME);
    }

    protected override bool OnTryAttack()
    {
        if (Unloaded)
        {
            FinishReload();
        }

        if (!base.OnTryAttack()) return false;

        StartCoroutine(UnloadBulletAfterDelay());
        return true;
    }

    protected override bool AttackCondition()
    {
        return base.AttackCondition() && LoadedSpentAmmoLeft < 1;
    }

    protected override bool ReloadCondition()
    {
        return base.ReloadCondition();
    }

    private IEnumerator UnloadBulletAfterDelay()
    {
        yield return new WaitForSeconds(WAIT_DURATION_TO_UNLOAD_BULLET_AFTER_ATTACK);
        UnloadBullet();
    }

    public override void OnLoadFinish()
    {
        base.OnLoadFinish();

        if (LoadedLivingAmmoLeft >= MaxLoadedAmmo || AmmoLeft <= 1)
        {
            Debug.Log("finish");
            FinishReload();
            OutOfAmmo = false;
        }
    }

    public override void OnUnloadFinish()
    {
        base.OnUnloadFinish();

        if (AmmoLeft <= 0 && LoadedLivingAmmoLeft <= 0)
        {
            OutOfAmmo = true;
        }
    }
}
