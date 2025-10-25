using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ChunkConnection : GenerateOnFinishBuildingEnviroment
{
    public class PreGeneratedChunkConnectionTempInfo : PreGeneratedEnviromentTempInfo
    {
        public enum ChunkConnectionState
        {
            CLOSED,
            OPENED
        }

        public ChunkConnectionState State = ChunkConnectionState.CLOSED;

        public PreGeneratedChunkConnectionTempInfo(ZIndexLayer generateWhere, Vector3 offset, ComplexGenerateionEnviroment targetGeneration, BuildingInfo building, ChunkInfo chunk) : base(generateWhere, offset, targetGeneration, building, chunk)
        {
            chunk.Connections.Add(this);
        }

        public override void Remove()
        {
            base.Remove();
            Chunk.Connections.Remove(this);
        }
    }

    public enum ChunkConnectionDirection
    {
        TOP,
        BOTTOM,
        LEFT,
        RIGHT
    }


    public ChunkConnectionDirection Direction;
    public List<GameObject> ObjectsOnOpenedConnection;
    public List<GameObject> ObjectsOnClosedConnection;

    public override List<GameObject> Generate(PreGeneratedEnviromentTempInfo generationInfo)
    {
        base.Generate(generationInfo);

        ZIndexLayer layer = generationInfo.GenerateWhere;
        List<GameObject> targetGenerationObjs;
        if ((generationInfo as PreGeneratedChunkConnectionTempInfo).State == PreGeneratedChunkConnectionTempInfo.ChunkConnectionState.CLOSED)
        {
            targetGenerationObjs = ObjectsOnClosedConnection;
        }
        else if ((generationInfo as PreGeneratedChunkConnectionTempInfo).State == PreGeneratedChunkConnectionTempInfo.ChunkConnectionState.OPENED)
        {
            targetGenerationObjs = ObjectsOnOpenedConnection;
        }
        else targetGenerationObjs = new();

        List<GameObject> result = new();
        foreach (GameObject targetGemeratonObj in targetGenerationObjs)
        {
            result.AddRange(
                    layer.TrySpawnObject(
                    targetGemeratonObj, 
                    NumberMath.Vec3ToVec3Int(generationInfo.Offset), 
                    generationInfo.Building,
                    generationInfo.Chunk
                    ) ?? new List<GameObject>(0)
                );
        }

        return result;
    }

    public override PreGeneratedEnviromentTempInfo PreGenerate(ZIndexLayer preGenerateWhere, Vector3 position, BuildingInfo building, ChunkInfo chunk)
    {
        return new PreGeneratedChunkConnectionTempInfo(preGenerateWhere, position, this, building, chunk);
    }

    public bool GetConnectionIsValid(ChunkConnection targetConnection)
    {
        return
            (Direction == ChunkConnectionDirection.TOP && targetConnection.Direction == ChunkConnectionDirection.BOTTOM) ||
            (Direction == ChunkConnectionDirection.BOTTOM && targetConnection.Direction == ChunkConnectionDirection.TOP) ||
            (Direction == ChunkConnectionDirection.LEFT && targetConnection.Direction == ChunkConnectionDirection.RIGHT) ||
            (Direction == ChunkConnectionDirection.RIGHT && targetConnection.Direction == ChunkConnectionDirection.LEFT);
    }

    public Vector3Int GetTilePosition()
    {
        return new Vector3Int((int)math.round(transform.position.x), (int)math.round(transform.position.y));
    }

    public bool GetConnectionIsPreffered(Vector2 direction)
    {
        return
            (Direction == ChunkConnectionDirection.RIGHT && direction.x >= 0) || (Direction == ChunkConnectionDirection.LEFT && direction.x <= 0) ||
            (Direction == ChunkConnectionDirection.TOP && direction.y >= 0) || (Direction == ChunkConnectionDirection.BOTTOM && direction.y <= 0);
    }
}
