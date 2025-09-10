using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ChunkInfo
{
    public List<ChunkConnection.PreGeneratedChunkConnectionTempInfo> Connections = new();
    public List<ComplexGenerateionEnviroment.PreGeneratedEnviromentTempInfo> DoorGenPositions = new();
    public BuildingInfo Building;
}
