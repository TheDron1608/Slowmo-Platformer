using UnityEngine;
using UnityEngine.Tilemaps;

[DefaultExecutionOrder(-2)]
public class MultiTileMapsContainer : MonoBehaviour
{
    private bool _requestUpdateNavigationAtEndOfFrame = false;
    private ZIndexLayer _layer;

    private void Awake()
    {
        _layer = LayerManager.Instance.GetZLayerOfGameObject(gameObject);
        Tilemap.tilemapTileChanged += Tilemap_tilemapTileChanged;
    }

    private void Tilemap_tilemapTileChanged(Tilemap arg1, Tilemap.SyncTile[] arg2)
    {
        if (LayerManager.Instance.GetZLayerOfGameObject(arg1.gameObject) != _layer) return;

        if (arg1.GetComponent<TileBehaviour>()?.ValidAsPlatform ?? false)
        {
            _requestUpdateNavigationAtEndOfFrame = true;
            Tilemap.tilemapTileChanged -= Tilemap_tilemapTileChanged;
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

    public Tilemap[] GetTileMaps()
    {
        return transform.GetComponentsInChildren<Tilemap>();
    }

    public bool GetHasAnyTileAt(Vector3Int position)
    {
        foreach (Tilemap tilemap in GetTileMaps())
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
                    targetTileMap.SetTile(new Vector3Int(x, y) + position , tile);

                    if (tilemap.GetComponent<TileBehaviour>().Overridable)
                    {
                        foreach (Tilemap subTargetTileMap in GetTileMaps())
                        {
                            if (!subTargetTileMap.GetComponent<TileBehaviour>().Overridable || subTargetTileMap == targetTileMap)
                            {
                                continue;
                            }
                            else if (subTargetTileMap.GetComponent<TileBehaviour>().OverrideOrder <= tilemap.GetComponent<TileBehaviour>().OverrideOrder)
                            {
                                subTargetTileMap.SetTile(new Vector3Int(x, y) + position, null);
                            }
                            else if (subTargetTileMap.HasTile(new Vector3Int(x, y) + position))
                            {
                                targetTileMap.SetTile(new Vector3Int(x, y) + position, null);
                                break;
                            }
                        }
                    }
                }
            }
        }

        return targetTileMap.gameObject;
    }
}
