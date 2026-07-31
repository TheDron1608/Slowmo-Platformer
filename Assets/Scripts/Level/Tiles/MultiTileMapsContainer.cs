using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

[DefaultExecutionOrder(-2)]
public class MultiTileMapsContainer : MonoBehaviour
{
    const float SPAWN_VELOCITY = 2;
    const float SPAWN_ANGULAR_VELOCITY = 720f;
    const float PARTICLE_MAX_DISTANCE_FROM_TILE_TO_UPDATE = 3f;

    public struct TileChangeData
    {
        public Vector3Int Position;
        public bool IsAdded;
    }

    public List<AbstractParticle> ParticlesOnBreakTile = new();
    public int ParticlesAmountOnBreakTile;
    public BackgroundRuleTile TileOnBreakTile;

    [SerializeField] private Tilemap _foreground;
    [SerializeField] private Tilemap _background;
    [SerializeField] private Tilemap _backgroundDecorations;
    [SerializeField] private Tilemap _hallucinationTilemap;
    [SerializeField] private Tilemap _overground;
    [SerializeField] private Tilemap _overgroundDecorations;

    [SerializeField] private TilemapRenderer _foregroundRenderer;
    [SerializeField] private TilemapRenderer _backgroundRenderer;
    [SerializeField] private TilemapRenderer _backgroundDecorationsRenderer;
    [SerializeField] private TilemapRenderer _hallucinationTilemapRenderer;
    [SerializeField] private TilemapRenderer _overgroundRenderer;
    [SerializeField] private TilemapRenderer _overgroundDecorationsRenderer;

    private bool _requestUpdateNavigationAtEndOfFrame = true;
    private List<TileChangeData> _requestUpdateTiles = new();
    private ZIndexLayer _layer;
    private Tilemap[] _tilemaps;

    public Tilemap[] GetTileMaps()
    {
        return _tilemaps;
    }
    public Tilemap GetForeground()
    {
        return _foreground;
    }
    public Tilemap GetBackground()
    {
        return _background;
    }
    public Tilemap GetBackgroundDecorations()
    {
        return _backgroundDecorations;
    }
    public Tilemap GetHallucinationTilemap()
    {
        return _hallucinationTilemap;
    }
    public Tilemap GetOverground()
    {
        return _overground;
    }
    public Tilemap GetOvergroundDecorations()
    {
        return _overgroundDecorations;
    }

    private void Awake()
    {
        _layer = LayerManager.Instance.GetZLayerOfGameObject(gameObject);
        _tilemaps = transform.GetComponentsInChildren<Tilemap>();
        Tilemap.tilemapTileChanged += Tilemap_tilemapTileChanged;
    }

    private void Tilemap_tilemapTileChanged(Tilemap arg1, Tilemap.SyncTile[] arg2)
    {
        if (LayerManager.Instance.GetZLayerOfGameObject(arg1.gameObject) != _layer) return;

        foreach (Tilemap.SyncTile tile in arg2)
        {
            if (tile.tile is ForegroundRuleTile foregroundTile && foregroundTile.ValidAsPlatform)
            {
                _requestUpdateNavigationAtEndOfFrame = true;
                Tilemap.tilemapTileChanged -= Tilemap_tilemapTileChanged;
                return;
            }
        }
    }

    private void LateUpdate()
    {
        //_layer.TileManager.Debug_DrawAINavigationPaths(Color.red, new Color(0, 1, 0, 0.5f), Time.deltaTime);

        if (_requestUpdateNavigationAtEndOfFrame)
        {
            _layer.TileManager.UpdateEntireTileAINavigationInfo();
            _requestUpdateNavigationAtEndOfFrame = false;
            Tilemap.tilemapTileChanged += Tilemap_tilemapTileChanged;
        }

        for (int i = 0; i < _requestUpdateTiles.Count; i++)
        {
            if (_requestUpdateTiles[i].IsAdded) continue;

            Vector3 tilePos = new Vector3(
                _requestUpdateTiles[i].Position.x + 0.5f,
                _requestUpdateTiles[i].Position.y + 0.5f,
                transform.position.z
                );

            if (!_foreground.HasTile(_requestUpdateTiles[i].Position))
            {
                foreach (Transform physicsParticleTransform in _layer.PhysicsParticlesContainer)
                {
                    if (
                        Vector2.Distance(physicsParticleTransform.position, tilePos) < PARTICLE_MAX_DISTANCE_FROM_TILE_TO_UPDATE &&
                        physicsParticleTransform.TryGetComponent(out PhysicsParticle physicsParticle)
                        )
                    {
                        physicsParticle.EnabledPhysics = true;
                    }
                }
            }
            if (!_background.HasTile(_requestUpdateTiles[i].Position))
            {
                foreach (Transform fluidParticleTransform in _layer.FluidParticlesContainer)
                {
                    if (
                        Vector2.Distance(fluidParticleTransform.position, tilePos) < PARTICLE_MAX_DISTANCE_FROM_TILE_TO_UPDATE &&
                        fluidParticleTransform.TryGetComponent(out FluidParticle fluidParticle)
                        )
                    {
                        fluidParticle.IsFlying = true;
                    }
                }
            }
        }

        _layer.TileManager.UpdateTileNavigation(_requestUpdateTiles);

        _requestUpdateTiles.Clear();
    }

    public Tilemap GetTileMapByBehaviourType(TileBehaviour.TileBehaviourType behaviourType)
    {
        foreach (TileBehaviour tileBehaviour in transform.GetComponentsInChildren<TileBehaviour>())
        {
            if (tileBehaviour.BehaviourType == behaviourType) return tileBehaviour.GetComponent<Tilemap>();
        }
        return null;
    }

    public bool GetHasAnyTileAt(Vector3Int position)
    {
        foreach (Tilemap tilemap in _tilemaps)
        {
            if (tilemap.HasTile(position)) return true;
        }
        return false;
    }

    public T GetTileAt<T>(Vector3Int position) where T : TileBase
    {
        foreach (Tilemap tilemap in _tilemaps)
        {
            T tile = tilemap.GetTile<T>(position);
            if (tile != null) return tile;
        }
        return null;
    }

    public List<TileBehaviour.TileBehaviourType> GetTileBehavioursAt(Vector2 position)
    {
        List<TileBehaviour.TileBehaviourType> result = new();
        Vector3Int tilePosition =
            new Vector3Int(
            (int)math.floor(position.x),
            (int)math.floor(position.y)
            );

        foreach (Tilemap tilemap in _tilemaps)
        {
            if (tilemap.HasTile(tilePosition))
            {
                result.Add(tilemap.GetComponent<TileBehaviour>().BehaviourType);
            }
        }
        return result;
    }

    public bool GetHasTileBehaviourAt(Vector2 position, TileBehaviour.TileBehaviourType behaviour)
    {
        Vector3Int tilePosition =
        new Vector3Int(
            (int)math.floor(position.x),
            (int)math.floor(position.y)
            );

        foreach (Tilemap tilemap in _tilemaps)
        {
            if (
                tilemap.TryGetComponent(out TileBehaviour tileBeh) &&
                tileBeh.BehaviourType == behaviour && 
                tilemap.HasTile(tilePosition))
            {
                return true;
            }
        }
        return false;
    }

    public bool GetHasValidAsPlatformAt(Vector3Int position)
    {
        return GetComponent<MultiTileMapsContainer>().GetForeground().GetTile<ForegroundRuleTile>(position)?.ValidAsPlatform ?? false;
    }

    public bool GetHasValidAsPlatformAt(Vector2 position)
    {
        Vector3Int tilePosition =
            new Vector3Int(
            (int)math.floor(position.x),
            (int)math.floor(position.y)
            );

        return GetHasValidAsPlatformAt(tilePosition);
    }

    public GameObject GenerateTilemap(Tilemap tilemap, Vector3Int position)
    {
        if (tilemap == null) return null;
        Tilemap targetTileMap = GetTileMapByBehaviourType(tilemap.GetComponent<TileBehaviour>().BehaviourType);
        if (targetTileMap == null) return null;
        LayerManager.Instance.TrySetLevelBottom(position.y - tilemap.cellBounds.yMin);

        for (int x = tilemap.cellBounds.min.x; x < tilemap.cellBounds.max.x; x++)
        {
            for (int y = tilemap.cellBounds.min.y; y < tilemap.cellBounds.max.y; y++)
            {
                TileBase tile = tilemap.GetTile(new Vector3Int(x, y));
                if (tile != null)
                {
                    Vector3Int tilePos = new Vector3Int(x, y) + position;
                    if (tile is ForegroundRuleTile foregroundTile)
                    {
                        ForegroundRuleTile oldForegroundTile = GetForeground().GetTile<ForegroundRuleTile>(tilePos);
                        if (oldForegroundTile == null || foregroundTile.OverrideOrder >= oldForegroundTile.OverrideOrder)
                        {
                            GetBackground().SetTile(tilePos, null);
                            GetForeground().SetTile(tilePos, foregroundTile);
                            if (!GetOverground().HasTile(tilePos)) GetOverground().SetTile(tilePos, foregroundTile);
                        }
                    }
                    else if (tile is RestrictInteriourWalls)
                    {
                        if (!targetTileMap.HasTile(tilePos))
                        {
                            targetTileMap.SetTile(tilePos, tile);
                        }
                    }
                    else
                    {
                        targetTileMap.SetTile(tilePos, tile);
                    }
                }
            }
        }

        return targetTileMap.gameObject;
    }

    public BoundsInt GetTotalTilemapCellBounds()
    {
        BoundsInt result = new(GetTileMaps()[0].cellBounds.position, GetTileMaps()[0].cellBounds.size);
        foreach (Tilemap tilemap in GetTileMaps())
        {
            if (tilemap.cellBounds.xMin < result.xMin) result.xMin = tilemap.cellBounds.xMin;
            if (tilemap.cellBounds.xMax > result.xMax) result.xMax = tilemap.cellBounds.xMax;
            if (tilemap.cellBounds.yMin < result.yMin) result.yMin = tilemap.cellBounds.yMin;
            if (tilemap.cellBounds.yMax > result.yMax) result.yMax = tilemap.cellBounds.yMax;
        }
        return result;
    }

    public void ToggleHallucinationTilemapVisibility(bool value)
    {
        if (gameObject?.IsDestroyed() ?? true) return;

        _foreground.GetComponent<OverrideRendererEnabled>().OverrideValue = value ? false : null;
        _background.GetComponent<OverrideRendererEnabled>().OverrideValue = value ? false : null;
        _backgroundDecorations.GetComponent<OverrideRendererEnabled>().OverrideValue = value ? false : null;
        _overground.GetComponent<OverrideRendererEnabled>().OverrideValue = value ? false : null;
        _overgroundDecorations.GetComponent<OverrideRendererEnabled>().OverrideValue = value ? false : null;

        _hallucinationTilemap.GetComponent<OverrideRendererEnabled>().OverrideValue = value;
    }

    public bool DestroyTileAt(Vector3Int position, bool includeBackground, bool includeOverground)
    {
        if (includeBackground)
        {
            _background.SetTile(position, null);
            _backgroundDecorations.SetTile(position, null);
        }
        else
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    Vector3Int currentPosition = position + new Vector3Int(x, y, 0);
                    if (
                        !_background.HasTile(currentPosition) && 
                        (_foreground.GetTile<ForegroundRuleTile>(currentPosition)?.ValidAsPlatform ?? false)
                        )
                    {
                        _background.SetTile(currentPosition, TileOnBreakTile);
                    }
                }
            }
        }

        if (includeOverground)
        {
            _overground.SetTile(position, null);
            _overgroundDecorations.SetTile(position, null);
        }

        if (_foreground.HasTile(position))
        {
            _foreground.SetTile(position, null);
            ParticleSpawner.SpawnInstantlyMultipleParticles(
                ParticlesOnBreakTile,
                Vector2.one * UnityEngine.Random.value + new Vector2(position.x, position.y),
                Vector2.one,
                0f,
                -SPAWN_VELOCITY,
                SPAWN_VELOCITY,
                -SPAWN_ANGULAR_VELOCITY,
                SPAWN_ANGULAR_VELOCITY,
                _foregroundRenderer.sharedMaterial,
                _layer,
                ParticlesAmountOnBreakTile,
                0f
                );

            TileChangeData newChangeData = new();
            newChangeData.Position = position;
            newChangeData.IsAdded = false;

            _requestUpdateTiles.Add(newChangeData);

            return true;
        }
        else
        {
            return false;
        }
    }

    public void SetTilemapsMaterialDependOnDifficulty(DifficultyManager.DifficultyStage difficulty)
    {
        foreach (Tilemap tilemap in _tilemaps)
        {
            tilemap.GetComponent<TileBehaviour>()?
                .SetMaterialDependOnDifficulty(difficulty);
        }
    }

    public void UpdateForegroundTilesOverBackground(BoundsInt updateArea)
    {
        for (int x = updateArea.xMin; x < updateArea.xMax; x++)
        {
            for (int y = updateArea.yMin; y < updateArea.yMax; y++)
            {
                Vector3Int currentTilePos = new Vector3Int(x, y);
                TileBase foregroundTile = _foreground.GetTile(currentTilePos);
                if (foregroundTile != null && !_background.HasTile(currentTilePos))
                {
                    _background.SetTile(currentTilePos, foregroundTile);
                }
            }
        }
    }

    private void OnDestroy()
    {
        Tilemap.tilemapTileChanged -= Tilemap_tilemapTileChanged;
    }
}
