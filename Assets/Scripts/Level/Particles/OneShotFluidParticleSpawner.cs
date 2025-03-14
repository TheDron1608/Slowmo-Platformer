using System.Collections;
using UnityEngine;

public class OneShotFluidParticleSpawner : FluidParticleSpawner
{
    protected override void OnAwake()
    {
        base.OnAwake();
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
        if (FluidParticleManager.Instance != null)
        {
            FluidParticleManager.Instance.OnSpawningFluidParticlesFinish -= Instance_OnSpawningFluidParticlesFinish;
        }
    }
}
