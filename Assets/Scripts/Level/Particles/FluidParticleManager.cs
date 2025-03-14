using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;
using static UnityEditor.Experimental.GraphView.GraphView;
using UnityEngine.UIElements;
using Unity.VisualScripting;

public class FluidParticleManager : MonoBehaviour
{
    const float SPEED_TO_USE_DRIP_FLUID = 3.5f;
    const float HUGE_BLOB_POSITION_SPREAD = 0.15f;

    [Serializable] public class FluidParticleSpreadType
    {
        public float Accuracy;
        public int MinParticles;
        public int MaxParticles;
        public int MaxParticleSize = -1;
        public float MinVelocity;
        public float MaxVelocity;
        public float MinAvgLifeTime;
        public float MaxAvgLifeTime;
        public int HugeBlobs;
        public int Repeat = 1;
        public float MinRepeatDelay = 0.5f;
        public float MaxRepeatDelay = 1f;
    }

    public enum FluidParticlesSpreadTypes
    {
        HEADSHOT,
        DAMAGE,
        LETHAL,
        BLEED
    }

    public static FluidParticleManager Instance;

    public event EventHandler<GameObject> OnSpawningFluidParticlesFinish;

    private void Awake()
    {
        if (Instance != null) throw new UnityException("limit of 1 FluidParticleManager per scene");
        Instance = this;
    }

    public List<FluidParticleSpreadType> FluidParticleSpreadTypes = new();
    public List<FluidParticle> BlobParticles;
    public List<FluidParticle> DripParticles;
    public List<FluidParticle> HugeBlobParticles;
    public float FluidMultiplier = 1f;

    public void SpawnFluidParticles(Vector2 position, Transform parent, ZIndexLayer zLayer, FluidParticlesSpreadTypes spreadType, Quaternion direction)
    {
        GameObject source = new GameObject("fluidSource", typeof(OneShotFluidParticleSpawner));

        source.transform.position = position;
        source.transform.parent = parent;
        source.transform.rotation = direction;

        var sourceFluidParticleManager = source.GetComponent<OneShotFluidParticleSpawner>();
        sourceFluidParticleManager.FluidParticlesSpreadType = spreadType;
        sourceFluidParticleManager.SpawnParticle();
    }


    public void SpawnFluidParticles(GameObject source, FluidParticlesSpreadTypes spreadType, Quaternion direction)
    {
        SpawnFluidParticles(source, FluidParticleSpreadTypes[(int)spreadType], direction);
    }
    public void SpawnFluidParticles(GameObject source, FluidParticleSpreadType spreadType, Quaternion direction)
    {
        int randomizedFluidParticlesAmount = (int)(NumberMath.PickRandomInRangeNoSeed(spreadType.MinParticles, spreadType.MaxParticles) * FluidMultiplier);
        ZIndexLayer zLayer = LayerManager.Instance.GetZLayerOfGameObject(source);    
        for (int i = 0; i < randomizedFluidParticlesAmount; i++)
        {
            if (spreadType.Repeat == 1)
            {
                SpawnSingleRandomizedFluidParticle(source.transform.position, zLayer, spreadType, direction);
                OnSpawningFluidParticlesFinish?.Invoke(this, source);
            }
            else
            {
                StartCoroutine(SpawnMultipleFluidParticles(source, spreadType));
            }
        }
        for (int i = 0; i < spreadType.HugeBlobs; i++)
        {
            Quaternion randomizedRotation = new();
            randomizedRotation.eulerAngles = new Vector3(0f, 0f, UnityEngine.Random.value * 360);
            SpawnSingleHugeFluidParticle(source.transform.position, zLayer, randomizedRotation);
        }
    }

    private IEnumerator SpawnMultipleFluidParticles(GameObject source, FluidParticleSpreadType spreadType)
    {
        ZIndexLayer zLayer = LayerManager.Instance.GetZLayerOfGameObject(source);
        for (int i = 0; i < spreadType.Repeat; i++)
        {
            if (source.IsDestroyed()) break;
            SpawnSingleRandomizedFluidParticle(source.transform.position, zLayer, spreadType, source.transform.rotation);
            yield return new WaitForSeconds(NumberMath.PickRandomInRangeNoSeed(spreadType.MinRepeatDelay, spreadType.MaxRepeatDelay));
        }
        OnSpawningFluidParticlesFinish?.Invoke(this, source);
    }

    private void SpawnSingleRandomizedFluidParticle(Vector2 position, ZIndexLayer zLayer, FluidParticleSpreadType spreadType, Quaternion direction)
    {
        Quaternion randomizedDirection = VectorMath.RandomizeQuarternion(direction, spreadType.Accuracy);
        float randomizedVelocity = NumberMath.PickRandomInRangeNoSeed(spreadType.MinVelocity, spreadType.MaxVelocity);
        float randomizedLifeTime = NumberMath.PickRandomInRangeNoSeed(spreadType.MinAvgLifeTime, spreadType.MaxAvgLifeTime) * (spreadType.MaxVelocity / randomizedVelocity);
        SpawnSingleFluidParticle(position, zLayer, randomizedDirection, randomizedVelocity, randomizedLifeTime, spreadType.MaxParticleSize);
    }

    private void SpawnSingleFluidParticle(Vector2 postion, ZIndexLayer zLayer, Quaternion direction, float velocity, float lifeTime, int maxSize)
    {
        FluidParticle newParticle;
        Vector3 spawnPosition = VectorMath.Vec2ToVec3(postion, zLayer.transform.position.z);
        Quaternion spawnRotation = direction;

        if (velocity > SPEED_TO_USE_DRIP_FLUID)
        {
            newParticle = Instantiate(NumberMath.PickRandomItemNoSeed(DripParticles, maxSize), spawnPosition, spawnRotation, zLayer.transform);
        }
        else
        {
            newParticle = Instantiate(NumberMath.PickRandomItemNoSeed(BlobParticles, maxSize), spawnPosition, spawnRotation, zLayer.transform);
        }
        newParticle.SetProperties(VectorMath.Quartenion2DToVec2(direction) * velocity, lifeTime);
    }

    private void SpawnSingleHugeFluidParticle(Vector2 postion, ZIndexLayer zLayer, Quaternion direction)
    {
        FluidParticle newParticle = Instantiate(
            NumberMath.PickRandomItemNoSeed(HugeBlobParticles), 
            VectorMath.Vec2ToVec3(VectorMath.RandomizeVec2(postion, HUGE_BLOB_POSITION_SPREAD), zLayer.transform.position.z), 
            direction, 
            zLayer.transform
            );
        newParticle.SetProperties(Vector2.zero, 0f);
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}
