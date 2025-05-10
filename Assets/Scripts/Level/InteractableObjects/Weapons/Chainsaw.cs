using System.Collections;
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
    public float FullUnpowerRequiredTime = 5f; //in seconds
    public float ChanceToSucessStart = 0.1f;

    private float _chainsawPowerLeft = 0f;
    /// <summary>
    /// Value between 1 and 0, increases by ChanceToSucessLoad if previous attempt to load was failed
    /// </summary>
    private float _currentChanceToSuccessStart = 0f;
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
    }

    /// <summary>
    /// Value between 1 and 0, recreases every frame, when reaches 0 unloads  itself
    /// </summary>
    public float ChainsawPowerLeft
    {
        get => _chainsawPowerLeft;
        set => _chainsawPowerLeft = value;
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
            _started = value;
            ChainsawPowerLeft = 1f;
            _animator.SetBool(ANIMATOR_STARTED_PROP_NAME, value);
            if (value)
            {
                _passiveCloudsSpawnerCoroutine = StartCoroutine(PassiveCloudsSpawn());
            }
            else
            {
                if (_passiveCloudsSpawnerCoroutine != null)
                {
                    StopCoroutine(_passiveCloudsSpawnerCoroutine);
                }
            }
        }
    }

    public bool TryStart()
    {
        if (Started || IsStarting) return false;

        _animator.SetTrigger(ANIMATOR_START_TRIGGER_NAME);
        IsStarting = true;

        return true;
    }

    public bool ForceTryStart()
    {
        IsStarting = false;

        if (Started) return false;

        _currentChanceToSuccessStart += ChanceToSucessStart;
        if (Random.value < _currentChanceToSuccessStart)
        {
            _currentChanceToSuccessStart = 0f;
            Started = true;
            return true;
        }
        else
        {
            return false;
        }
    }

    public override bool AttackCondition()
    {
        return base.AttackCondition() && Started && !IsStarting;
    }

    private IEnumerator PassiveCloudsSpawn()
    {
        while (true)
        {
            if (Random.value < CLOUDS_PARTICLE_SPAWN_CHANCE)
            {
                _cloudsParticleSpawner.SpawnParticle();
            }
            yield return new WaitForSeconds(CLOUDS_PARTICLE_SPAWN_DURATION);
        }
    }

    private void FixedUpdate()
    {
        UpdateChainsawPower();
    }

    private void UpdateChainsawPower()
    {
        if (ChainsawPowerLeft > 0f && IsInCooldown)
        {
            ChainsawPowerLeft -= Time.fixedDeltaTime / FullUnpowerRequiredTime;
            if (_chainsawPowerLeft < 0f)
            {
                ChainsawPowerLeft = 0f;
                Started = false;
            }
        }
        else if (ChainsawPowerLeft <= 1f && !IsInCooldown)
        {
            ChainsawPowerLeft += Time.fixedDeltaTime / FullUnpowerRequiredTime;
            if (ChainsawPowerLeft > 1f)
            {
                ChainsawPowerLeft = 1f;
            }
        }
    }
}
