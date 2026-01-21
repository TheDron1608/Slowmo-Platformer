using System;
using System.Collections.Generic;
using UnityEngine;

public class ParticlesManager : MonoBehaviour
{
    public float PhysicsParticlesGlobalSpawnAmountMultiplier = 1f;
    public float FluidParticlesGlobalSpawnAmountMultiplier = 1f;
    public float CloudParticlesGlobalSpawnAmountMultiplier = 1f;
    [Header("Limits")]
    public int PhysicsParticlesMaxAmount = 256;
    public int FluidParticlesMaxAmount = 512;
    public int CloudParticlesMaxAmount = 64;
    public int LightParticlesMaxAmount = 32;
    [Header("UnusedInstancesContainers")]
    public Transform UnusedPhysicsParticleContainer;
    public Transform UnusedFluidParticleContainer;
    public Transform UnusedCloudParticleContainer;
    public Transform UnusedLightParticleContainer;
    [Header("SpawnInstances")]
    [SerializeField] private PhysicsParticle _emptyPhysicsParticleInstance;
    [SerializeField] private FluidParticle _emptyFluidParticleInstance;
    [SerializeField] private CloudParticle _emptyCloudParticleInstance;
    [SerializeField] private LightParticle _emptyLightParticleInstance;

    private List<PhysicsParticle> _physicsParticles;
    private List<FluidParticle> _fluidParticles;
    private List<CloudParticle> _cloudParticles;
    private List<LightParticle> _lightParticles;

    public static ParticlesManager Instance;

    private void Awake()
    {
        if (Instance != null) throw new Exception("limit of 1 ParticleManager per scene");
        Instance = this;

        InitParticles();
    }

    private void InitParticles()
    {
        _physicsParticles = new List<PhysicsParticle>(PhysicsParticlesMaxAmount);
        for (int i = 0; i < PhysicsParticlesMaxAmount; i++)
        {
            _physicsParticles.Insert(i, Instantiate(_emptyPhysicsParticleInstance, UnusedPhysicsParticleContainer));
            _physicsParticles[i].gameObject.SetActive(false);
        }
        _fluidParticles = new List<FluidParticle>(FluidParticlesMaxAmount);
        for (int i = 0; i < FluidParticlesMaxAmount; i++)
        {
            _fluidParticles.Insert(i, Instantiate(_emptyFluidParticleInstance, UnusedFluidParticleContainer));
            _fluidParticles[i].gameObject.SetActive(false);
        }
        _cloudParticles = new List<CloudParticle>(CloudParticlesMaxAmount);
        for (int i = 0; i < CloudParticlesMaxAmount; i++)
        {
            _cloudParticles.Insert(i, Instantiate(_emptyCloudParticleInstance, UnusedCloudParticleContainer));
            _cloudParticles[i].gameObject.SetActive(false);
        }
        _lightParticles = new List<LightParticle>(LightParticlesMaxAmount);
        for (int i = 0; i < LightParticlesMaxAmount; i++)
        {
            _lightParticles.Insert(i, Instantiate(_emptyLightParticleInstance, UnusedLightParticleContainer));
            _lightParticles[i].gameObject.SetActive(false);
        }
    }

    public AbstractParticle GetUnusedParticle(AbstractParticle prefab)
    {
        Transform unusedParticlesContainer = GetUnusedParticlesContainerByType(prefab);
        if (unusedParticlesContainer.childCount > 0)
        {
            return unusedParticlesContainer.GetChild(0).GetComponent<AbstractParticle>();
        }
        else
        {
            AbstractParticle farestParticle = null;
            float currentDistance = -1f;
            foreach (ZIndexLayer layer in LayerManager.Instance.ZLayers)
            {
                Transform usedParticlesContainer = layer.GetParticlesContainerByType(prefab);
                if (
                    usedParticlesContainer.childCount > 0
                    )
                {
                    float distance = Vector2.Distance(usedParticlesContainer.GetChild(0).transform.position, Camera.main.transform.position);
                    if (distance > currentDistance)
                    {
                        currentDistance = distance;
                        farestParticle = usedParticlesContainer.GetChild(0).GetComponent<AbstractParticle>();
                    }
                }
            }
            return farestParticle ?? throw new UnityException("not found any " + prefab.name + " particle");
        }
    }

    private Transform GetUnusedParticlesContainerByType(AbstractParticle prefab)
    {
        if (prefab is PhysicsParticle)
        {
            return UnusedPhysicsParticleContainer;
        }
        else if (prefab is FluidParticle)
        {
            return UnusedFluidParticleContainer;
        }
        else if (prefab is CloudParticle)
        {
            return UnusedCloudParticleContainer;
        }
        else if (prefab is LightParticle)
        {
            return UnusedLightParticleContainer;
        }
        else
        {
            throw new UnityException("could not find container for type " + (prefab?.name ?? "null"));
        }
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}
