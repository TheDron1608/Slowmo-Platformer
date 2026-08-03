using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class ParticleSpawner : MonoBehaviour
{
    const float PARTICLE_VELOCITY_ON_AMOUNT_MULTIPLIER_DEPENDENCE = 0.667f;

    public AbstractParticle DefaultParticle;
    public float SpawnVelocity = 1f;
    public float SpawnAngle = 0f;
    public float SpawnAngularVeclocity = 0f;
    public Material OverrideEffectMaterial = null;

    public AbstractParticle SpawnParticle()
    {
        return SpawnParticle(DefaultParticle);
    }

    public AbstractParticle SpawnParticle(AbstractParticle particle)
    {
        ZIndexLayer layer = LayerManager.Instance.GetZLayerOfGameObject(gameObject);
        if (layer == null) return null;

        return SpawnParticle(
                particle,
                transform.position,
                VectorMath.Quartenion2DToVec2(transform.rotation),
                SpawnAngle,
                SpawnVelocity * CalculateParticleAmountGlobalMultiplierByParticle(particle) * PARTICLE_VELOCITY_ON_AMOUNT_MULTIPLIER_DEPENDENCE,
                SpawnAngularVeclocity,
                OverrideEffectMaterial != null ? OverrideEffectMaterial : (GameObjectUtility.TryGetComponentInSelfOrParent(gameObject, out SpriteRenderer selfSprite) ? selfSprite.sharedMaterial : null),
                layer
                );
    }

    public void SpawnMultipleParticles(int amount, float duration = 0.05f)
    {
        SpawnMultipleParticles(DefaultParticle, amount, duration);
    }

    public void SpawnMultipleParticles(AbstractParticle particle, int amount, float duration = 0.05f)
    {
        int multipliedAmount = (int)math.round(amount * CalculateParticleAmountGlobalMultiplierByParticle(particle));

        if (multipliedAmount > 1)
        {
            StartCoroutine(SpawnMultiplieParticles(particle, multipliedAmount, duration));
        }
        else if (multipliedAmount == 1)
        {
            SpawnParticle(particle);
        }
    }
    private IEnumerator SpawnMultiplieParticles(AbstractParticle particle, int amount, float duration = 0.05f)
    {
        for (int i = 0; i < amount; i++)
        {
            SpawnParticle(particle);
            yield return new WaitForSeconds(duration);
        }
    }

    public static List<AbstractParticle> SpawnInstantlyMultipleParticles(
        List<AbstractParticle> particles,
        Vector2 position,
        Vector2 direction,
        float angle,
        float minSpawnVelocity,
        float maxSpawnVelocity,
        float minSpawnAngularVelocity,
        float maxSpawnAngularVelocity,
        Material material,
        ZIndexLayer layer,
        int amount,
        float accuracy = 1f,
        bool enablePhysics = true,
        bool pickRandomly = true
        )
    {
        float particleMultiplier = CalculateParticleAmountGlobalMultiplierByParticle(particles.FirstOrDefault());
        int multipliedAmount = (int)math.round(amount * particleMultiplier);
        List<AbstractParticle> result = new(multipliedAmount);
        for (int i = 0; i < multipliedAmount; i++)
        {
            AbstractParticle newParticle = SpawnParticle(
                pickRandomly ? NumberMath.PickRandomItem(particles) : particles[i % particles.Count],
                position,
                VectorMath.RandomizeVec2(direction, accuracy),
                angle,
                NumberMath.PickRandomInRangeNoSeed(minSpawnVelocity, maxSpawnVelocity) * particleMultiplier * PARTICLE_VELOCITY_ON_AMOUNT_MULTIPLIER_DEPENDENCE,
                NumberMath.PickRandomInRangeNoSeed(minSpawnAngularVelocity, maxSpawnAngularVelocity),
                material,
                layer,
                enablePhysics
                );

            if (newParticle != null) result.Insert(i, newParticle);
        }

        return result;
    }

    public static AbstractParticle SpawnParticle(
        AbstractParticle particle,
        Vector2 position,
        Vector2 direction,
        float angle,
        float spawnVelocity,
        float spawnAngularVelocity,
        Material material,
        ZIndexLayer layer,
        bool enablePhysics = true
        )
    {
        if (particle == null) return null;

        AbstractParticle spawnParticle = ParticlesManager.Instance.GetUnusedParticle(particle);

        spawnParticle.SetParticleAttrs(
            particle,
            position,
            direction,
            angle,
            spawnVelocity,
            spawnAngularVelocity,
            material,
            layer,
            enablePhysics
            );

        return spawnParticle;
    }

    private static float CalculateParticleAmountGlobalMultiplierByParticle( AbstractParticle particle)
    {
        if (particle is PhysicsParticle)
        {
            return  ParticlesManager.Instance.PhysicsParticlesGlobalSpawnAmountMultiplier;
        }
        else if (particle is FluidParticle)
        {
            return ParticlesManager.Instance.FluidParticlesGlobalSpawnAmountMultiplier;
        }
        else if (particle is CloudParticle)
        {
            return ParticlesManager.Instance.CloudParticlesGlobalSpawnAmountMultiplier;
        }
        else
        {
            return 1f;
        }
    }
}
