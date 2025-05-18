using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;
using static UnityEditor.Experimental.GraphView.GraphView;
using UnityEngine.UIElements;
using Unity.VisualScripting;
using Unity.Collections;

public class FluidParticleManager : MonoBehaviour
{
    const float SPEED_TO_USE_DRIP_FLUID = 3.5f;
    const float HUGE_BLOB_PARTICLES_POSITION_SPREAD = 0.15f;
    const float HUGE_BLOB_PARTICLES_ROTATION_SPREAD = 0f;
    const float HUGE_DRIP_PARTICLES_POSITION_SPREAD = 0.05f;
    const float HUGE_DRIP_PARTICLES_ROTATION_SPREAD = 0.958f; //15 degrees

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
        public int HugeDrips;
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

    public List<FluidParticleSpreadType> FluidParticleSpreadTypes = new();
    public List<FluidParticle> BlobParticles;
    public List<FluidParticle> DripParticles;
    public List<FluidParticle> HugeBlobParticles;
    public List<FluidParticle> HugeDripParticles;
    public float FluidMultiplier = 1f;

    [SerializeField] private int _maxFluidParticles = 512;

    private List<FluidParticle> _fluidParticles = new();

    public void OnAddFluidParticle(FluidParticle FluidParticle)
    {
        _fluidParticles.Add(FluidParticle);
        UpdateFluidParticlesLimit();
    }
    public void OnRemoveFluidParticle(FluidParticle FluidParticle)
    {
        _fluidParticles.Remove(FluidParticle);
    }

    private void UpdateFluidParticlesLimit()
    {
        if (_fluidParticles.Count > _maxFluidParticles)
        {
            for (int i = 0; i < _fluidParticles.Count; i++)
            {
                if (_fluidParticles[i].GetIsRemoving() == false)
                {
                    _fluidParticles[i].RemoveFluidParticle();
                    break;
                }
            }
        }
    }

    private void Awake()
    {
        if (Instance != null) throw new UnityException("limit of 1 FluidParticleManager per scene");
        Instance = this;
    }

    public void SpawnFluidParticles(Vector2 position, Transform parent, ZIndexLayer zLayer, FluidParticlesSpreadTypes spreadType, Quaternion direction, Material material)
    {
        GameObject source = new GameObject("fluidSource", typeof(OneShotFluidParticleSpawner));

        source.transform.position = position;
        source.transform.parent = parent;
        source.transform.rotation = direction;

        var sourceFluidParticleManager = source.GetComponent<OneShotFluidParticleSpawner>();
        sourceFluidParticleManager.FluidParticlesSpreadType = spreadType;
        sourceFluidParticleManager.FluidMaterial = material;
        sourceFluidParticleManager.SpawnParticle();
    }

    public void SpawnFluidParticles(GameObject source, FluidParticlesSpreadTypes spreadType, Quaternion direction, Material material)
    {
        SpawnFluidParticles(source, FluidParticleSpreadTypes[(int)spreadType], direction, material);
    }

    public void SpawnFluidParticles(GameObject source, FluidParticleSpreadType spreadType, Quaternion direction, Material material)
    {
        int randomizedFluidParticlesAmount = (int)(NumberMath.PickRandomInRangeNoSeed(spreadType.MinParticles, spreadType.MaxParticles) * FluidMultiplier);
        ZIndexLayer zLayer = LayerManager.Instance.GetZLayerOfGameObject(source);    
        for (int i = 0; i < randomizedFluidParticlesAmount; i++)
        {
            if (spreadType.Repeat == 1)
            {
                SpawnSingleRandomizedFluidParticle(source.transform.position, zLayer, spreadType, direction, material);
                OnSpawningFluidParticlesFinish?.Invoke(this, source);
            }
            else
            {
                StartCoroutine(SpawnMultipleFluidParticles(source, spreadType, material));
            }
        }
        for (int i = 0; i < spreadType.HugeBlobs; i++)
        {
            SpawnSingleHugeBlobFluidParticle(source.transform.position, zLayer, direction, material);
        }
        for (int i = 0; i < spreadType.HugeDrips; i++)
        {
            SpawnSingleHugeDripFluidParticle(source.transform.position, zLayer, direction, material);
        }
    }

    private IEnumerator SpawnMultipleFluidParticles(GameObject source, FluidParticleSpreadType spreadType, Material material)
    {
        ZIndexLayer zLayer = LayerManager.Instance.GetZLayerOfGameObject(source);
        for (int i = 0; i < spreadType.Repeat; i++)
        {
            if (source.IsDestroyed()) break;
            SpawnSingleRandomizedFluidParticle(source.transform.position, zLayer, spreadType, source.transform.rotation, material);
            yield return new WaitForSeconds(NumberMath.PickRandomInRangeNoSeed(spreadType.MinRepeatDelay, spreadType.MaxRepeatDelay));
        }
        OnSpawningFluidParticlesFinish?.Invoke(this, source);
    }

    private void SpawnSingleRandomizedFluidParticle(Vector2 position, ZIndexLayer zLayer, FluidParticleSpreadType spreadType, Quaternion direction, Material material)
    {
        Quaternion randomizedDirection = VectorMath.RandomizeQuarternion(direction, spreadType.Accuracy);
        float randomizedVelocity = NumberMath.PickRandomInRangeNoSeed(spreadType.MinVelocity, spreadType.MaxVelocity);
        float randomizedLifeTime = NumberMath.PickRandomInRangeNoSeed(spreadType.MinAvgLifeTime, spreadType.MaxAvgLifeTime) * (spreadType.MaxVelocity / randomizedVelocity);
        SpawnSingleFluidParticle(position, zLayer, randomizedDirection, randomizedVelocity, randomizedLifeTime, spreadType.MaxParticleSize, material);
    }

    private void SpawnSingleFluidParticle(Vector2 postion, ZIndexLayer zLayer, Quaternion direction, float velocity, float lifeTime, int maxSize, Material material)
    {
        FluidParticle newParticle;
        Vector3 spawnPosition = VectorMath.Vec2ToVec3(postion, zLayer.transform.position.z);
        Quaternion spawnRotation = direction;

        if (velocity > SPEED_TO_USE_DRIP_FLUID)
        {
            newParticle = Instantiate(NumberMath.PickRandomItemNoSeed(DripParticles, maxSize), spawnPosition, spawnRotation, zLayer.FluidParticlesContainer);
        }
        else
        {
            newParticle = Instantiate(NumberMath.PickRandomItemNoSeed(BlobParticles, maxSize), spawnPosition, spawnRotation, zLayer.FluidParticlesContainer);
        }
        newParticle.SetProperties(VectorMath.Quartenion2DToVec2(direction) * velocity, lifeTime, material);
        LayerManager.Instance.ChangeZIndexForGameObject(zLayer, newParticle.gameObject);
        OnAddFluidParticle(newParticle);
    }

    private void SpawnSingleHugeBlobFluidParticle(Vector2 postion, ZIndexLayer zLayer, Quaternion direction, Material material)
    {
        FluidParticle newParticle = Instantiate(
            NumberMath.PickRandomItemNoSeed(HugeBlobParticles), 
            VectorMath.Vec2ToVec3(VectorMath.RandomizeVec2(postion, HUGE_BLOB_PARTICLES_POSITION_SPREAD), zLayer.transform.position.z),
            VectorMath.RandomizeQuarternion(direction, HUGE_BLOB_PARTICLES_ROTATION_SPREAD),
            zLayer.FluidParticlesContainer
            );
        newParticle.SetProperties(Vector2.zero, 0f, material);
        LayerManager.Instance.ChangeZIndexForGameObject(zLayer, newParticle.gameObject);
        OnAddFluidParticle(newParticle);
    }
    private void SpawnSingleHugeDripFluidParticle(Vector2 postion, ZIndexLayer zLayer, Quaternion direction, Material material)
    {
        FluidParticle newParticle = Instantiate(
            NumberMath.PickRandomItemNoSeed(HugeDripParticles),
            VectorMath.Vec2ToVec3(VectorMath.RandomizeVec2(postion, HUGE_DRIP_PARTICLES_POSITION_SPREAD), zLayer.transform.position.z),
            VectorMath.RandomizeQuarternion(direction, HUGE_DRIP_PARTICLES_ROTATION_SPREAD),
            zLayer.FluidParticlesContainer
            );
        newParticle.SetProperties(Vector2.zero, 0f, material);
        LayerManager.Instance.ChangeZIndexForGameObject(zLayer, newParticle.gameObject);
        OnAddFluidParticle(newParticle);
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}
