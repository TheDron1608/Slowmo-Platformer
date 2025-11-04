using UnityEngine;
using UnityEngine.Tilemaps;

[DefaultExecutionOrder(-2)]
public class MultiTileMapsContainer : MonoBehaviour
{
    private bool _requestUpdateNavigationAtEndOfFrame = true;
    private ZIndexLayer _layer;
    private Tilemap[] _tilemaps;

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

    public GameObject GenerateTilemap(Tilemap tilemap, Vector3Int position)
    {
        if (tilemap == null) return null;
        Tilemap targetTileMap = GetTileMapByBehaviourType(tilemap.GetComponent<TileBehaviour>().BehaviourType);
        if (targetTileMap == null) return null;

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
                        ForegroundRuleTile oldForegroundTile = GetTileMapByBehaviourType(TileBehaviour.TileBehaviourType.FOREBGROUND).GetTile<ForegroundRuleTile>(tilePos);
                        if (oldForegroundTile == null || foregroundTile.OverrideOrder >= oldForegroundTile.OverrideOrder)
                        {
                            GetTileMapByBehaviourType(TileBehaviour.TileBehaviourType.BACKGROUND).SetTile(tilePos, null);
                            GetTileMapByBehaviourType(TileBehaviour.TileBehaviourType.FOREBGROUND).SetTile(tilePos, foregroundTile);
                        }
                    }
                    else
                    {
                        GetTileMapByBehaviourType(tilemap.GetComponent<TileBehaviour>().BehaviourType).SetTile(tilePos, tile);
                    }
                }
            }
        }

        return targetTileMap.gameObject;
    }
}
