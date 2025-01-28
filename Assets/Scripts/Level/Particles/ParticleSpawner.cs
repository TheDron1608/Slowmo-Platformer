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

    public static event EventHandler<PhysicsParticle> OnPhysicsParticleSpawned;
    public static event EventHandler<ParticleSystem> OnParticleSystemSpawned;

    public void SpawnParticle(int amount = 1, float duration = 0.05f, GameObject particle = null)
    {
        SpawnParticleProcess(amount, duration, particle ?? DefaultParticle);
    }

    private void SpawnParticleProcess(int amount, float duration, GameObject particle)
    {
        if (particle.TryGetComponent(out PhysicsParticle physicsParticle))
        {
            StartCoroutine(SpawnPhysicsParticles(physicsParticle, amount, duration));
        }
        else if (particle.TryGetComponent(out ParticleSystem particleSystemParticle))
        {
            StartCoroutine(SpawnPaticleSystemParticle(particleSystemParticle, amount, duration));
        }
    }

    private IEnumerator SpawnPhysicsParticles(PhysicsParticle physicsParticle, int amount, float duration)
    {
        for (int i = 0; i < amount; i++)
        {
            PhysicsParticle newParticle = Instantiate(physicsParticle, LayerManager.Instance.GetZLayerOfGameObject(gameObject).transform);

            newParticle.transform.position = transform.position;
            newParticle.transform.rotation = transform.rotation;

            if (newParticle.TryGetComponent(out Rigidbody2D newParticleRigidBody))
            {
                Quaternion spawnAnleQuarternion = transform.parent.rotation;
                spawnAnleQuarternion.eulerAngles = new Vector3(
                    0f,
                    spawnAnleQuarternion.eulerAngles.y,
                    spawnAnleQuarternion.eulerAngles.z + SpawnAngle
                    );

                newParticleRigidBody.linearVelocity = VectorMath.Quartenion2DToVec2(spawnAnleQuarternion) * SpawnVelocity;
                newParticleRigidBody.linearVelocity = new Vector2(
                    newParticleRigidBody.linearVelocity.x + (UnityEngine.Random.value * SPAWN_VELOCITY_EXTRA_RANOM_MULTIPLIER * SpawnVelocity),
                    newParticleRigidBody.linearVelocity.y + (UnityEngine.Random.value * SPAWN_VELOCITY_EXTRA_RANOM_MULTIPLIER * SpawnVelocity)
                    );

                newParticleRigidBody.angularVelocity = SpawnAngularVeclocity * (UnityEngine.Random.value * 2 - 1);
            }

            OnPhysicsParticleSpawned?.Invoke(this, newParticle);

            yield return new WaitForSeconds(duration);
        }
    }

    private IEnumerator SpawnPaticleSystemParticle(ParticleSystem particleSystemParticle, int amount, float duration)
    {
        Vector3 eulerAngle = transform.rotation.eulerAngles;

        ParticleSystem newParticle = Instantiate(particleSystemParticle, LayerManager.Instance.GetZLayerOfGameObject(gameObject).transform);

        ParticleSystem.Burst firstBurst = newParticle.emission.GetBurst(0);
        firstBurst.count = amount;
        newParticle.emission.SetBurst(0, firstBurst);
        if (eulerAngle.y > 90f)
        {
            var main = newParticle.main;
            main.startSpeedMultiplier = -1;
        }

        newParticle.transform.position = transform.position;
        newParticle.transform.eulerAngles = new Vector3(
            eulerAngle.x,
            0f,
            eulerAngle.z
            );

        OnParticleSystemSpawned?.Invoke(this, newParticle);

        yield return new WaitForSeconds(duration);
    }
}
