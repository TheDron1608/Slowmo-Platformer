using System.Collections;
using Unity.Mathematics;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;

public class ParticleSpawner : MonoBehaviour
{
    public GameObject Particle;
    public float SpawnVelocity = 1f;
    public float SpawnAngle = 0f;
    public float SpawnAngularVeclocity = 0f;
    public float DurationBetweenSpawningParticles = 0.05f; //in seconds

    public void SpawnParticle(int amount = 1)
    {
        SpawnParticleProcess(amount);
    }

    private void SpawnParticleProcess(int amount)
    {
        if (Particle.TryGetComponent(out PhysicsParticle physicsParticle))
        {
            StartCoroutine(SpawnPhysicsParticles(physicsParticle, amount));
        }
        else if (Particle.TryGetComponent(out ParticleSystem particleSystemParticle))
        {
            StartCoroutine(SpawnPaticleSystemParticle(particleSystemParticle, amount));
        }
    }

    private IEnumerator SpawnPhysicsParticles(PhysicsParticle physicsParticle, int amount)
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

                newParticleRigidBody.angularVelocity = SpawnAngularVeclocity;
            }

            yield return new WaitForSeconds(DurationBetweenSpawningParticles);
        }
    }

    private IEnumerator SpawnPaticleSystemParticle(ParticleSystem particleSystemParticle, int amount)
    {
        ParticleSystem newParticle = Instantiate(particleSystemParticle, LayerManager.Instance.GetZLayerOfGameObject(gameObject).transform);

        ParticleSystem.Burst firstBurst = newParticle.emission.GetBurst(0);
        firstBurst.count = amount;
        newParticle.emission.SetBurst(0, firstBurst);

        newParticle.transform.position = transform.position;
        newParticle.transform.rotation = transform.rotation;

        yield return new WaitForSeconds(DurationBetweenSpawningParticles);
    }
}
