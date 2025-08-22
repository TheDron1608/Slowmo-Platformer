using UnityEngine;
using UnityEngine.Tilemaps;

public class Chunk : MonoBehaviour
{
    public ChunkConnectionPosition[] GetConnections()
    {
        return transform.GetComponentsInChildren<ChunkConnectionPosition>();
    }

    public bool GetAnyConnectionIsValid(ChunkConnectionPosition targetConnection, out ChunkConnectionPosition validConnection)
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

    public Tilemap GetChunkMask()
    {
        foreach (Transform child in transform)
        {
            if (child.TryGetComponent(out TileBehaviour tileBehaviour) && tileBehaviour.BehaviourType == TileBehaviour.TileBehaviourType.CHUNK_MASK && child.TryGetComponent(out Tilemap result))
            {
                return result;
            }
        }
        throw new UnityException("Chunk mask not found");
    }

    public bool TryGenerateChunk(MultiTileMapsContainer generateWhere, Vector3Int position, out ChunkConnectionPosition[] generatedConnections)
    {
        Tilemap chunkMask = GetChunkMask();

        for (int x = chunkMask.cellBounds.min.x; x < chunkMask.cellBounds.max.x; x++)
        {
            for (int y = chunkMask.cellBounds.min.y; y < chunkMask.cellBounds.max.y; y++)
            {
                if (generateWhere.GetHasAnyTileAt(new Vector3Int(x, y) + position))
                {
                    generatedConnections = default;
                    return false;
                }
            }
        }

        ForceGenerateChunk(generateWhere, position, out generatedConnections);
        return true;
    }

    public void ForceGenerateChunk(MultiTileMapsContainer generateWhere, Vector3Int position, out ChunkConnectionPosition[] generatedConnections)
    {
        foreach (Transform child in transform)
        {
            generateWhere.TrySpawnObject(child.gameObject, position);
        }

        ChunkConnectionPosition[] connections = GetConnections();
        ChunkConnectionPosition[] result = new ChunkConnectionPosition[connections.Length];
        for (int i = 0; i < connections.Length; i++)
        {
            result[i] = Instantiate(connections[i], connections[i].transform.position + position, transform.rotation, generateWhere.transform);
            result[i].InitPrefabProps(connections[i].transform.parent.GetComponent<ChunkConnection>());
        }

        generatedConnections = result;
    }

    public bool TryAddChunk(MultiTileMapsContainer container, ChunkConnectionPosition sourceChunkConnection, out ChunkConnectionPosition connectedChunkConntection)
    {
        connectedChunkConntection = default;
        if (!GetAnyConnectionIsValid(sourceChunkConnection, out ChunkConnectionPosition newChunkConnection))
        {
            return false;
        }

        if (!TryGenerateChunk(container, sourceChunkConnection.GetTilePosition() - newChunkConnection.GetTileRelativePosition(), out ChunkConnectionPosition[] newConnections))
        {
            return false;
        }

        sourceChunkConnection.OnOpenedChunkConnection();

        foreach (ChunkConnectionPosition newConnection in newConnections)
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
