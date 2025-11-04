using UnityEngine;

public class BuildingEnterChunk : Chunk
{
    public DoorGenerationPosition Enter;

    public bool TryGenerateChunkWithEnterAt(
    ZIndexLayer generateWhere,
    Vector3Int position,
    BuildingInfo building,
    out ChunkInfo newChunk
    )
    {
        return TryGenerateChunk(
            generateWhere,
            position - VectorMath.Vec3ToVec3Int(Enter.transform.position),
            building,
            out newChunk
            );
    }
}
