using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;


[DefaultExecutionOrder(-1)]
public class ZIndexLayer : MonoBehaviour
{
    const int MAX_Z_LAYERS = 3;

    const string ENVIROMENT_LAYER_NAME = "Enviroment";
    const string CHARACTERS_LAYER_NAME = "Characters";
    const string HOLDABLES_LAYER_NAME = "Holdables";
    const string HITABLE_HOLDABLES_LAYER_NAME = "HitableHoldables";
    const string FURNITURE_LAYER_NAME = "Furniture";
    const string PROJECTILES_LAYER_NAME = "Projectiles";
    const string PARTICLES_LAYER_NAME = "Particles";

    const string BACKGROUND_SORTING_LAYER_NAME = "Background";
    const string OBJECTS_SORTING_LAYER_NAME = "Objects";
    const string ENVIROMENT_SORTING_LAYER_NAME = "Enviroment";
    const string OVERGROUND_SORTING_LAYER_NAME = "Overground";
    const string OTHER_SORTING_LAYER_NAME = "Other";
    const string FOG_SORTING_LAYER_NAME = "Fog";

    public struct LayerAlphaMode
    {
        public float Alpha;
        public float OvergoundAlpha;
        public float HideOnOvergrounded;

        public LayerAlphaMode(float alpha, float overgoundAlpha, float hideOnOvergrounded)
        {
            Alpha = alpha;
            OvergoundAlpha = overgoundAlpha;
            HideOnOvergrounded = hideOnOvergrounded;
        }
    }

    public int ZIndex = 1;
    public TileManager TileManager;

    public int EnviromentLayer { get; private set; }
    public int CharactersLayer { get; private set; }
    public int HoldablesLayer { get; private set; }
    public int HitableHoldablesLayer { get; private set; }
    public int FurnituresLayer { get; private set; }
    public int ProjectilesLayer { get; private set; }
    public int ParticlesLayer { get; private set; }
    public int EntireLayerMask { get; private set; }

    public int BackgroundSortingLayer { get; private set; }
    public int ObjectsSortingLayer { get; private set; }
    public int EnviromentSortingLayer { get; private set; }
    public int OvergroundSortingLayer { get; private set; }
    public int OtherSortingLayer { get; private set; }
    public int FogSortingLayer { get; private set; }

    public Transform CharactersContainer { get => _charactersContainer; }
    public Transform FurnitureContainer { get => _furnitureContainer; }
    public Transform HoldablesContainer { get => _holdablesContainer; }
    public Transform PhysicsParticlesContainer { get => _physicsParticlesContainer; }
    public Transform FluidParticlesContainer { get => _fluidParticlesContainer; }
    public Transform CloudParticlesContainer { get => _cloudParticlesContainer; }
    public Transform LightParticlesContainer { get => _lightParticlesContainer; }
    public Transform ProjectilesContainer { get => _projectilesContainer; }
    public Transform InteractableEnviromentContainer { get => _interactableEnviromentContainer; }
    public Transform OtherContainer { get => _otherContainer; }
    public MultiTileMapsContainer MultiTileMapsContainer { get => _multiTileMapsContainer; }

    [SerializeField] private Transform _charactersContainer;
    [SerializeField] private Transform _furnitureContainer;
    [SerializeField] private Transform _holdablesContainer;
    [SerializeField] private Transform _physicsParticlesContainer;
    [SerializeField] private Transform _fluidParticlesContainer;
    [SerializeField] private Transform _cloudParticlesContainer;
    [SerializeField] private Transform _lightParticlesContainer;
    [SerializeField] private Transform _projectilesContainer;
    [SerializeField] private Transform _interactableEnviromentContainer;
    [SerializeField] private Transform _otherContainer;
    [SerializeField] private MultiTileMapsContainer _multiTileMapsContainer;

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
            if (_generationTempInfo[i].TargetGeneration.TryGetComponent(out T t) && (!_generationTempInfo[i].Generated || includeGenerated))
            {
                result.Add(_generationTempInfo[i]);
            }
        }
        return result;
    }

    private void Awake()
    {
        if (ZIndex < 1 || ZIndex > MAX_Z_LAYERS) throw new UnityException("ZIndexLayer ZIndex max value is " + MAX_Z_LAYERS + " and min value is 1");

        InitializeEnviromoentLayers();
        UpdateLayerForAllChildren();

        LayerManager.Instance.TrySetLevelBottom(MultiTileMapsContainer.GetForeground().cellBounds.yMin);

        SetEnvromentMaterialDependOnDifficulty(DifficultyManager.Instance.CurrentDifficulty.Value);
    }

    private void InitializeEnviromoentLayers()
    {
        EnviromentLayer = LayerMask.NameToLayer($"Z{ZIndex}{ENVIROMENT_LAYER_NAME}");
        CharactersLayer = LayerMask.NameToLayer($"Z{ZIndex}{CHARACTERS_LAYER_NAME}");
        HoldablesLayer = LayerMask.NameToLayer($"Z{ZIndex}{HOLDABLES_LAYER_NAME}");
        HitableHoldablesLayer = LayerMask.NameToLayer($"Z{ZIndex}{HITABLE_HOLDABLES_LAYER_NAME}");
        FurnituresLayer = LayerMask.NameToLayer($"Z{ZIndex}{FURNITURE_LAYER_NAME}");
        ProjectilesLayer = LayerMask.NameToLayer($"Z{ZIndex}{PROJECTILES_LAYER_NAME}");
        ParticlesLayer = LayerMask.NameToLayer($"Z{ZIndex}{PARTICLES_LAYER_NAME}");

        BackgroundSortingLayer = SortingLayer.NameToID($"Z{ZIndex}{BACKGROUND_SORTING_LAYER_NAME}");
        ObjectsSortingLayer = SortingLayer.NameToID($"Z{ZIndex}{OBJECTS_SORTING_LAYER_NAME}");
        EnviromentSortingLayer = SortingLayer.NameToID($"Z{ZIndex}{ENVIROMENT_SORTING_LAYER_NAME}");
        OvergroundSortingLayer = SortingLayer.NameToID($"Z{ZIndex}{OVERGROUND_SORTING_LAYER_NAME}");
        OtherSortingLayer = SortingLayer.NameToID($"Z{ZIndex}{OTHER_SORTING_LAYER_NAME}");
        FogSortingLayer = SortingLayer.NameToID($"Z{ZIndex}{FOG_SORTING_LAYER_NAME}");

        EntireLayerMask =
            (1 << EnviromentLayer) | (1 << CharactersLayer) | (1 << HoldablesLayer) | (1 << HitableHoldablesLayer) | (1 << FurnituresLayer) | (1 << ProjectilesLayer);
    }

    private void SetAlphaForAllChildren(
        LayerAlphaMode layerAlpha,
        Transform t,
        LayerSpriteCustomVisibility.LayerSpriteCustomVisibilityTypes inheritType = LayerSpriteCustomVisibility.LayerSpriteCustomVisibilityTypes.DEFAULT
        )
    {
        if (t.TryGetComponent(out LayerSpriteCustomVisibility customVisibility))
        {
            inheritType = customVisibility.VisibilityType;
        }

        SetAlphaForGameObject(layerAlpha, t, inheritType);

        for (int i = 0; i < t.childCount; i++)
        {
            SetAlphaForAllChildren(layerAlpha, t.GetChild(i), inheritType);
        }
    }

    private void SetAlphaForGameObject(
        LayerAlphaMode layerAlpha,
        Transform t,
        LayerSpriteCustomVisibility.LayerSpriteCustomVisibilityTypes type = LayerSpriteCustomVisibility.LayerSpriteCustomVisibilityTypes.DEFAULT
        )
    {
        if (t.TryGetComponent(out Renderer renderer))
        {
            float targetAlpha;
            switch (type)
            {
                case LayerSpriteCustomVisibility.LayerSpriteCustomVisibilityTypes.DEFAULT:
                    targetAlpha = layerAlpha.Alpha;
                    break;
                case LayerSpriteCustomVisibility.LayerSpriteCustomVisibilityTypes.OVERGROUND:
                    targetAlpha = layerAlpha.OvergoundAlpha;
                    break;
                case LayerSpriteCustomVisibility.LayerSpriteCustomVisibilityTypes.HIDE_ON_OVERGROUNDED:
                    targetAlpha = layerAlpha.HideOnOvergrounded;
                    break;
                default:
                    throw new UnityException("not found pattern for visibility of type: " + type.ToString());
            }

            if (t.TryGetComponent(out OverrideRendererEnabled overrider))
            {
                overrider.TryUpdateEnabled(targetAlpha > 0.05f);
            }
            else if (renderer.enabled != targetAlpha > 0.05f)
            {
                renderer.enabled = targetAlpha > 0.05f;
            }

            if (t.TryGetComponent(out SpriteRenderer spriteRenderer))
            {
                spriteRenderer.color = new Color(
                    spriteRenderer.color.r,
                    spriteRenderer.color.g,
                    spriteRenderer.color.b,
                    targetAlpha
                    );
            }
            else if (t.TryGetComponent(out Tilemap tilemap))
            {
                tilemap.color = new Color(
                    tilemap.color.r,
                    tilemap.color.g,
                    tilemap.color.b,
                    targetAlpha
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
            UpdateLayerForAllChildren(t.GetChild(i));
        }
    }

    private void UpdateLayerForGameObject(GameObject gameObject)
    {
        if (gameObject.TryGetComponent(out Renderer renderer))
        {
            renderer.sortingOrder = renderer.sortingOrder % 1000 + ZIndex * 1000;

            if (gameObject.TryGetComponent(out DynamicMaterial dynamicMaterial))
            {
                if (gameObject.tag == LayerManager.FURNITURE_TAG_NAME)
                {
                    dynamicMaterial.DefaultMaterial = DifficultyManager.Instance.CurrentDifficulty.Value.SecondaryEnviromentMaterial;
                }
                else if (gameObject.tag == LayerManager.ENVIROMENT_TAG_NAME)
                {
                    dynamicMaterial.DefaultMaterial = DifficultyManager.Instance.CurrentDifficulty.Value.PrimaryEnviromentMaterial;
                }
            }
            else
            {
                if (gameObject.tag == LayerManager.FURNITURE_TAG_NAME)
                {
                    renderer.sharedMaterial = DifficultyManager.Instance.CurrentDifficulty.Value.SecondaryEnviromentMaterial;
                }
                else if (gameObject.tag == LayerManager.ENVIROMENT_TAG_NAME)
                {
                    renderer.sharedMaterial = DifficultyManager.Instance.CurrentDifficulty.Value.PrimaryEnviromentMaterial;
                }
            }
        }
        else if (gameObject.TryGetComponent(out Canvas canvas))
        {
            canvas.sortingOrder = canvas.sortingOrder % 1000 + ZIndex * 1000;
        }

        switch (gameObject.tag)
        {
            case LayerManager.PROJECTILE_TAG_NAME:
            case LayerManager.CHARACTER_TAG_NAME:
            case LayerManager.FURNITURE_TAG_NAME:
            case LayerManager.HOLDABLE_TAG_NAME:
            case LayerManager.PHYSICS_PARTICLE_TAG_NAME:
            case LayerManager.CLOUD_PARTICLE_TAG_NAME:
            case LayerManager.LIGHT_PARTICLE_TAG_NAME:
                SetLightRendererLayer(
                    gameObject,
                    ObjectsSortingLayer,
                    new int[] { BackgroundSortingLayer, ObjectsSortingLayer, FogSortingLayer },
                    new int[] { }
                );
                break;

            case LayerManager.FLUID_PARTICLE_TAG_NAME:
                int targetLayer = gameObject.TryGetComponent(out FluidParticle fp) ? fp.GetCurrentLayerSotringOrder(this) : BackgroundSortingLayer;
                SetLightRendererLayer(
                    gameObject,
                    targetLayer,
                    new int[] { targetLayer },
                    new int[] { }
                );
                break;

            case LayerManager.ENVIROMENT_TAG_NAME:
                gameObject.TryGetComponent(out TileBehaviour tileBeh);
                switch (tileBeh?.BehaviourType)
                {
                    case TileBehaviour.TileBehaviourType.BACKGROUND:
                    case TileBehaviour.TileBehaviourType.BACKGROUND_DECORATIONS:
                    case TileBehaviour.TileBehaviourType.HALLUCINATION_TILES:
                        SetLightRendererLayer(
                            gameObject,
                            BackgroundSortingLayer,
                            new int[] { BackgroundSortingLayer },
                            new int[] { }
                        );
                        break;
                    case TileBehaviour.TileBehaviourType.OVERGROUND:
                    case TileBehaviour.TileBehaviourType.OVERGROUND_DECORATIONS:
                    case TileBehaviour.TileBehaviourType.OVERGROUND_HALLUCINATION_TILES:
                        SetLightRendererLayer(
                            gameObject,
                            OvergroundSortingLayer,
                            new int[] { OvergroundSortingLayer },
                            new int[] { OvergroundSortingLayer }
                        );
                        break;
                    default:
                        SetLightRendererLayer(
                            gameObject,
                            EnviromentSortingLayer,
                            new int[] { EnviromentSortingLayer, BackgroundSortingLayer, ObjectsSortingLayer },
                            new int[] { BackgroundSortingLayer, ObjectsSortingLayer, FogSortingLayer }
                        );
                        break;
                }
                break;
            case LayerManager.OTHER_TAG_NAME:
                SetLightRendererLayer(
                    gameObject,
                    OtherSortingLayer,
                    new int[] { },
                    new int[] { }
                );
                break;
            case LayerManager.FOG_TAG_NAME:
                SetLightRendererLayer(
                    gameObject,
                    FogSortingLayer,
                    new int[] { },
                    new int[] { }
                );
                break;
        }

        switch (gameObject.tag)
        {
            case LayerManager.ZLAYER_TAG_NAME:
            case LayerManager.OTHER_TAG_NAME:
            case LayerManager.FOG_TAG_NAME:
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
                gameObject.layer = (gameObject.TryGetComponent(out Holdable h) ? h.GetIsHitableNow() : false) ? HitableHoldablesLayer : HoldablesLayer;
                break;

            case LayerManager.PHYSICS_PARTICLE_TAG_NAME:
            case LayerManager.FLUID_PARTICLE_TAG_NAME:
            case LayerManager.CLOUD_PARTICLE_TAG_NAME:
                gameObject.layer = ParticlesLayer;
                break;

            default:
                gameObject.layer = gameObject.transform.parent.gameObject.layer;
                break;
        }

        SetAlphaForGameObject(LayerAlpha, gameObject.transform);
    }

    private void SetLightRendererLayer(GameObject gameObject, int sortingLayerId, int[] lightTargetSortingLayers, int[] shadowTargetSortingLayers)
    {
        if (gameObject.TryGetComponent(out Renderer renderer))
        {
            renderer.sortingLayerID = sortingLayerId;
        }
        if (gameObject.TryGetComponent(out DynamicLightSortingLayer lightSortingLayer))
        {
            lightSortingLayer.SortingLayer = lightTargetSortingLayers;
        }
        if (gameObject.TryGetComponent(out DynamicShadowCasterSortingLayer shadowSortingLayer))
        {
            shadowSortingLayer.SortingLayer = shadowTargetSortingLayers;
        }
        if (gameObject.TryGetComponent(out Canvas canvas))
        {
            canvas.sortingLayerID = sortingLayerId;
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
    public List<GameObject> TrySpawnObject(GameObject spawnObject, Vector3 position, BuildingInfo building, ChunkInfo chunk)
    {
        if (spawnObject == null) return null;

        if (spawnObject.TryGetComponent(out RandomSpawn randomSpawn))
        {
            return TrySpawnObject(randomSpawn.PickRandomSpawnObject(), position, building, chunk);
        }
        else if (spawnObject.TryGetComponent(out RandomSpawnMultiItem rsmt))
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
            return new List<GameObject> { MultiTileMapsContainer.GenerateTilemap(tilemap, VectorMath.Vec3ToVec3Int(position) + NumberMath.Vec3ToVec3Int(tilemap.transform.position)) };
        }
        else if (spawnObject.TryGetComponent(out ComplexGenerateionEnviroment complexGeneratable))
        {
            complexGeneratable.PreGenerate(this, position, building, chunk);
            return null;
        }
        else if (!spawnObject.TryGetComponent(out NonGeneratableObject ngo))
        {
            GameObject newObject = Instantiate(spawnObject, position + spawnObject.transform.position, spawnObject.transform.rotation, transform);
            newObject.gameObject.SetActive(true);
            LayerManager.Instance.ChangeZIndexForGameObject(this, newObject);
            LayerManager.Instance.TrySetLevelBottom(newObject.transform.position.y);
            UpdateLayerForGameObject(newObject);

            chunk?.AddObjectInside(newObject);

            LayerManager.Instance.InvokeOnObjectSpawned(newObject);

            return new List<GameObject> { newObject };
        }
        else
        {
            return null;
        }
    }

    public Transform GetParticlesContainerByType(AbstractParticle prefab)
    {
        if (prefab is PhysicsParticle)
        {
            return PhysicsParticlesContainer;
        }
        else if (prefab is FluidParticle)
        {
            return FluidParticlesContainer;
        }
        else if (prefab is CloudParticle)
        {
            return CloudParticlesContainer;
        }
        else if (prefab is LightParticle)
        {
            return LightParticlesContainer;
        }
        else
        {
            throw new UnityException("could not find container for type " + prefab.name);
        }
    }

    public void Debug_ArtificalInvokeOnObjectSpawnedForAll()
    {
        foreach (Transform character in CharactersContainer)
        {
            LayerManager.Instance.InvokeOnObjectSpawned(character.gameObject);
        }
        foreach (Transform furnture in FurnitureContainer)
        {
            LayerManager.Instance.InvokeOnObjectSpawned(furnture.gameObject);
        }
        foreach (Transform interactableEnv in InteractableEnviromentContainer)
        {
            LayerManager.Instance.InvokeOnObjectSpawned(interactableEnv.gameObject);
        }
        foreach (Transform holdable in HoldablesContainer)
        {
            LayerManager.Instance.InvokeOnObjectSpawned(holdable.gameObject);
        }
    }

    public void SetEnvromentMaterialDependOnDifficulty(DifficultyManager.DifficultyStage difficulty)
    {
        foreach (Transform furniture in FurnitureContainer)
        {
            if (furniture.TryGetComponent(out DynamicMaterial furnitureDynamicMaterial))
            {
                furnitureDynamicMaterial.DefaultMaterial = difficulty.PrimaryEnviromentMaterial;
            }
            else if (furniture.TryGetComponent(out Renderer furnitureRenderer))
            {
                furnitureRenderer.sharedMaterial = difficulty.PrimaryEnviromentMaterial;
            }
        }

        foreach (Transform interactableEnviroment in InteractableEnviromentContainer)
        {
            if (interactableEnviroment.TryGetComponent(out DynamicMaterial interactableEnviromentDynamicMaterial))
            {
                interactableEnviromentDynamicMaterial.DefaultMaterial = difficulty.PrimaryEnviromentMaterial;
            }
            else if (interactableEnviroment.TryGetComponent(out Renderer interactableEnvRenderer))
            {
                interactableEnvRenderer.sharedMaterial = difficulty.PrimaryEnviromentMaterial;
            }
        }

        MultiTileMapsContainer.SetTilemapsMaterialDependOnDifficulty(difficulty);
    }
}