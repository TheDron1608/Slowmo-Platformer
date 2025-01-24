using System.Collections;
using UnityEngine;

public class ParticleSpawner : MonoBehaviour
{
    public GameObject Particle;
    public float SpawnVelocity = 1f;
    public float SpawnAngle = 0f;
    public float SpawnAngularVeclocity = 0f;

    public void SpawnParticle(int amount = 1, float duration = 0.05f)
    {
        SpawnParticleProcess(amount, duration);
    }

    private void SpawnParticleProcess(int amount, float duration)
    {
        if (Particle.TryGetComponent(out PhysicsParticle physicsParticle))
        {
            StartCoroutine(SpawnPhysicsParticles(physicsParticle, amount, duration));
        }
        else if (Particle.TryGetComponent(out ParticleSystem particleSystemParticle))
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

                newParticleRigidBody.angularVelocity = SpawnAngularVeclocity;
            }

            yield return new WaitForSeconds(duration);
        }
    }

    private IEnumerator SpawnPaticleSystemParticle(ParticleSystem particleSystemParticle, int amount, float duration)
    {
        ParticleSystem newParticle = Instantiate(particleSystemParticle, LayerManager.Instance.GetZLayerOfGameObject(gameObject).transform);

        ParticleSystem.Burst firstBurst = newParticle.emission.GetBurst(0);
        firstBurst.count = amount;
        newParticle.emission.SetBurst(0, firstBurst);

        newParticle.transform.position = transform.position;
        newParticle.transform.rotation = transform.rotation;

        yield return new WaitForSeconds(duration);
    }
}
