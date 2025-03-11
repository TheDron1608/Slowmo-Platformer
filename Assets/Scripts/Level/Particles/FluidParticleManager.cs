using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class FluidParticleManager : MonoBehaviour
{
    const float SPEED_TO_USE_DRIP_FULID = 5f;

    const float BASE_FLUID_LIFE_TIME = 0.25f;

    const int BASE_GIB_PARTICLES_AMOUNT = 10;
    const float BASE_GIB_VELOCITY = 2.5f;

    public enum FluidParticlesSpreadTypes
    {
        GIB
    }

    public static FluidParticleManager Instance;

    private void Awake()
    {
        if (Instance != null) throw new UnityException("limit of 1 FluidParticleManager per scene");
        Instance = this;
    }

    public List<FluidParticle> BlobParticles;
    public List<FluidParticle> DripParticles;
    public float FluidMultiplier = 1f;

    public void SpawnFluidParticle(GameObject source, FluidParticlesSpreadTypes spreadType, Quaternion? direction = null)
    {
        SpawnFluidParticle(VectorMath.Vec3ToVec2(source.transform.position), LayerManager.Instance.GetZLayerOfGameObject(source), spreadType, direction);
    }

    public void SpawnFluidParticle(Vector2 postion, ZIndexLayer zLayer, FluidParticlesSpreadTypes spreadType, Quaternion? direction = null)
    {
        switch (spreadType)
        {
            case FluidParticlesSpreadTypes.GIB:
                for (int i = 0; i < BASE_GIB_PARTICLES_AMOUNT * FluidMultiplier; i++)
                {
                    Quaternion randomDirection = new();
                    randomDirection.eulerAngles = new Vector3(0f, 0f, Random.value * 360f);
                    SpawnSingleFluidParticle(postion, zLayer, randomDirection, BASE_GIB_VELOCITY, BASE_FLUID_LIFE_TIME);
                }
                break;
        }
    }

    private void SpawnSingleFluidParticle(Vector2 postion, ZIndexLayer zLayer, Quaternion direction, float velocity, float lifeTime)
    {
        FluidParticle newParticle;
        Vector3 spawnPosition = VectorMath.Vec2ToVec3(postion, zLayer.transform.position.z);
        Quaternion spawnRotation = direction;

        if (velocity > SPEED_TO_USE_DRIP_FULID)
        {
            newParticle = Instantiate(DripParticles[(int)(Random.value * DripParticles.Count)], spawnPosition, spawnRotation);
        }
        else
        {
            newParticle = Instantiate(BlobParticles[(int)(Random.value * BlobParticles.Count)], spawnPosition, spawnRotation);
        }
        newParticle.SetProperties(VectorMath.Quartenion2DToVec2(direction) * velocity, lifeTime);
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}
