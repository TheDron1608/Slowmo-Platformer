using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Chunk : MonoBehaviour
{
    public ChunkConnection[] GetConnections()
    {
        return transform.GetComponentsInChildren<ChunkConnection>();
    }

    public bool GetAnyConnectionIsValid(ChunkConnection targetConnection, out ChunkConnection validConnection)
    {
        foreach (var connection in GetConnections())
        {
            if (connection.GetConnectionIsValid(targetConnection))
            {
                validConnection = connection;
                return true;
            }
        }
        validConnection = default;
        return false;
    }

    public bool TryGenerateChunk(MultiTileMapsContainer generateWhere, Vector3Int position, out ChunkConnection[] generatedConnections)
    {
        foreach (Transform child in transform)
        {
            if (child.TryGetComponent(out TileBehaviour tileBehaviour) && tileBehaviour.BehaviourType == TileBehaviour.TileBehaviourType.CHUNK_MASK && child.TryGetComponent(out Tilemap sourceTileMap))
            {
                for (int x = sourceTileMap.cellBounds.min.x; x < sourceTileMap.cellBounds.max.x; x++)
                {
                    for (int y = sourceTileMap.cellBounds.min.y; y < sourceTileMap.cellBounds.max.y; y++)
                    {
                        foreach (Tilemap targetTileMap in generateWhere.GetTileMaps())
                        {
                            if (targetTileMap.GetComponent<TileBehaviour>()?.ValidAsPlatform ?? false)
                            {
                                if (sourceTileMap.HasTile(new Vector3Int(x, y)) && targetTileMap.HasTile(new Vector3Int(x, y) + position))
                                {
                                    /*generateWhere.GetComponent<TileManager>().Debug_MarkArea(
                                        new Vector2(sourceTileMap.cellBounds.min.x + position.x, sourceTileMap.cellBounds.min.y + position.y), 
                                        new Vector2(sourceTileMap.cellBounds.max.x + position.x, sourceTileMap.cellBounds.max.y + position.y), 
                                        Color.red, 
                                        999f
                                        );*/
                                    generatedConnections = default;
                                    return false;
                                }
                            }
                        }
                    }
                }

                /*generateWhere.GetComponent<TileManager>().Debug_MarkArea(
                    new Vector2(sourceTileMap.cellBounds.min.x + position.x, sourceTileMap.cellBounds.min.y + position.y),
                    new Vector2(sourceTileMap.cellBounds.max.x + position.x, sourceTileMap.cellBounds.max.y + position.y),
                    Color.green,
                    999f
                    );*/
            }
        }

        ForceGenerateChunk(generateWhere, position, out generatedConnections);
        return true;
    }

    public void ForceGenerateChunk(MultiTileMapsContainer generateWhere, Vector3Int position, out ChunkConnection[] generatedConnections)
    {
        foreach (Transform child in transform)
        {
            //skip generating if random tilemap generation chance is failed
            if (child.TryGetComponent(out RandomTilemapGenerateChance tilemapChance) && tilemapChance.GenerateChance < Random.value) continue;

            //generating alternative tilemaps instead of main if chance is successed
            Tilemap sourceTileMap;
            if (!child.TryGetComponent(out sourceTileMap)) continue;
            float randomAlternativeChunkSeed = Random.value;
            float currentRandomAlternativeChunkSeed = 0f;
            foreach (RandomTilemapGenerateChance altTileMap in child.GetComponentsInChildren<RandomTilemapGenerateChance>())
            {
                currentRandomAlternativeChunkSeed += altTileMap.GenerateChance;
                if (currentRandomAlternativeChunkSeed > randomAlternativeChunkSeed)
                {
                    sourceTileMap = altTileMap.GetComponent<Tilemap>();
                    break;
                }
            }

            //redrawing each tile from prefab's tilemap to scene's tilemap
            Tilemap targetTileMap = generateWhere.GetTileMapByBehaviourType(sourceTileMap.GetComponent<TileBehaviour>().BehaviourType);
            if (targetTileMap == null) continue;

            for (int x = sourceTileMap.cellBounds.min.x; x < sourceTileMap.cellBounds.max.x; x++)
            {
                for (int y = sourceTileMap.cellBounds.min.y; y < sourceTileMap.cellBounds.max.y; y++)
                {
                    TileBase tile = sourceTileMap.GetTile(new Vector3Int(x, y));
                    if (tile != null)
                    {
                        targetTileMap.SetTile(new Vector3Int(x, y) + position, tile);

                        if (sourceTileMap.GetComponent<TileBehaviour>().ValidAsPlatform)
                        {
                            foreach (Tilemap subTargetTileMap in generateWhere.GetTileMaps())
                            {
                                if (!subTargetTileMap.GetComponent<TileBehaviour>().ValidAsPlatform || subTargetTileMap == targetTileMap)
                                {
                                    continue;
                                }
                                else if (subTargetTileMap.GetComponent<TileBehaviour>().OverrideOrder <= sourceTileMap.GetComponent<TileBehaviour>().OverrideOrder)
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

        ChunkConnection[] connections = GetConnections();
        ChunkConnection[] result = new ChunkConnection[connections.Length];
        for (int i = 0; i < connections.Length; i++)
        {
            result[i] = Instantiate(connections[i], connections[i].transform.position + position, transform.rotation, generateWhere.transform);
        }

        generatedConnections = result;
    }

    public bool TryAddChunk(MultiTileMapsContainer container, ChunkConnection sourceChunkConnection, out ChunkConnection connectedChunkConntection)
    {
        connectedChunkConntection = default;
        if (!GetAnyConnectionIsValid(sourceChunkConnection, out ChunkConnection newChunkConnection))
        {
            return false;
        }

        if (!TryGenerateChunk(container, sourceChunkConnection.GetTilePosition() - newChunkConnection.GetTileRelativePosition(), out ChunkConnection[] newConnections))
        {
            return false;
        }

        foreach (ChunkConnection newConnection in newConnections)
        {
            if (newConnection.GetTilePosition() == sourceChunkConnection.GetTilePosition())
            {
                connectedChunkConntection = newConnection;
                break;
            }
        }

        return true;
    }
}
