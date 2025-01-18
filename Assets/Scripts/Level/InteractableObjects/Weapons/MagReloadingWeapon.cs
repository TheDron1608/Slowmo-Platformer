using System.Collections;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// used for weapon with manazines link assault rifle or auto shotgun
/// </summary>
public class MagReloadingWeapon : RangedWeapon
{
    const float AWAIT_TIME_TO_SPAWN_BULLET_PARTICLE_ON_ATTACK = 0.1f; //in seconds
    const string MAGS_PARTICLE_SPAWNER_GAMEOBJECT_NAME = "MagParticleSpawner";

    [Header("Mag reloading weapon")]
    [SerializeField] private int _magSize = 16;
    [SerializeField] private int _maxMags = 2;
    [SerializeField] private int _mags = 1;

    private ParticleSpawner _magsPraticleSpawner;

    public int MagSize
    {
        get => _magSize;
        set
        {
            _magSize = value;
            MaxAmmo = _maxMags * _magSize;
        }
    }
    public int MaxMags
    {
        get => _maxMags;
        set
        {
            _maxMags = value;
            MaxAmmo = _maxMags * _magSize;
        }
    }
    public int Mags
    {
        get => _mags;
        set
        {
            _mags = value;
            AmmoLeft = _magSize + (AmmoLeft > 0 ? 1 : 0);
        }
    }

    public override bool GetIsNeedReload()
    {
        return AmmoLeft <= 0;
    }

    protected override void OnAwake()
    {
        base.OnAwake();

        _magsPraticleSpawner = transform.Find(MAGS_PARTICLE_SPAWNER_GAMEOBJECT_NAME).GetComponent<ParticleSpawner>();
    }

    public void SpawnMagParticle()
    {
        _magsPraticleSpawner.SpawnParticle(1);
    }

    protected override bool ReloadCondition()
    {
        return AmmoLeft <= MagSize;
    }

    protected override bool AttackCondition()
    {
        return base.AttackCondition() && AmmoLeft > 0;
    }

    protected override void OnReload()
    {
        if (Mags <= 0)
        {
            TryUnload();
            return;
        }

        base.OnReload();
    }

    protected override bool OnTryAttack()
    {
        if (!base.OnTryAttack()) return false;

        StartCoroutine(SpawnParticleAfterDuration());
        return true;
    }

    public override void OnUnloadFinish()
    {
        base.OnUnloadFinish();

        SpawnMagParticle();
    }

    public override void OnLoadFinish()
    {
        base.OnLoadFinish();

        Mags--;
    }

    protected override void OnPickedUp()
    {
        base.OnPickedUp();

        if (Unloaded && Mags > 0)
        {
            TryCloseMag();
        }
    }

    private IEnumerator SpawnParticleAfterDuration()
    {
        yield return new WaitForSeconds(AWAIT_TIME_TO_SPAWN_BULLET_PARTICLE_ON_ATTACK);
        SpawnBulletParticles(1);
    }


    protected override void SpawnBullet()
    {
        base.SpawnBullet();
        AmmoLeft--;
    }

    protected override void SpawnBuckshot()
    {
        base.SpawnBuckshot();
        AmmoLeft--;
    }

    protected override IEnumerator SpawnBurst()
    {
        for (int i = 0; i < BuckshotProjectilesAmount; i++)
        {
            if (AmmoLeft <= 0) break;
            SpawnBullet();
            yield return new WaitForSeconds(DurationBetweenBurstProjectiles);
        }
    }

    protected override IEnumerator SpawnBuckshotBurst()
    {
        for (int i = 0; i < BuckshotProjectilesAmount; i++)
        {
            if (AmmoLeft <= 0) break;
            SpawnBuckshot();
            yield return new WaitForSeconds(DurationBetweenBurstProjectiles);
        }
    }
}
