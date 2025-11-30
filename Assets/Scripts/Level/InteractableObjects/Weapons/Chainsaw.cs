using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class Chainsaw : MeleeWeapon
{
    const string ANIMATOR_STARTED_PROP_NAME = "Started";
    const string ANIMATOR_START_TRIGGER_NAME = "Start";

    const float CLOUDS_PARTICLE_SPAWN_DURATION = 0.5f;
    const float CLOUDS_PARTICLE_SPAWN_CHANCE = 0.75f;

    const string CLOUDS_PARTICLE_SPAWNER_GAMEOBJECT_NAME = "CloudsParticleSpawner";
    const string KNOCKBACK_COLLIDER_GAMEOBJECT_NAME = "KnockbackCollider";

    [Header("Chainsaw")]
    public float MaxFuel = 10f;
    public float MaxStartSuccessChance = .75f;
    public float MinStartSuccessChance = 0.25f;
    public float MaxJampChancePerSecond = 0.667f;
    public float MinJamChancePerSecond = 0f;
    public SoundPlayer SoundOnTryStart;
    public AudioSource PassiveSoundOnStarted;

    private float _fuelLeft;


    private bool _isStarting = false;
    private bool _started = false;

    private Collider2D _colliderComponent;
    private Rigidbody2D _rigidBodyComponent;
    private ParticleSpawner _cloudsParticleSpawner;
    private Coroutine _passiveCloudsSpawnerCoroutine;

    protected override void OnAwake()
    {
        base.OnAwake();
        if (!TryGetComponent(out _colliderComponent)) throw new UnityException("Collider2D component not found");
        if (!TryGetComponent(out _rigidBodyComponent)) throw new UnityException("RigidBody2D component not found");
        _cloudsParticleSpawner = transform.Find(CLOUDS_PARTICLE_SPAWNER_GAMEOBJECT_NAME).GetComponent<ParticleSpawner>();
        FuelLeft = MaxFuel;
    }

    public bool IsStarting
    {
        get => _isStarting;
        private set => _isStarting = value;
    }

    public bool Started
    {
        get => _started;
        set
        {
            if (_started == value) return;

            _started = value;
            _animator.SetBool(ANIMATOR_STARTED_PROP_NAME, value);
            if (value)
            {
                _passiveCloudsSpawnerCoroutine = StartCoroutine(PassiveCloudsSpawn());
                PassiveSoundOnStarted.Play();
            }
            else
            {
                PassiveSoundOnStarted.Stop();
                if (_passiveCloudsSpawnerCoroutine != null)
                {
                    StopCoroutine(_passiveCloudsSpawnerCoroutine);
                }
            }
        }
    }

    public float FuelLeft
    {
        get => _fuelLeft;
        set => _fuelLeft = value;
    }

    public bool TryStart()
    {
        if (Started || IsStarting) return false;

        _animator.SetTrigger(ANIMATOR_START_TRIGGER_NAME);
        SoundOnTryStart.PlaySound();

        IsStarting = true;

        return true;
    }

    public bool OnTryStartFinish()
    {
        IsStarting = false;

        if (Started) return false;

        if (FuelLeft > 0 && UnityEngine.Random.value < math.lerp(MinStartSuccessChance, MaxStartSuccessChance, FuelLeft / MaxFuel))
        {
            Started = true;
            return true;
        }
        else
        {
            return false;
        }
    }

    protected override bool AttackCondition()
    {
        return base.AttackCondition() && Started && !IsStarting;
    }

    private IEnumerator PassiveCloudsSpawn()
    {
        while (true)
        {
            if (UnityEngine.Random.value < CLOUDS_PARTICLE_SPAWN_CHANCE)
            {
                _cloudsParticleSpawner.SpawnParticle();
            }
            yield return new WaitForSeconds(CLOUDS_PARTICLE_SPAWN_DURATION);
        }
    }

    private void FixedUpdate()
    {
        if (Started && Projectiles.Count > 0)
        {
            FuelLeft -= Time.fixedDeltaTime;

            if (FuelLeft <= 0 || UnityEngine.Random.value < math.lerp(MaxJampChancePerSecond, MinJamChancePerSecond, FuelLeft / MaxFuel) * Time.fixedDeltaTime)
            {
                Started = false;
            }
        }
    }
}
