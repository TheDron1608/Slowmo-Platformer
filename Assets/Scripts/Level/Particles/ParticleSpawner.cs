using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSpawner : MonoBehaviour
{
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
        return SpawnParticle(
                particle,
                transform.position,
                VectorMath.Quartenion2DToVec2(transform.rotation),
                SpawnVelocity,
                SpawnAngularVeclocity,
                OverrideEffectMaterial != null ? OverrideEffectMaterial : (GameObjectUtility.TryGetComponentInSelfOrParent(gameObject, out SpriteRenderer selfSprite) ? selfSprite.sharedMaterial : null),
                LayerManager.Instance.GetZLayerOfGameObject(gameObject)
                );
    }

    public void SpawnMultipleParticles(int amount, float duration = 0.05f)
    {
        SpawnMultipleParticles(DefaultParticle, amount, duration);
    }

    public void SpawnMultipleParticles(AbstractParticle particle, int amount, float duration = 0.05f)
    {
        if (amount > 1)
        {
            StartCoroutine(SpawnMultiplieParticles(particle, amount, duration));
        }
        else
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
        float minSpawnVelocity,
        float maxSpawnVelocity,
        float minSpawnAngularVelocity,
        float maxSpawnAngularVelocity,
        Material material,
        ZIndexLayer layer,
        int amount,
        float accuracy = 1f
        )
    {
        List<AbstractParticle> result = new(amount);
        for (int i = 0; i < amount; i++)
        {
            result.Insert(i, SpawnParticle(
                NumberMath.PickRandomItem(particles),
                position,
                VectorMath.RandomizeVec2(direction, accuracy),
                NumberMath.PickRandomInRangeNoSeed(minSpawnVelocity, maxSpawnVelocity),
                NumberMath.PickRandomInRangeNoSeed(minSpawnAngularVelocity, maxSpawnAngularVelocity),
                material,
                layer
                ));
        }

        return result;
    }

    public static AbstractParticle SpawnParticle(
        AbstractParticle particle, 
        Vector2 position, 
        Vector2 direction, 
        float spawnVelocity, 
        float spawnAngularVelocity, 
        Material material, 
        ZIndexLayer layer
        )
    {
        AbstractParticle spawnParticle = ParticlesManager.Instance.GetUnusedPhysicsParticle(particle);

        spawnParticle.SetParticleAttrs(
            position,
            direction,
            spawnVelocity,
            spawnAngularVelocity,
            material,
            layer,
            GameObjectUtility.GetComponentWithPossibleFail<SpriteRenderer>(particle.gameObject)?.sprite,
            GameObjectUtility.GetComponentWithPossibleFail<Animator>(particle.gameObject),
            GameObjectUtility.GetComponentWithPossibleFail<BoxCollider2D>(particle.gameObject),
            particle.gameObject.name
            );

        return spawnParticle;
    }
}
