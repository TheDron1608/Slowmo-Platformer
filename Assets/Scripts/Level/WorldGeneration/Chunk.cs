using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Chunk : MonoBehaviour
{
    public ChunkConnection[] GetConnections()
    {
        return transform.GetComponentsInChildren<ChunkConnection>();
    }

    public DoorGenerationPosition[] GetDoorGenerationPositions()
    {
        return transform.GetComponentsInChildren<DoorGenerationPosition>();
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

    public bool TryGenerateChunk(ZIndexLayer generateWhere, Vector3Int position, BuildingInfo building, out ChunkInfo chunkInfo)
    {
        Tilemap chunkMask = GetChunkMask();

        for (int x = chunkMask.cellBounds.min.x; x < chunkMask.cellBounds.max.x; x++)
        {
            for (int y = chunkMask.cellBounds.min.y; y < chunkMask.cellBounds.max.y; y++)
            {
                if (generateWhere.MultiTileMapsContainer.GetHasAnyTileAt(new Vector3Int(x, y) + position))
                {
                    chunkInfo = default;
                    return false;
                }
            }
        }

        ForceGenerateChunk(generateWhere, position, building, out chunkInfo);
        return true;
    }

    public void ForceGenerateChunk(ZIndexLayer generateWhere, Vector3Int position, BuildingInfo building, out ChunkInfo chunkInfo)
    {
        GameObject chunkInfoGO = new GameObject("ChunkInfo");
        chunkInfoGO.transform.parent = generateWhere.WorldGenerationDataObjectsContainer.transform;
        chunkInfo = chunkInfoGO.AddComponent<ChunkInfo>();

        chunkInfo.Building = building;
        building.Chunks.Add(chunkInfo);

        foreach (Transform child in transform)
        {
            generateWhere.TrySpawnObject(child.gameObject, position, building, chunkInfo);
        }

        
        chunkInfo.DoorGenPositions = new List<DoorGenerationPosition>();
        foreach (DoorGenerationPosition door in GetDoorGenerationPositions())
        {
            Vector3 spawnPosition = new Vector3(
                door.transform.position.x + position.x,
                door.transform.position.y + position.y,
                generateWhere.transform.position.z
                );
            chunkInfo.DoorGenPositions.Add(Instantiate(door, spawnPosition, transform.rotation, generateWhere.WorldGenerationDataObjectsContainer));
        }
        
    }

    public bool TryAddChunk(ZIndexLayer addWhere, ChunkConnection sourceChunkConnection, BuildingInfo building, out ChunkInfo newChunkInfo, out ChunkConnection connectedChunkConntection)
    {
        newChunkInfo = default;
        connectedChunkConntection = default;
        if (!GetAnyConnectionIsValid(sourceChunkConnection, out ChunkConnection newChunkConnection))
        {
            return false;
        }

        if (!TryGenerateChunk(addWhere, sourceChunkConnection.GetTilePosition() - newChunkConnection.GetRelativeTilePosition(), building, out newChunkInfo))
        {
            return false;
        }

        sourceChunkConnection.State = ChunkConnection.ChunkConnectionState.OPENED;

        foreach (ChunkConnection newConnection in newChunkInfo.Connections)
        {
            if (newConnection.GetTilePosition() == sourceChunkConnection.GetTilePosition())
            {
                newConnection.DestroyConnection();
                connectedChunkConntection = newConnection;
                break;
            }
        }

        return true;
    }

    public bool TryGenerateChunkWithDoor(ZIndexLayer generateWhere, Vector3Int position, BuildingInfo building, out ChunkInfo newChunk, out DoorGenerationPosition door)
    {
        int randomDoorArrayKey = (int)(UnityEngine.Random.value * GetDoorGenerationPositions().Length);

        if (TryGenerateChunk(
            generateWhere, 
            position - VectorMath.Vec2IntToVec3Int(TileManager.PositionToTilePosition(GetDoorGenerationPositions()[randomDoorArrayKey].transform.position)), 
            building,
            out newChunk)
            )
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
