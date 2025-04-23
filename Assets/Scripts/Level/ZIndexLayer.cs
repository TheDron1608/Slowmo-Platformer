using UnityEngine;
using UnityEngine.Tilemaps;

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

    private float _alpha = 1f;

    public float Alpha
    {
        get => _alpha;
        set
        {
            if (_alpha == value) return;

            _alpha = value;
            SetAlphaForAllChildren(_alpha, transform);
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
    }

    private void SetAlphaForAllChildren(float alpha, Transform t)
    {
        for (int i = 0; i < t.childCount; i++)
        {
            SetAlphaForAllChildren(alpha, t.GetChild(i));

            if (t.GetChild(i).TryGetComponent(out SpriteRenderer spriteRenderer))
            {
                spriteRenderer.color = new Color(
                    spriteRenderer.color.r,
                    spriteRenderer.color.g,
                    spriteRenderer.color.b,
                    alpha
                    );
            }
            else if (t.GetChild(i).TryGetComponent(out Tilemap tilemap))
            {
                tilemap.color = new Color(
                    tilemap.color.r,
                    tilemap.color.g,
                    tilemap.color.b,
                    alpha
                    );
            }
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
            spriteRenderer.sortingOrder = spriteRenderer.sortingOrder % 100 + ZIndex * 100;
        }
        else if (gameObject.TryGetComponent(out TilemapRenderer tileMapRenderer))
        {
            tileMapRenderer.sortingOrder = tileMapRenderer.sortingOrder % 100 + ZIndex * 100;
        }

        switch (gameObject.tag)
        {
            case LayerManager.ZLAYER_TAG_NAME:
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

            case LayerManager.PROJECTILE_TAG_NAME:
                gameObject.layer = ProjectilesLayer;
                break;

            default:
                gameObject.layer = gameObject.transform.parent.gameObject.layer;
                break;
        }
    }
}
