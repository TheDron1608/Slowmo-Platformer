using System.Collections;
using UnityEngine;

public class FluidParticleSpawner : MonoBehaviour
{
    public FluidParticleManager.FluidParticlesSpreadTypes FluidParticlesSpreadType;

    public void SpawnParticle()
    {
        FluidParticleManager.Instance.SpawnFluidParticles(gameObject, FluidParticlesSpreadType, transform.rotation);
    }
}
