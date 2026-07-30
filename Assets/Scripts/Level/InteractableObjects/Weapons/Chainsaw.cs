using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class Chainsaw : MeleeWeapon
{
    const string ANIMATOR_STARTED_PROP_NAME = "Started";
    const string ANIMATOR_START_TRIGGER_NAME = "Start";

    const float CHAINSAW_STARTED_VOLUME_INCREASE_DURATION = 0.5f;
    const float CHAINSAW_STARED_MIN_PITCH = 0.5f;
    const float CLOUDS_PARTICLE_SPAWN_DURATION = 0.5f;
    const float CLOUDS_PARTICLE_SPAWN_CHANCE = 0.75f;

    const string CLOUDS_PARTICLE_SPAWNER_GAMEOBJECT_NAME = "CloudsParticleSpawner";
    
    public enum ChainsawStartState
    {
        FAIL,
        SUCCESS,
        OUT_OF_FUEL
    }

    [Header("Chainsaw")]
    public float MaxFuel = 10f;
    public float FuelLeft = 10f;
    public float MaxStartSuccessChance = .75f;
    public float MinStartSuccessChance = 0.25f;
    public float MaxJampChancePerSecond = 0.667f;
    public float MinJamChancePerSecond = 0f;
    public AbstractSoundPlayer SoundOnTryStart;
    public AbstractSoundPlayer SoundOnSuccessStart;
    public AbstractSoundPlayer SoundOnOutOfFuel;
    public AbstractSoundPlayer PassiveSoundOnStarted;

    private bool _isStarting = false;
    private ChainsawStartState _startingState = ChainsawStartState.FAIL;
    private bool _started = false;
    private float _passiveSoundProgress = 0f;

    private ParticleSpawner _cloudsParticleSpawner;
    private Coroutine _passiveCloudsSpawnerCoroutine;

    protected override void OnAwake()
    {
        base.OnAwake();

        _cloudsParticleSpawner = transform.Find(CLOUDS_PARTICLE_SPAWNER_GAMEOBJECT_NAME).GetComponent<ParticleSpawner>();
    }

    public override string GetAmmoInfoOnSelect()
    {
        if (FuelLeft > 0)
        {
            return FuelLeft.ToString("0.0") + " / " + MaxFuel;
        }
        else
        {
            return "<color=red>" + FuelLeft.ToString("0.0") + " / " + MaxFuel + "</color>";
        }
    }

    protected override void VirtualOnEnable()
    {
        base.VirtualOnEnable();
        IsStarting = false;
    }

    public bool IsStarting
    {
        get => _isStarting;
        private set => _isStarting = value;
    }

    public ChainsawStartState StartingState
    {
        get => _startingState;
        private set => _startingState = value;
    }

    public bool Started
    {
        get => _started;
        set
        {
            if (_started == value) return;
            if (value && IsThrown) return;

            _started = value;
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

        if (FuelLeft <= 0)
        {
            _startingState = ChainsawStartState.OUT_OF_FUEL;
        }
        else if (UnityEngine.Random.value > math.lerp(MinStartSuccessChance, MaxStartSuccessChance, FuelLeft / MaxFuel))
        {
            _startingState = ChainsawStartState.FAIL;
        }
        else
        {
            _startingState = ChainsawStartState.SUCCESS;
        }

        _animator.SetTrigger(ANIMATOR_START_TRIGGER_NAME);

        IsStarting = true;

        return true;
    }

    public bool OnTryStartFinish()
    {
        IsStarting = false;

        if (Started) return false;

        if (_startingState == ChainsawStartState.SUCCESS)
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
        if (Started || (IsStarting && StartingState == ChainsawStartState.SUCCESS))
        {
            _passiveSoundProgress = NumberMath.LimitFloatBetweenZeroAndOne(_passiveSoundProgress + Time.fixedDeltaTime / CHAINSAW_STARTED_VOLUME_INCREASE_DURATION);
        }
        else
        {
            _passiveSoundProgress = NumberMath.LimitFloatBetweenZeroAndOne(_passiveSoundProgress - Time.fixedDeltaTime / CHAINSAW_STARTED_VOLUME_INCREASE_DURATION);
        }

        if(_passiveSoundProgress > 0f)
        {
            PassiveSoundOnStarted.Volume = _passiveSoundProgress;
            PassiveSoundOnStarted.Pitch = math.max(_passiveSoundProgress, CHAINSAW_STARED_MIN_PITCH);
            if (!PassiveSoundOnStarted.GetIsPlaying()) PassiveSoundOnStarted.PlaySound(true);
        }
        else
        {
            PassiveSoundOnStarted.BreakAllSounds();
        }

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
