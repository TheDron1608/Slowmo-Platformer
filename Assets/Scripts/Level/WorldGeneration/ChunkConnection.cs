using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class ChunkConnection : GenerateOnFinishBuildingEnviroment
{
    public enum ChunkConnectionDirection
    {
        TOP,
        BOTTOM,
        LEFT,
        RIGHT
    }

    public enum ChunkConnectionState
    {
        CLOSED,
        OPENED
    }

    public ChunkConnectionDirection Direction;
    public List<GameObject> ObjectsOnOpenedConnection;
    public List<GameObject> ObjectsOnClosedConnection;

    private ChunkConnection _originalPrefab;
    private ChunkInfo _chunk;
    private ChunkConnectionState _state = ChunkConnectionState.CLOSED;

    public ChunkInfo Chunk
    {
        get => _chunk;
        private set => _chunk = value;
    }
    public ChunkConnectionState State
    {
        get => _state;
        set => _state = value;
    }
    public ChunkConnection OriginalPrefab
    {
        get => _originalPrefab;
        set => _originalPrefab = value;
    }

    public override void Generate()
    {
        ZIndexLayer layer = LayerManager.Instance.GetZLayerOfGameObject(gameObject);
        List<GameObject> targetGenerationObjs;
        if (State == ChunkConnectionState.CLOSED) targetGenerationObjs = OriginalPrefab.ObjectsOnClosedConnection;
        else if (State == ChunkConnectionState.OPENED) targetGenerationObjs = OriginalPrefab.ObjectsOnOpenedConnection;
        else targetGenerationObjs = new();

        foreach (GameObject targetGemeratonObj in targetGenerationObjs)
        {
            layer.TrySpawnObject(
                targetGemeratonObj, 
                NumberMath.Vec3ToVec3Int(transform.position - OriginalPrefab.transform.position), 
                _chunk?.Building,
                _chunk
                );
        }
        //DestroyConnection();
}

    public override void PreGenerate(ZIndexLayer preGenerateWhere, Vector3 position, BuildingInfo building, ChunkInfo chunk)
    {
        ChunkConnection newConnection = Instantiate(this, position + transform.position, transform.rotation, preGenerateWhere.WorldGenerationDataObjectsContainer);
        newConnection.Chunk = chunk;
        newConnection.OriginalPrefab = this;
        chunk.Connections.Add(newConnection);

        foreach (Transform child in newConnection.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public bool GetConnectionIsValid(ChunkConnection targetConnection)
    {
        return
            (Direction == ChunkConnectionDirection.TOP && targetConnection.Direction == ChunkConnectionDirection.BOTTOM) ||
            (Direction == ChunkConnectionDirection.BOTTOM && targetConnection.Direction == ChunkConnectionDirection.TOP) ||
            (Direction == ChunkConnectionDirection.LEFT && targetConnection.Direction == ChunkConnectionDirection.RIGHT) ||
            (Direction == ChunkConnectionDirection.RIGHT && targetConnection.Direction == ChunkConnectionDirection.LEFT);
    }

    public void DestroyConnection()
    {
        gameObject.SetActive(false);
        Destroy(gameObject);
    }

    public Vector3Int GetTilePosition()
    {
        return new Vector3Int((int)math.round(transform.position.x), (int)math.round(transform.position.y));
    }
}
