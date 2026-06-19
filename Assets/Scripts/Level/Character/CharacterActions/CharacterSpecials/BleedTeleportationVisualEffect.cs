using Unity.VisualScripting;
using UnityEngine;

public class BleedTeleportationVisualEffect : MonoBehaviour
{
    const float BLEED_MIN_VELOCITY = 1f;
    const float BLEED_MAX_VELOCITY = 3f;
    const float OVERRIDE_PARTICLE_LIFETIME = 0f;
    const float PASSIVE_SOUND_PLAY_DELAY = 0.125f;

    public float Speed = 8.5f;
    public float ParticlesPerSecond = 10f;
    public SoundPlayer PassiveBleedTeleportationSound;

    private CharacterBleedTeleportation _teleportationUser;
    private CharacterComponentsManager _targetTeleportTo;
    private float _timeToNextParticleSpawn = 0;
    private ZIndexLayer _currentZLayer;
    private float _timeSincePassiveSound = 0f;

    public CharacterBleedTeleportation TeleportationUser
    {
        get => _teleportationUser;
        set => _teleportationUser = value;
    }
    public CharacterComponentsManager TargetTeleportTo
    {
        get => _targetTeleportTo;
        set
        {
            _targetTeleportTo = value;
            _currentZLayer = TargetTeleportTo?.CharacterCollision.CurrentZLayer;
            LayerManager.Instance.ChangeZIndexForGameObject(_currentZLayer, gameObject);
        }
    }

    private void Update()
    {
        _timeSincePassiveSound += Time.deltaTime;
        if (_timeSincePassiveSound > PASSIVE_SOUND_PLAY_DELAY)
        {
            _timeSincePassiveSound = 0f;
            PassiveBleedTeleportationSound.PlaySound(false, transform.position);
        }

        if (TeleportationUser == null || TeleportationUser.IsDestroyed())
        {

        }
        else if (_targetTeleportTo == null || _targetTeleportTo.IsDestroyed())
        {
            _teleportationUser.TryFinishTeleport(_targetTeleportTo);
        }
        else
        {
            _teleportationUser.CharComponents.transform.position = transform.position;
            transform.position = VectorMath.Vec2ToVec3(Vector2.MoveTowards(transform.position, _targetTeleportTo.Center.transform.position, Speed * Time.deltaTime), transform.position.z);

            _timeToNextParticleSpawn += Time.deltaTime;

            if (_timeToNextParticleSpawn > 1f / ParticlesPerSecond)
            {
                _timeToNextParticleSpawn = 0f;

                AbstractParticle newParticle = ParticleSpawner.SpawnParticle(
                    NumberMath.PickRandomItem(_teleportationUser.CharComponents.CharacterHealth.ParticlesOnGib),
                    transform.position,
                    VectorMath.PickRandomDirection(),
                    0f,
                    NumberMath.PickRandomInRangeNoSeed(BLEED_MIN_VELOCITY, BLEED_MAX_VELOCITY),
                    0f,
                    _teleportationUser.CharComponents.CharacterEffectsReceiver.EffectMaterial,
                    _currentZLayer
                    );

                if (newParticle is FluidParticle newFluidParticle)
                {
                    newFluidParticle.LifeTime = OVERRIDE_PARTICLE_LIFETIME;
                }
            }

            if (Vector2.Distance(transform.position, _targetTeleportTo.Center.transform.position) < 0.05f)
            {
                _teleportationUser.TryFinishTeleport(_targetTeleportTo);
            }
        }
    }
}