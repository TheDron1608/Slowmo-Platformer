using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChunkInfo
{
    public List<ChunkConnection.PreGeneratedChunkConnectionTempInfo> Connections = new();
    public List<DoorGenerationPosition.PreGeneratedDoorTempInfo> DoorGenPositions = new();
    public BuildingInfo Building;
    public List<GameObject> ObjectsInside = new();
    public Chunk OriginalChunk;
    public int DistanceFromMainGenerationBranch = 0;

    public ChunkInfo(Chunk originalChunk)
    {
        OriginalChunk = originalChunk;
    }

    public void AddObjectInside(GameObject obj)
    {
        ObjectsInside.Add(obj);

        if (obj.TryGetComponent(out BreakableObject breakableObj))
        {
            breakableObj.OnBroken += BreakableObj_OnBroken;
        }
    }

    public ChunkConnection.PreGeneratedChunkConnectionTempInfo PickRandomFilteredConnection(ChunkConnection.PreGeneratedChunkConnectionTempInfo.ChunkConnectionState filter)
    {
        return NumberMath.PickRandomItem(Connections.Where((connection) => connection.State == filter).ToArray());
    }

    public Vector3? PickDoorAvgPosition()
    {
        if (DoorGenPositions.Count == 0) return null;

        Vector3 result = Vector2.zero;
        foreach (var doorGen in DoorGenPositions)
        {
            result += doorGen.GetSpawnPosition();
        }
        return result / DoorGenPositions.Count;
    }

    public Vector3 PickConnectionsAvgPosition()
    {
        Vector3 result = Vector2.zero;
        foreach (var connection in Connections)
        {
            result += connection.GetSpawnPosition();
        }
        return result / Connections.Count;
    }

    private void BreakableObj_OnBroken(object sender, MonoBehaviour e)
    {
        ObjectsInside.Remove(((MonoBehaviour)sender).gameObject);
    }

    ~ChunkInfo()
    {
        foreach (GameObject objectInside in ObjectsInside)
        {
            if (objectInside?.TryGetComponent(out BreakableObject breakableObj) ?? false)
            {
                breakableObj.OnBroken -= BreakableObj_OnBroken;
            }
        }
    }
}
