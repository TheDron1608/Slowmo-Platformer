using UnityEngine;
using UnityEngine.Tilemaps;

public class MultiTileMapsContainer : MonoBehaviour
{
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

    public void GenerateTilemap(Tilemap tilemap, Vector3Int position)
    {
        if (tilemap == null) return;
        Tilemap targetTileMap = GetTileMapByBehaviourType(tilemap.GetComponent<TileBehaviour>().BehaviourType);
        if (targetTileMap == null) return;

        for (int x = tilemap.cellBounds.min.x; x < tilemap.cellBounds.max.x; x++)
        {
            for (int y = tilemap.cellBounds.min.y; y < tilemap.cellBounds.max.y; y++)
            {
                TileBase tile = tilemap.GetTile(new Vector3Int(x, y));
                if (tile != null)
                {
                    targetTileMap.SetTile(new Vector3Int(x, y) + position, tile);

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
    }
}
