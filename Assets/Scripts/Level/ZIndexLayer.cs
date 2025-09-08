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
    private const string WORLD_GENERATION_DATA_OBJECTS_CONTAINER_NAME = "WorldGenerationDataObjects";

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
    public Transform WorldGenerationDataObjectsContainer { get; private set; }
    public MultiTileMapsContainer MultiTileMapsContainer { get; private set; }

    private LayerAlphaMode _alphaMode;

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
        HoldablesContainer = transform.Find(HOLDABLES_CONTAINER_NAME);
        ProjectilesContainer = transform.Find(PROJECTILES_CONTAINER_NAME);
        WorldGenerationDataObjectsContainer = transform.Find(WORLD_GENERATION_DATA_OBJECTS_CONTAINER_NAME);

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
    }

    public ZIndexLayer PickLayerAbove()
    {
        return LayerManager.Instance.ZLayers.ElementAtOrDefault(LayerManager.Instance.ZLayers.IndexOf(this) + 1);
    }

    public ZIndexLayer PickLayerUnder()
    {
        return LayerManager.Instance.ZLayers.ElementAtOrDefault(LayerManager.Instance.ZLayers.IndexOf(this) - 1);
    }

    public void TrySpawnObject(GameObject spawnObject, Vector3Int position, BuildingInfo building, ChunkInfo chunk)
    {
        if (spawnObject == null) return;

        if (spawnObject.TryGetComponent(out RandomSpawn randomSpawn))
        {
            TrySpawnObject(randomSpawn.PickRandomSpawnObject(), position, building, chunk);
        }
        else if (spawnObject.GetComponent<RandomSpawnMultiItem>() != null)
        {
            foreach (Transform spawnObjectChild in spawnObject.transform)
            {
                TrySpawnObject(spawnObjectChild.gameObject, position, building, chunk);
            }
        }
        else if (spawnObject.TryGetComponent(out Tilemap tilemap))
        {
            MultiTileMapsContainer.GenerateTilemap(tilemap, position);
        }
        else if (spawnObject.TryGetComponent(out ComplexGenerateionEnviroment complexGeneratable))
        {
            complexGeneratable.PreGenerate(this, position, building, chunk);
        }
        else if (spawnObject.GetComponent<NonGeneratableObject>() == null)
        {
            GameObject newObject = Instantiate(spawnObject, position + spawnObject.transform.position, spawnObject.transform.rotation, transform);
            LayerManager.Instance.ChangeZIndexForGameObject(this, newObject);
            UpdateLayerForGameObject(newObject);
        }
    }
}
