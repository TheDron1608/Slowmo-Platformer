using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class CharacterLoseLimbParticleSpawner : AbstractCharacterComponent
{
    const float SPAWNING_FREQ = 4f;
    const float SPAWNING_DURATION = 5f;
    const float SPAWNING_FREQ_ON_ALIVE = 1.75f;

    const float SPAWNING_ACCURACY = 0.75f;
    const float SPAWNING_MAX_VELOCITY = 7f;
    const float SPAWNING_MIN_VELOCITY = 3.5f;

    [SerializeField] private Transform _lastLostLimbPosition;

    private CharacterPart.PartTypes? _lastRemovedPart = null;
    private float _timeSinceLastLostLimb = 999999f;
    private float _timeSinceLastParticleSpawn = 999999f;
    private List<AbstractParticle> _spawnParticles = new();

    public void OnRemovedLimbPart(CharacterLimbPart value)
    {
        _lastLostLimbPosition.transform.position = new Vector3(
            value.Collider.bounds.center.x,
            value.Collider.bounds.center.y,
            CharComponents.transform.position.z
            );
        _lastRemovedPart = value.PartType;
        _spawnParticles = value.CharPartHealth.ParticlesOnHit;
        _timeSinceLastLostLimb = 0f;
        _timeSinceLastParticleSpawn = 0f;
    }

    private void FixedUpdate()
    {

        if (!_lastRemovedPart.HasValue || _spawnParticles.Count == 0) return;
        float globalMult = ParticleSpawner.CalculateParticleAmountGlobalMultiplierByParticle(_spawnParticles[0]);
        if (globalMult <= 0f) return;

        _timeSinceLastLostLimb += Time.deltaTime;
        _timeSinceLastParticleSpawn += Time.deltaTime;


        if (
            _timeSinceLastLostLimb < SPAWNING_DURATION &&
            _timeSinceLastParticleSpawn * SPAWNING_FREQ * globalMult > _timeSinceLastLostLimb
            )
        {
            SpawnParticle(false, math.max(1f, globalMult));
            _timeSinceLastParticleSpawn = 0f;
        }
        else if
            (
            _timeSinceLastLostLimb > SPAWNING_DURATION &&
            CharComponents.CharacterPartsManager.GetCharacterPart(_lastRemovedPart.Value) == null &&
            _timeSinceLastParticleSpawn * globalMult > 1f / SPAWNING_FREQ_ON_ALIVE &&
            !CharComponents.CharacterEffectsReceiver.GetHasEffect<ILethalEffect>()
            )
        {
            SpawnParticle(true, math.max(1f, globalMult));
            _timeSinceLastParticleSpawn = 0f;
        }
    }

    private void SpawnParticle(bool ignoreTime, float speedMult)
    {
        ParticleSpawner.SpawnParticle(
            NumberMath.PickRandomItem(_spawnParticles),
            transform.position,
            VectorMath.RandomizeVec2(_lastLostLimbPosition.transform.position - transform.position, SPAWNING_ACCURACY)
                * new Vector2(CharComponents.CharacterVisual.FlippedH ? -1f : 1f, 1f),
            0f,
            math.lerp(SPAWNING_MIN_VELOCITY, SPAWNING_MAX_VELOCITY * speedMult, ignoreTime ? 0.5f : 1f - _timeSinceLastLostLimb / SPAWNING_DURATION)
                * NumberMath.PickRandomInRangeNoSeed(SPAWNING_ACCURACY, 2f - SPAWNING_ACCURACY),
            0f,
            CharComponents.CharacterEffectsReceiver.EffectMaterial,
            CharComponents.CharacterCollision.CurrentZLayer
            );
    }
}
