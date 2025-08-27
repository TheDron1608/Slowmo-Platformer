using System.Collections.Generic;
using UnityEngine;

public class BuildingInfo : MonoBehaviour
{
    public List<ChunkInfo> Chunks = new();
    public DoorGenerationPosition Enter;
    public DoorGenerationPosition Exit;

    public void AddChunk(ChunkInfo chunk)
    {
        Chunks.Add(chunk);
        chunk.Building = this;
    }
}
