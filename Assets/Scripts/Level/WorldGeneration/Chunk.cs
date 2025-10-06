using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Chunk : MonoBehaviour
{
    public int BaseEnemiesAmount = 1;

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
                    //generateWhere.TileManager.Debug_MarkArea(new Vector3(chunkMask.cellBounds.min.x, chunkMask.cellBounds.min.y), new Vector3(chunkMask.cellBounds.max.x, chunkMask.cellBounds.max.y), Color.red, 999f);
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
        chunkInfo = new();
        building.Chunks.Add(chunkInfo);
        chunkInfo.Building = building;

        foreach (Transform child in transform)
        {
            generateWhere.TrySpawnObject(child.gameObject, position, building, chunkInfo);
        }
    }

    public bool TryAddChunk(
        ZIndexLayer addWhere, 
        ChunkConnection.PreGeneratedChunkConnectionTempInfo sourceChunkConnection, 
        BuildingInfo building, 
        out ChunkInfo newChunkInfo, 
        out ChunkConnection.PreGeneratedChunkConnectionTempInfo connectedChunkConntection)
    {
        newChunkInfo = default;
        connectedChunkConntection = default;
        if (!GetAnyConnectionIsValid(sourceChunkConnection.TargetGeneration.GetComponent<ChunkConnection>(), out ChunkConnection newChunkConnection))
        {
            return false;
        }

        if (!TryGenerateChunk(addWhere, NumberMath.Vec3ToVec3Int(sourceChunkConnection.GetSpawnPosition()) - newChunkConnection.GetTilePosition(), building, out newChunkInfo))
        {
            //addWhere.TileManager.Debug_MarkTile(sourceChunkConnection.Offset - newChunkConnection.GetTilePosition(), Color.blue, 999f);
            return false;
        }


        foreach (ChunkInfo chunk in building.Chunks)
        {
            if (newChunkInfo == chunk) continue;
            foreach (ChunkConnection.PreGeneratedChunkConnectionTempInfo connection in chunk.Connections)
            {
                for (int i = 0; i < newChunkInfo.Connections.Count; i++)
                {
                    if (TileManager.PositionToTilePosition(connection.GetSpawnPosition()) == TileManager.PositionToTilePosition(newChunkInfo.Connections[i].GetSpawnPosition()))
                    {
                        connection.State = ChunkConnection.PreGeneratedChunkConnectionTempInfo.ChunkConnectionState.OPENED;
                        newChunkInfo.Connections[i].Remove();
                        i--;
                    }
                }
            }
        }

        return true;
    }

    public bool TryGenerateChunkWithDoor(
        ZIndexLayer generateWhere, 
        Vector3Int position, 
        BuildingInfo building, 
        out ChunkInfo newChunk, 
        out DoorGenerationPosition.PreGeneratedDoorTempInfo door
        )
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
