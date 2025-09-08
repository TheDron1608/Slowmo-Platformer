using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ChunkInfo : MonoBehaviour
{
    public List<ChunkConnection> Connections = new();
    public List<DoorGenerationPosition> DoorGenPositions = new();
    public BuildingInfo Building;
}
