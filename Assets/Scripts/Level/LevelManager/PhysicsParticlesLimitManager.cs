using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PhysicsParticlesLimitManager : MonoBehaviour
{
    public int MaxParticles = 128;
    public float MaxSimulatedParticlesLife = 15f;
    public float ParticleDissapearDuration = 2f;

    public List<PhysicsParticle> ParticlesOnLevel;

    public static PhysicsParticlesLimitManager Instance;

    private void Awake()
    {
        ParticleSpawner.OnPhysicsParticleSpawned += ParticleSpawner_OnPhysicsParticleSpawned;
        ParticlesOnLevel = new(MaxParticles);
        if (Instance != null) throw new UnityException("Limit of 1 PhysicsParticleLimitManager Instance per level");
        Instance = this;
    }

    public void RemovePhysicsParticle(PhysicsParticle physicsParticle)
    {
        if (physicsParticle != null && !physicsParticle.IsDestroyed())
        {
            StartCoroutine(RemoveParticleProcess(physicsParticle));
        }
    }

    private void ParticleSpawner_OnPhysicsParticleSpawned(object sender, PhysicsParticle e)
    {
        ParticlesOnLevel.Add(e);
        if (ParticlesOnLevel.Count > MaxParticles)
        {
            RemovePhysicsParticle(ParticlesOnLevel[0]);
            ParticlesOnLevel.RemoveAt(0);
        }

        StartCoroutine(RemoveWhenMaxParticleLifeIsOutCoroutine(e));
    }

    private IEnumerator RemoveParticleProcess(PhysicsParticle physicsParticle)
    {
        if (physicsParticle.TryGetComponent(out SpriteRenderer physicsParticleSpriteRenderer)) 
        {
            while (physicsParticleSpriteRenderer.color.a > 0f)
            {
                physicsParticleSpriteRenderer.color = new Color(
                    physicsParticleSpriteRenderer.color.r,
                    physicsParticleSpriteRenderer.color.g,
                    physicsParticleSpriteRenderer.color.b,
                    physicsParticleSpriteRenderer.color.a - Time.deltaTime
                    );
                yield return new WaitForEndOfFrame();
            }
        }

        if (physicsParticle != null && !physicsParticle.gameObject.IsDestroyed())
        {
            Destroy(physicsParticle.gameObject);
        }
    }

    private IEnumerator RemoveWhenMaxParticleLifeIsOutCoroutine(PhysicsParticle physicsParticle)
    {
        yield return new WaitForSeconds(MaxSimulatedParticlesLife);
        if (physicsParticle.EnabledPhysics)
        {
            RemoveParticleProcess(physicsParticle);
        }
    }

    private void OnDestroy()
    {
        ParticleSpawner.OnPhysicsParticleSpawned -= ParticleSpawner_OnPhysicsParticleSpawned;
    }
}
