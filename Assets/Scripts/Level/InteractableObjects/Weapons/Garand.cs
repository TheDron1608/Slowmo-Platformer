using UnityEngine;

public class Garand : BulletReloadingWeapon
{
    const string RELOAD_MAG_ANIMATOR_TRIGGER_NAME = "ReloadMag";

    public SoundPlayer SoundOnUnloadedAllAmmo;
    public SoundPlayer SoundOnLoadMag;
    public ParticleSpawner ParticleOnUnloadedAllAmmo;

    protected override void OnReload()
    {
        if (LoadedLivingAmmoLeft == 0)
        {
            _animator.SetTrigger(RELOAD_MAG_ANIMATOR_TRIGGER_NAME);
        }
        base.OnReload();
    }

    protected override bool OnTryAttackSuccess(Vector2 direction)
    {
        bool result = base.OnTryAttackSuccess(direction);

        SpawnBulletParticles(1);
        LoadedSpentAmmoLeft--;

        if (LoadedLivingAmmoLeft == 0)
        {
            ParticleOnUnloadedAllAmmo.SpawnParticle();
            SoundOnUnloadedAllAmmo.PlaySound();
        }

        return result;
    }
}