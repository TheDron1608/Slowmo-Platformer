using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Chunk : MonoBehaviour
{
    public ChunkConnectionPosition[] GetConnections()
    {
        return transform.GetComponentsInChildren<ChunkConnectionPosition>();
    }

    public DoorGenerationPosition[] GetDoorGenerationPositions()
    {
        return transform.GetComponentsInChildren<DoorGenerationPosition>();
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

    public bool TryGenerateChunk(MultiTileMapsContainer generateWhere, Vector3Int position, out ChunkInfo chunkInfo)
    {
        Tilemap chunkMask = GetChunkMask();

        for (int x = chunkMask.cellBounds.min.x; x < chunkMask.cellBounds.max.x; x++)
        {
            for (int y = chunkMask.cellBounds.min.y; y < chunkMask.cellBounds.max.y; y++)
            {
                if (generateWhere.GetHasAnyTileAt(new Vector3Int(x, y) + position))
                {
                    chunkInfo = default;
                    return false;
                }
            }
        }

        ForceGenerateChunk(generateWhere, position, out chunkInfo);
        return true;
    }

    public void ForceGenerateChunk(MultiTileMapsContainer generateWhere, Vector3Int position, out ChunkInfo chunkInfo)
    {
        GameObject chunkInfoGO = new GameObject("ChunkInfo");
        chunkInfoGO.transform.parent = LayerManager.Instance.GetZLayerOfGameObject(generateWhere.gameObject).WorldGenerationDataObjectsContainer.transform;
        chunkInfo = chunkInfoGO.AddComponent<ChunkInfo>();

        foreach (Transform child in transform)
        {
            generateWhere.TrySpawnObject(child.gameObject, position);
        }

        ChunkConnectionPosition[] connections = GetConnections();
        chunkInfo.Connections = new ChunkConnectionPosition[connections.Length];
        for (int i = 0; i < connections.Length; i++)
        {
            Vector3 spawnPosition = new Vector3(
                connections[i].transform.position.x + position.x,
                connections[i].transform.position.y + position.y,
                LayerManager.Instance.GetZLayerOfGameObject(generateWhere.gameObject).transform.position.z
                );
            chunkInfo.Connections[i] = Instantiate(connections[i], spawnPosition, transform.rotation, LayerManager.Instance.GetZLayerOfGameObject(generateWhere.gameObject).WorldGenerationDataObjectsContainer);
            chunkInfo.Connections[i].InitPrefabProps(connections[i].transform.parent.GetComponent<ChunkConnection>());
        }
        DoorGenerationPosition[] doors = GetDoorGenerationPositions();
        chunkInfo.DoorGenPositions = new DoorGenerationPosition[doors.Length];
        for (int i = 0; i < doors.Length; i++)
        {
            Vector3 spawnPosition = new Vector3(
                doors[i].transform.position.x + position.x,
                doors[i].transform.position.y + position.y,
                LayerManager.Instance.GetZLayerOfGameObject(generateWhere.gameObject).transform.position.z
                );
            chunkInfo.DoorGenPositions[i] = Instantiate(doors[i], spawnPosition, transform.rotation, LayerManager.Instance.GetZLayerOfGameObject(generateWhere.gameObject).WorldGenerationDataObjectsContainer);
        }
    }

    public bool TryAddChunk(MultiTileMapsContainer container, ChunkConnectionPosition sourceChunkConnection, out ChunkInfo newChunkInfo, out ChunkConnectionPosition connectedChunkConntection)
    {
        newChunkInfo = default;
        connectedChunkConntection = default;
        if (!GetAnyConnectionIsValid(sourceChunkConnection, out ChunkConnectionPosition newChunkConnection))
        {
            return false;
        }

        if (!TryGenerateChunk(container, sourceChunkConnection.GetTilePosition() - newChunkConnection.GetTileRelativePosition(), out newChunkInfo))
        {
            return false;
        }

        sourceChunkConnection.OnOpenedChunkConnection();

        foreach (ChunkConnectionPosition newConnection in newChunkInfo.Connections)
        {
            if (newConnection.GetTilePosition() == sourceChunkConnection.GetTilePosition())
            {
                connectedChunkConntection = newConnection;
                break;
            }
        }

        return true;
    }

    public bool TryGenerateChunkWithDoor(MultiTileMapsContainer generateWhere, Vector3Int position, out ChunkInfo newChunk, out DoorGenerationPosition door)
    {
        int randomDoorArrayKey = (int)(UnityEngine.Random.value * GetDoorGenerationPositions().Length);

        if (TryGenerateChunk(generateWhere, position - VectorMath.Vec2IntToVec3Int(TileManager.PositionToTilePosition(GetDoorGenerationPositions()[randomDoorArrayKey].transform.position)), out newChunk))
        {
            door = newChunk.DoorGenPositions[randomDoorArrayKey];
            return true;
        }
        else
        {
            door = default;
            return false;
        }
    }
}
