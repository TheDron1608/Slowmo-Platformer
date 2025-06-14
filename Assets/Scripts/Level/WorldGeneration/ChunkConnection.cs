using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class ChunkConnection : MonoBehaviour
{
    public enum ChunkConnectionDirection
    {
        TOP,
        BOTTOM,
        LEFT,
        RIGHT
    }

    public ChunkConnectionDirection Direction;

    public Chunk GetChunk()
    {
        return transform.parent.GetComponent<Chunk>();
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
        return new Vector3Int((int)math.floor(transform.position.x), (int)math.floor(transform.position.y));
    }

    public Vector3Int GetTileRelativePosition()
    {
        return new Vector3Int((int)math.floor(transform.position.x - transform.parent.position.x), (int)math.floor(transform.position.y - transform.parent.position.y));
    }
}
