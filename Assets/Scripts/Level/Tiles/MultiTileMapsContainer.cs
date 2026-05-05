using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

[DefaultExecutionOrder(-2)]
public class MultiTileMapsContainer : MonoBehaviour
{
    [SerializeField] private Tilemap _foreground;
    [SerializeField] private Tilemap _background;
    [SerializeField] private Tilemap _backgroundDecorations;
    [SerializeField] private Tilemap _hallucinationTilemap;
    [SerializeField] private Tilemap _overground;
    [SerializeField] private Tilemap _overgroundDecorations;

    private bool _requestUpdateNavigationAtEndOfFrame = true;
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
        if (_requestUpdateNavigationAtEndOfFrame)
        {
            _layer.TileManager.UpdateEntireTileAINavigationInfo();
            //_layer.TileManager.Debug_DrawAINavigationPaths(Color.red, 999f, 3, 4);
            _requestUpdateNavigationAtEndOfFrame = false;
            Tilemap.tilemapTileChanged += Tilemap_tilemapTileChanged;
        }
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
            if (tilemap.GetComponent<TileBehaviour>()?.BehaviourType == behaviour && tilemap.HasTile(tilePosition))
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
        if (gameObject.IsDestroyed()) return;

        _foreground.GetComponent<OverrideRendererEnabled>().OverrideValue = value ? false : null;
        _background.GetComponent<OverrideRendererEnabled>().OverrideValue = value ? false : null;
        _backgroundDecorations.GetComponent<OverrideRendererEnabled>().OverrideValue = value ? false : null;
        _overground.GetComponent<OverrideRendererEnabled>().OverrideValue = value ? false : null;
        _overgroundDecorations.GetComponent<OverrideRendererEnabled>().OverrideValue = value ? false : null;

        _hallucinationTilemap.GetComponent<OverrideRendererEnabled>().OverrideValue = value;
    }

    public void SetTilemapsMaterialDependOnDifficulty(DifficultyManager.DifficultyStage difficulty)
    {
        foreach (Tilemap tilemap in _tilemaps)
        {
            tilemap.GetComponent<TileBehaviour>()?
                .SetMaterialDependOnDifficulty(difficulty);
        }
    }

    private void OnDestroy()
    {
        Tilemap.tilemapTileChanged -= Tilemap_tilemapTileChanged;
    }
}
