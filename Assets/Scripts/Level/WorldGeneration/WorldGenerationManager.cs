using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class WorldGenerationManager : MonoBehaviour
{
    const int GENERATION_FAIL_ITERATIONS_LIMIT = 4;

    public int BuildingsAmount = 8;
    public int MinBuildingRooms = 3;
    public int MaxBuildingRooms = 12;
    public int BuildingDistance = 25;
    public List<Chunk> Chunks = new();
    public int Seed;

    private UnityEngine.Random.State _randomState;

    public void GenerateLevel()
    {
        UnityEngine.Random.state = _randomState;

        Vector3Int currentBuildingEnterPosition = Vector3Int.zero;
        ZIndexLayer currentBuildingLayer = NumberMath.PickMiddleItemFromList(LayerManager.Instance.ZLayers);

        for (int i = 0; i < BuildingsAmount; i++)
        {
            Vector3Int currentBuildingExitPosition = 
                currentBuildingEnterPosition + 
                new Vector3Int(
                    (int)math.floor((UnityEngine.Random.value - 0.5f) * BuildingDistance), 
                    (int)math.floor((UnityEngine.Random.value - 0.5f) * BuildingDistance)
                    );

            GenerateBuilding(
                currentBuildingLayer,
                currentBuildingEnterPosition,
                NumberMath.PickRandomInRangeNoSeed(MinBuildingRooms, MaxBuildingRooms),
                currentBuildingExitPosition
                );

            currentBuildingEnterPosition = currentBuildingExitPosition;
            if (NumberMath.RandomCoinflip())
            {
                currentBuildingLayer = currentBuildingLayer.PickLayerAbove() ?? currentBuildingLayer.PickLayerUnder(); 
            }
            else
            {
                currentBuildingLayer = currentBuildingLayer.PickLayerUnder() ?? currentBuildingLayer.PickLayerAbove();
            }
        }

        foreach (ZIndexLayer layer in LayerManager.Instance.ZLayers)
        {
            foreach (GenerateOnFinishLevelEnviroment lateGenEnviroment in layer.WorldGenerationDataObjectsContainer.GetComponentsInChildren<GenerateOnFinishLevelEnviroment>(false))
            {
                lateGenEnviroment.Generate();
            }
            foreach (GenerateOnFinishLevelEnviroment lateGenEnviroment in layer.WorldGenerationDataObjectsContainer.GetComponentsInChildren<GenerateOnFinishLevelEnviroment>(false))
            {
                lateGenEnviroment.gameObject.SetActive(false);
            }
        }
    }

    public void GenerateBuilding(ZIndexLayer layer, Vector3Int position, int chunksAmount, Vector3Int prefferedPosition)
    {
        NumberMath.PickRandomItem(Chunks).ForceGenerateChunk(layer.MultiTileMapsContainer, position, out ChunkConnectionPosition[] firstChunkConnections);

        for (int i = 1; i < chunksAmount; i++)
        {
            if (layer.MultiTileMapsContainer.GetHasAnyTileAt(prefferedPosition)) break;

            foreach (
                ChunkConnectionPosition avaibleConnection in
                layer.WorldGenerationDataObjectsContainer.GetComponentsInChildren<ChunkConnectionPosition>(false).OrderBy(
                    (ChunkConnectionPosition connection) => Vector3.Distance(connection.transform.position, prefferedPosition)
                    )
                )
            {
                if (!avaibleConnection.isActiveAndEnabled) continue;

                bool successfullGenerating = false;
                for (int j = 0; j < GENERATION_FAIL_ITERATIONS_LIMIT; j++)
                {
                    if (NumberMath.PickRandomItem(Chunks).TryAddChunk(layer.MultiTileMapsContainer, avaibleConnection, out ChunkConnectionPosition newChunkConnection))
                    {
                        newChunkConnection.DestroyConnection();
                        avaibleConnection.DestroyConnection();
                        successfullGenerating = true;
                        break;
                    }
                }
                if (successfullGenerating) break;
            }
        }

        foreach (ChunkConnectionPosition unclosedConnection in layer.WorldGenerationDataObjectsContainer.GetComponentsInChildren<ChunkConnectionPosition>(false))
        {
            if (!unclosedConnection.isActiveAndEnabled) continue;

            bool needCloseConnection = true;
            foreach (ChunkConnectionPosition unclosedConnection2 in layer.WorldGenerationDataObjectsContainer.GetComponentsInChildren<ChunkConnectionPosition>(false))
            {
                if (unclosedConnection != unclosedConnection2 && unclosedConnection.GetTilePosition() == unclosedConnection2.GetTilePosition())
                {
                    unclosedConnection.OnOpenedChunkConnection();
                    unclosedConnection.DestroyConnection();
                    unclosedConnection2.DestroyConnection();
                    needCloseConnection = false;
                    break;
                }
            }

            if (needCloseConnection)
            {
                unclosedConnection.OnClosedChunkConnection();
            }
        }

        foreach (GenerateOnFinishBuildingEnviroment lateGenEnviroment in layer.WorldGenerationDataObjectsContainer.GetComponentsInChildren<GenerateOnFinishBuildingEnviroment>(false))
        {
            lateGenEnviroment.Generate();
        }
        foreach (GenerateOnFinishBuildingEnviroment lateGenEnviroment in layer.WorldGenerationDataObjectsContainer.GetComponentsInChildren<GenerateOnFinishBuildingEnviroment>(false))
        {
            lateGenEnviroment.gameObject.SetActive(false);
        }
    }

    private void Awake()
    {
        _randomState = UnityEngine.Random.state;
        UnityEngine.Random.InitState(Seed);

        GenerateLevel();
    }
}
