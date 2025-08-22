using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class ChunkConnectionPosition : MonoBehaviour
{
    public enum ChunkConnectionDirection
    {
        TOP,
        BOTTOM,
        LEFT,
        RIGHT
    }

    public ChunkConnectionDirection Direction;

    private ChunkConnection _originalPrefabConnection;
    private Vector3Int _tilemapOffset;

    public Chunk GetChunk()
    {
        return transform.parent.GetComponent<Chunk>();
    }

    public bool GetConnectionIsValid(ChunkConnectionPosition targetConnection)
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

    public void InitPrefabProps(ChunkConnection prefab)
    {
        _originalPrefabConnection = prefab;
        _tilemapOffset = prefab.GetComponentInChildren<ChunkConnectionPosition>().GetTileRelativePosition();
    }

    public void OnClosedChunkConnection()
    {
        foreach (GameObject objectOnClose in _originalPrefabConnection.ObjectsOnClosedConnection)
        {
            transform.parent.GetComponent<MultiTileMapsContainer>().TrySpawnObject(objectOnClose, GetTilePosition() - _tilemapOffset);
        }
        DestroyConnection();
    }

    public void OnOpenedChunkConnection()
    {
        foreach (GameObject objectOnClose in _originalPrefabConnection.ObjectsOnOpenedConnection)
        {
            transform.parent.GetComponent<MultiTileMapsContainer>().TrySpawnObject(objectOnClose, GetTilePosition() - _tilemapOffset);
        }
    }

    public void DestroyConnection()
    {
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
