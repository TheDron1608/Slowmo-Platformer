using System.Collections;
using UnityEngine;

public class OneShotFluidParticleSpawner : FluidParticleSpawner
{
    private void Awake()
    {
        FluidParticleManager.Instance.OnSpawningFluidParticlesFinish += Instance_OnSpawningFluidParticlesFinish;
    }

    private void Instance_OnSpawningFluidParticlesFinish(object sender, GameObject e)
    {
        if (e == gameObject)
        {
            GameObject.Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        FluidParticleManager.Instance.OnSpawningFluidParticlesFinish -= Instance_OnSpawningFluidParticlesFinish;
    }
}
