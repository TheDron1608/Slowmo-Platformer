using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[DefaultExecutionOrder(-1)]
public class ZIndexLayer : MonoBehaviour
{
    private const string ENVIROMENT_LAYER_NAME = "Enviroment";
    private const string CHARACTERS_LAYER_NAME = "Characters";
    private const string HOLDABLES_LAYER_NAME = "Holdables";
    private const string FURNITURE_LAYER_NAME = "Furniture";
    private const string PROJECTILES_LAYER_NAME = "Projectiles";

    private const string FLUID_PARTICLES_CONTAINER_NAME = "FluidParticles";
    private const string CHARACTERS_CONTAINER_NAME = "Characters";
    private const string FURNITURE_CONTAINER_NAME = "Furniture";
    private const string HOLDABLES_CONTAINER_NAME = "Holdables";
    private const string PHYSICS_PARTICLES_CONTAINER_NAME = "PhysicsParticles";
    private const string PROJECTILES_CONTAINER_NAME = "Projectiles";
    private const string INTERACTABLE_ENVIROMENT_CONTAINER_NAME = "InteractableEnviroment";

    public struct LayerAlphaMode
    {
        public float Alpha;
        public float OvergoundAlpha;

        public LayerAlphaMode(float alpha, float overgoundAlpha)
        {
            Alpha = alpha;
            OvergoundAlpha = overgoundAlpha;
        }
    }

    public int ZIndex = 1;
    public TileManager TileManager;

    public int EnviromentLayer { get; private set; }
    public int CharactersLayer { get; private set; }
    public int HoldablesLayer { get; private set; }
    public int FurnituresLayer { get; private set; }
    public int ProjectilesLayer { get; private set; }
    public int EntireLayerMask { get; private set; }

    public Transform FluidParticlesContainer { get; private set; }
    public Transform CharactersContainer { get; private set; }
    public Transform FurnitureContainer { get; private set; }
    public Transform HoldablesContainer { get; private set; }
    public Transform PhysicsParticlesContainer { get; private set; }
    public Transform ProjectilesContainer { get; private set; }
    public Transform InteractableEnviromentContainer {  get; private set; }
    public MultiTileMapsContainer MultiTileMapsContainer { get; private set; }

    private LayerAlphaMode _alphaMode;
    private List<BuildingInfo> _buildingsInfo = new();
    private List<ComplexGenerateionEnviroment.PreGeneratedEnviromentTempInfo> _generationTempInfo = new();

    public LayerAlphaMode LayerAlpha
    {
        get => _alphaMode;
        set
        {
            if (_alphaMode.Alpha == value.Alpha && _alphaMode.OvergoundAlpha == value.OvergoundAlpha) return;
            _alphaMode = value;
            SetAlphaForAllChildren(LayerAlpha, transform);
        }
    }
    public List<BuildingInfo> BuildinsInfo
    {
        get => _buildingsInfo;
        set => _buildingsInfo = value;
    }
    public List<ComplexGenerateionEnviroment.PreGeneratedEnviromentTempInfo> GenerationTempInfo
    {
        get => _generationTempInfo;
        set => _generationTempInfo = value;
    }
    public List<ComplexGenerateionEnviroment.PreGeneratedEnviromentTempInfo> GetGenerationTempInfoByType<T>(bool includeGenerated) where T : ComplexGenerateionEnviroment
    {
        List<ComplexGenerateionEnviroment.PreGeneratedEnviromentTempInfo> result = new();
        for (int i = 0; i < _generationTempInfo.Count; i++)
        {
            if (_generationTempInfo[i].TargetGeneration.GetComponent<T>() != null && (!_generationTempInfo[i].Generated || includeGenerated))
            {
                result.Add(_generationTempInfo[i]);
            }
        }
        return result;
    }

    private void Awake()
    {
        if (ZIndex < 1 || ZIndex > 5) throw new UnityException("ZIndexLayer ZIndex max value is 5 and min value is 1");

        InitializeEnviromoentLayers();
        UpdateLayerForAllChildren();
    }

    private void InitializeEnviromoentLayers()
    {
        EnviromentLayer = LayerMask.NameToLayer($"Z{ZIndex}{ENVIROMENT_LAYER_NAME}");
        CharactersLayer = LayerMask.NameToLayer($"Z{ZIndex}{CHARACTERS_LAYER_NAME}");
        HoldablesLayer = LayerMask.NameToLayer($"Z{ZIndex}{HOLDABLES_LAYER_NAME}");
        FurnituresLayer = LayerMask.NameToLayer($"Z{ZIndex}{FURNITURE_LAYER_NAME}");
        ProjectilesLayer = LayerMask.NameToLayer($"Z{ZIndex}{PROJECTILES_LAYER_NAME}");

        EntireLayerMask = (1 << EnviromentLayer) | (1 << CharactersLayer) | (1 << HoldablesLayer) | (1 << FurnituresLayer) | (1 << ProjectilesLayer);

        FluidParticlesContainer = transform.Find(FLUID_PARTICLES_CONTAINER_NAME);
        PhysicsParticlesContainer = transform.Find(PHYSICS_PARTICLES_CONTAINER_NAME);
        CharactersContainer = transform.Find(CHARACTERS_CONTAINER_NAME);
        FurnitureContainer = transform.Find(FURNITURE_CONTAINER_NAME);
        InteractableEnviromentContainer = transform.Find(INTERACTABLE_ENVIROMENT_CONTAINER_NAME);
        HoldablesContainer = transform.Find(HOLDABLES_CONTAINER_NAME);
        ProjectilesContainer = transform.Find(PROJECTILES_CONTAINER_NAME);

        MultiTileMapsContainer = transform.GetComponentInChildren<MultiTileMapsContainer>();
    }

    private void SetAlphaForAllChildren(LayerAlphaMode layerAlpha, Transform t, bool foundOvergound = false)
    {
        foundOvergound |= t.GetComponent<OvergoundSprite>() != null;

        if (t.TryGetComponent(out SpriteRenderer spriteRenderer))
        {
            spriteRenderer.color = new Color(
                spriteRenderer.color.r,
                spriteRenderer.color.g,
                spriteRenderer.color.b,
                foundOvergound ? layerAlpha.OvergoundAlpha : layerAlpha.Alpha
                );
        }
        else if (t.TryGetComponent(out Tilemap tilemap))
        {
            tilemap.color = new Color(
                tilemap.color.r,
                tilemap.color.g,
                tilemap.color.b,
                foundOvergound ? layerAlpha.OvergoundAlpha : layerAlpha.Alpha
                );
        }

        for (int i = 0; i < t.childCount; i++)
        {
            SetAlphaForAllChildren(layerAlpha, t.GetChild(i), foundOvergound);
        }
    }

    public void UpdateLayerForAllChildren()
    {
        UpdateLayerForAllChildren(transform);
    }
    public void UpdateLayerForAllChildren(Transform t)
    {
        UpdateLayerForGameObject(t.gameObject);

        for (int i = 0; i < t.childCount; i++)
        {
            UpdateLayerForGameObject(t.GetChild(i).gameObject);

            UpdateLayerForAllChildren(t.GetChild(i));
        }
    }

    public void UpdateLayerForGameObject(GameObject gameObject)
    {
        if (gameObject.TryGetComponent(out SpriteRenderer spriteRenderer))
        {
            spriteRenderer.sortingOrder = spriteRenderer.sortingOrder % 1000 + ZIndex * 1000;
        }
        else if (gameObject.TryGetComponent(out TilemapRenderer tileMapRenderer))
        {
            tileMapRenderer.sortingOrder = tileMapRenderer.sortingOrder % 1000 + ZIndex * 1000;
        }
        else if (gameObject.TryGetComponent(out ParticleSystemRenderer particleSystem))
        {
            particleSystem.sortingOrder = particleSystem.sortingOrder % 1000 + ZIndex * 1000;
        }

        switch (gameObject.tag)
        {
            case LayerManager.ZLAYER_TAG_NAME:
                break;

            case LayerManager.PROJECTILE_TAG_NAME:
                gameObject.layer = ProjectilesLayer; 
                break;

            case LayerManager.ENVIROMENT_TAG_NAME:
                gameObject.layer = EnviromentLayer;
                break;

            case LayerManager.CHARACTER_TAG_NAME:
                gameObject.layer = CharactersLayer;
                break;

            case LayerManager.FURNITURE_TAG_NAME:
                gameObject.layer = FurnituresLayer;
                break;

            case LayerManager.HOLDABLE_TAG_NAME:
            case LayerManager.PHYSICS_PARTICLE_TAG_NAME:
            case LayerManager.FLUID_PARTICLE_TAG_NAME:
                gameObject.layer = HoldablesLayer;
                break;

            default:
                gameObject.layer = gameObject.transform.parent.gameObject.layer;
                break;
        }

        SetAlphaForAllChildren(LayerAlpha, gameObject.transform);
    }

    public ZIndexLayer PickLayerAbove()
    {
        return LayerManager.Instance.ZLayers.ElementAtOrDefault(LayerManager.Instance.ZLayers.IndexOf(this) + 1);
    }

    public ZIndexLayer PickLayerUnder()
    {
        return LayerManager.Instance.ZLayers.ElementAtOrDefault(LayerManager.Instance.ZLayers.IndexOf(this) - 1);
    }

    /// <summary>
    /// generates object on relative position, 
    /// can spawn random and multiple objects, like:
    /// * simple GameObject (if has not NonGeneratableObject component)
    /// * ComplexGenerateionEnviroments' objects
    /// also draws tilemap over existing tilemap
    /// </summary>
    /// <param name="spawnObject">spawned object</param>
    /// <param name="position">relative offset of generated object</param>
    /// <param name="building">appends new object to building's info, also can be used in generation process</param>
    /// <param name="chunk">appends new object to chunks's info, also can be used in generation process</param>
    /// <returns>
    /// list of:
    /// * spawned objects
    /// * tilmaps wich were drawn over
    /// * null if spawned only complex generation object or failed generating
    /// </returns>
    public List<GameObject> TrySpawnObject(GameObject spawnObject, Vector3Int position, BuildingInfo building, ChunkInfo chunk)
    {
        if (spawnObject == null) return null;

        if (spawnObject.TryGetComponent(out RandomSpawn randomSpawn))
        {
            return TrySpawnObject(randomSpawn.PickRandomSpawnObject(), position, building, chunk);
        }
        else if (spawnObject.GetComponent<RandomSpawnMultiItem>() != null)
        {
            List<GameObject> result = new();
            foreach (Transform spawnObjectChild in spawnObject.transform)
            {
                result.AddRange(TrySpawnObject(spawnObjectChild.gameObject, position, building, chunk) ?? new List<GameObject>(0));
            }
            return result;
        }
        else if (spawnObject.TryGetComponent(out Tilemap tilemap))
        {
            return new List<GameObject> { MultiTileMapsContainer.GenerateTilemap(tilemap, position + NumberMath.Vec3ToVec3Int(tilemap.transform.position)) };
        }
        else if (spawnObject.TryGetComponent(out ComplexGenerateionEnviroment complexGeneratable))
        {
            complexGeneratable.PreGenerate(this, position, building, chunk);
            return null;
        }
        else if (spawnObject.GetComponent<NonGeneratableObject>() == null)
        {
            GameObject newObject = Instantiate(spawnObject, position + spawnObject.transform.position, spawnObject.transform.rotation, transform);
            LayerManager.Instance.ChangeZIndexForGameObject(this, newObject);
            UpdateLayerForGameObject(newObject);

            chunk?.AddObjectInside(newObject);

            return new List<GameObject> { newObject };
        }
        else
        {
            return null; 
        }
    }
}
