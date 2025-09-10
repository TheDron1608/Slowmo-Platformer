using System.Collections.Generic;
using UnityEngine;

public class BuildingInfo
{
    public List<ChunkInfo> Chunks = new();
    public ComplexGenerateionEnviroment.PreGeneratedEnviromentTempInfo Enter;
    public ComplexGenerateionEnviroment.PreGeneratedEnviromentTempInfo Exit;
    public int LowerstCoorY = int.MaxValue;
    public int HighestCoorY = int.MinValue;
}
