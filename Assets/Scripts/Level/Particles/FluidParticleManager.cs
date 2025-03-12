using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;
using UnityEngine.UIElements;
using System;
using Unity.Mathematics;

public class FluidParticleManager : MonoBehaviour
{
    const float SPEED_TO_USE_DRIP_FLUID = 3.5f;

    [Serializable] public class FluidParticleSpreadType
    {
        public float Accuracy;
        public int MinParticles;
        public int MaxParticles;
        public float MinVelocity;
        public float MaxVelocity;
        public float MinAvgLifeTime;
        public float MaxAvgLifeTime;
    }

    public enum FluidParticlesSpreadTypes
    {
        HEADSHOT,
        DAMAGE,
        LETHAL
    }

    public static FluidParticleManager Instance;

    private void Awake()
    {
        if (Instance != null) throw new UnityException("limit of 1 FluidParticleManager per scene");
        Instance = this;
    }

    public List<FluidParticleSpreadType> FluidParticleSpreadTypes = new();
    public List<FluidParticle> BlobParticles;
    public List<FluidParticle> DripParticles;
    public float FluidMultiplier = 1f;

    public void SpawnFluidParticles(GameObject source, FluidParticlesSpreadTypes spreadType, Quaternion direction)
    {
        SpawnFluidParticles(VectorMath.Vec3ToVec2(source.transform.position), LayerManager.Instance.GetZLayerOfGameObject(source), spreadType, direction);
    }

    public void SpawnFluidParticles(Vector2 position, ZIndexLayer zLayer, FluidParticlesSpreadTypes spreadType, Quaternion direction)
    {
        SpawnFluidParticles(position, zLayer, FluidParticleSpreadTypes[(int)spreadType], direction);
    }

    public void SpawnFluidParticles(Vector2 position, ZIndexLayer zLayer, FluidParticleSpreadType spreadType, Quaternion direction)
    {
        int randomizedFluidParticlesAmount = (int)(NumberMath.PickRandomInRangeNoSeed(spreadType.MinParticles, spreadType.MaxParticles) * FluidMultiplier);
        for (int i = 0; i < randomizedFluidParticlesAmount; i++)
        {
            Quaternion randomizedDirection = VectorMath.RandomizeQuarternion(direction, spreadType.Accuracy);
            float randomizedVelocity = NumberMath.PickRandomInRangeNoSeed(spreadType.MinVelocity, spreadType.MaxVelocity);
            float randomizedLifeTime = NumberMath.PickRandomInRangeNoSeed(spreadType.MinAvgLifeTime, spreadType.MaxAvgLifeTime) * (spreadType.MaxVelocity / randomizedVelocity);
            SpawnSingleFluidParticle(position, zLayer, randomizedDirection, randomizedVelocity, randomizedLifeTime);
        }
    }

    private void SpawnSingleFluidParticle(Vector2 postion, ZIndexLayer zLayer, Quaternion direction, float velocity, float lifeTime)
    {
        FluidParticle newParticle;
        Vector3 spawnPosition = VectorMath.Vec2ToVec3(postion, zLayer.transform.position.z);
        Quaternion spawnRotation = direction;

        if (velocity > SPEED_TO_USE_DRIP_FLUID)
        {
            newParticle = Instantiate(NumberMath.PickRandomItemNoSeed(DripParticles), spawnPosition, spawnRotation, zLayer.transform);
        }
        else
        {
            newParticle = Instantiate(NumberMath.PickRandomItemNoSeed(BlobParticles), spawnPosition, spawnRotation, zLayer.transform);
        }
        newParticle.SetProperties(VectorMath.Quartenion2DToVec2(direction) * velocity, lifeTime);
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}
