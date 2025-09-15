using System;
using System.Collections;
using UnityEngine;

public class ParticleSpawner : MonoBehaviour
{
    const float SPAWN_VELOCITY_EXTRA_RANOM_MULTIPLIER = .25f;

    public GameObject DefaultParticle;
    public float SpawnVelocity = 1f;
    public float SpawnAngle = 0f;
    public float SpawnAngularVeclocity = 0f;
    public Material OverrideEffectMaterial = null;
    public bool OneShot = false;

    public static event EventHandler<PhysicsParticle> OnPhysicsParticleSpawned;
    public static event EventHandler<ParticleSystem> OnParticleSystemSpawned;

    public static GameObject SpawnParticle(
        GameObject particle,
        Transform source,
        float SpawnVelocity = 1f,
        float SpawnAngularVelocity = 0f,
        float SpawnAngle = 0f,
        Material OverrideEffectMaterial = null,
        Vector2? overridePosition = null,
        Quaternion? overrideRotation = null
        )
    {
        if (particle.TryGetComponent(out PhysicsParticle physicsParticle))
        {
            return SpawnPhysicsParticle(
                physicsParticle,
                source,
                SpawnVelocity,
                SpawnAngularVelocity,
                SpawnAngle,
                OverrideEffectMaterial,
                overridePosition,
                overrideRotation
                ).gameObject;
        }
        else if (particle.TryGetComponent(out ParticleSystem particleSystemParticle))
        {
            return SpawnPaticleSystemParticle(
                particleSystemParticle,
                source,
                SpawnVelocity,
                OverrideEffectMaterial,
                overridePosition,
                overrideRotation
                ).gameObject;
        }
        throw new UnityException("ParticlesSpawner.SpawnParticle particle argument must be gameObject with ParticleSystem or PhysicsParticle components");
    }

    public void SpawnParticle(GameObject overrideParticle = null)
    {
        GameObject currentParticle = overrideParticle ?? DefaultParticle;

        if (currentParticle.TryGetComponent(out PhysicsParticle physicsParticle))
        {
            SpawnPhysicsParticle(
                physicsParticle,
                transform,
                SpawnVelocity,
                SpawnAngularVeclocity,
                SpawnAngle,
                OverrideEffectMaterial
                );
        }
        else if (currentParticle.TryGetComponent(out ParticleSystem particleSystemParticle))
        {
            SpawnPaticleSystemParticle(
                particleSystemParticle,
                transform,
                SpawnVelocity,
                OverrideEffectMaterial
                );
        }
    }

    public void SpawnParticle(int amount, float duration = 0.05f, GameObject overrideParticle = null)
    {
        StartCoroutine(SpawnMultiplieParticles(amount,duration, overrideParticle));
    }
    private IEnumerator SpawnMultiplieParticles(int amount, float duration = 0.05f, GameObject overrideParticle = null)
    {
        for (int i = 0; i < amount; i++)
        {
            SpawnParticle(overrideParticle);
            yield return new WaitForSeconds(duration);
        }
    }

    private static PhysicsParticle SpawnPhysicsParticle(
        PhysicsParticle particle,
        Transform source,
        float SpawnVelocity = 1f,
        float SpawnAngularVelocity = 0f,
        float SpawnAngle = 0f,
        Material overrideEffectMaterial = null,
        Vector2? overridePosition = null,
        Quaternion? overrideRotation = null
        )
    {
        PhysicsParticle newParticle = Instantiate(particle, LayerManager.Instance.GetZLayerOfGameObject(source.gameObject).PhysicsParticlesContainer);

        newParticle.transform.position = VectorMath.Vec2ToVec3(overridePosition.GetValueOrDefault(source.position), newParticle.transform.position.z);
        newParticle.transform.rotation = overrideRotation.GetValueOrDefault(source.rotation);

        if (newParticle.TryGetComponent(out SpriteRenderer newParticleSpriteRenderer))
        {
            if (overrideEffectMaterial != null) newParticleSpriteRenderer.sharedMaterial = overrideEffectMaterial;
            else if (source.TryGetComponent(out SpriteRenderer soruceSprite)) newParticleSpriteRenderer.sharedMaterial = soruceSprite.sharedMaterial;
            else if (source.parent.TryGetComponent(out soruceSprite)) newParticleSpriteRenderer.sharedMaterial = soruceSprite.sharedMaterial;
        }

        if (newParticle.TryGetComponent(out Rigidbody2D newParticleRigidBody))
        {
            Quaternion spawnAnleQuarternion = overrideRotation.GetValueOrDefault(source.rotation);
            spawnAnleQuarternion.eulerAngles = new Vector3(
                0f,
                spawnAnleQuarternion.eulerAngles.y,
                spawnAnleQuarternion.eulerAngles.z + SpawnAngle
                );

            newParticleRigidBody.linearVelocity = VectorMath.Quartenion2DToVec3(spawnAnleQuarternion) * SpawnVelocity;
            newParticleRigidBody.linearVelocity = new Vector2(
                newParticleRigidBody.linearVelocity.x + (UnityEngine.Random.value * SPAWN_VELOCITY_EXTRA_RANOM_MULTIPLIER * SpawnVelocity),
                newParticleRigidBody.linearVelocity.y + (UnityEngine.Random.value * SPAWN_VELOCITY_EXTRA_RANOM_MULTIPLIER * SpawnVelocity)
                );

            newParticleRigidBody.angularVelocity = SpawnAngularVelocity * (UnityEngine.Random.value * 2 - 1);
        }

        if (newParticle is CharacterPartPhysicsParticle charPartPhysicsParticle)
        {
            charPartPhysicsParticle.CharacterPart = source.GetComponent<CharacterPart>() ?? source.GetComponentInParent<CharacterPart>();
        }

        OnPhysicsParticleSpawned?.Invoke(source, newParticle);

        return newParticle;
    }

    private static ParticleSystem SpawnPaticleSystemParticle(
        ParticleSystem particle,
        Transform source,
        float SpawnVelocity = 1f,
        Material overrideEffectMaterial = null,
        Vector2? overridePosition = null,
        Quaternion? overrideRotation = null
        )
    {
        ZIndexLayer layer = LayerManager.Instance.GetZLayerOfGameObject(source.gameObject);
        ParticleSystem newParticle = Instantiate(particle, layer.PhysicsParticlesContainer);

        LayerManager.Instance.ChangeZIndexForGameObject(layer, newParticle.gameObject);

        ParticleSystem.Burst firstBurst = newParticle.emission.GetBurst(0);
        firstBurst.count = 1;
        
        newParticle.emission.SetBurst(0, firstBurst);

        var main = newParticle.main;
        main.startSpeed = SpawnVelocity;

        newParticle.transform.position = VectorMath.Vec2ToVec3(overridePosition.GetValueOrDefault(source.position), newParticle.transform.position.z);
        Vector3 eulerAngle = overrideRotation.GetValueOrDefault(source.rotation).eulerAngles;
        newParticle.transform.eulerAngles = new Vector3(
            eulerAngle.x,
            eulerAngle.y > 90f ? 180f : 0f,
            eulerAngle.z
            );

        /*
        if (newParticle.TryGetComponent(out ParticleSystemRenderer newParticleSystemRenderer))
        {
            if (overrideEffectMaterial != null) newParticleSystemRenderer.sharedMaterial = overrideEffectMaterial;
            else if (source.TryGetComponent(out SpriteRenderer soruceSprite)) newParticleSystemRenderer.sharedMaterial = soruceSprite.sharedMaterial;
            else if (source.parent.TryGetComponent(out soruceSprite)) newParticleSystemRenderer.sharedMaterial = soruceSprite.sharedMaterial;
        }
        */

        OnParticleSystemSpawned?.Invoke(source, newParticle);

        return newParticle;
    }
}
