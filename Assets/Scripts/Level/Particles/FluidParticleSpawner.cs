using System.Collections;
using UnityEngine;

public class FluidParticleSpawner : MonoBehaviour
{
    public FluidParticleManager.FluidParticlesSpreadTypes FluidParticlesSpreadType;
    public Material FluidMaterial;
    [SerializeField] private bool _playOnAwake = false;

    private void Awake()
    {
        OnAwake();
    }

    protected virtual void OnAwake()
    {
        if (_playOnAwake)
        {
            SpawnParticle();
        }
    }

    public void SpawnParticle()
    {
        FluidParticleManager.Instance.SpawnFluidParticles(gameObject, FluidParticlesSpreadType, transform.rotation, FluidMaterial);
    }
}
