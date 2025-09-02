using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class WorldGenerationManager : MonoBehaviour
{
    const int GENERATION_FAIL_ITERATIONS_LIMIT = 4;
    const int GENERATION_BUILDING_FAIL_INTERATIONS_LIMIT = 12;

    public int BuildingsAmount = 8;
    public int MinBuildingRooms = 3;
    public int MaxBuildingRooms = 12;
    public int BuildingDistance = 25;
    public List<Chunk> Chunks = new();
    public Vector2 GenerateDirection = Vector2.one;
    public float ExtraRoomGenerationChance = 0.1f;
    public int Seed;

    private UnityEngine.Random.State _randomState;

    public void GenerateLevel()
    {
        //setting variables
        UnityEngine.Random.state = _randomState;

        Vector3Int currentBuildingEnterPosition = Vector3Int.zero;
        int currentBuildingLayerIndex = LayerManager.Instance.ZLayers.Count / 2;
        BuildingInfo prevBuilding = null;

        //generating buildings
        for (int i = 0; i < BuildingsAmount; i++)
        {
            //trying generating building at next/current layer til not find valid layer, if no valid layer stop generating
            int buildingLayerStep;
            if (currentBuildingLayerIndex == 0)
            {
                buildingLayerStep = 1;
            }
            else if (currentBuildingLayerIndex == LayerManager.Instance.ZLayers.Count - 1)
            {
                buildingLayerStep = -1;
            }
            else
            {
                buildingLayerStep = NumberMath.RandomCoinflip() ? -1 : 1;
            }

            for (
                int layerIndex = currentBuildingLayerIndex + buildingLayerStep;
                layerIndex != currentBuildingLayerIndex;
                layerIndex += buildingLayerStep
                )
            {
                if (layerIndex >= LayerManager.Instance.ZLayers.Count) layerIndex = 0;
                if (layerIndex < 0) layerIndex = LayerManager.Instance.ZLayers.Count - 1;

                //trying generate building, if failed GENERATION_BUILDING_FAIL_INTERATIONS_LIMIT times finish generating
                if (TryGenerateBuilding(
                    LayerManager.Instance.ZLayers[currentBuildingLayerIndex],
                    currentBuildingEnterPosition,
                    NumberMath.PickRandomInRangeNoSeed(MinBuildingRooms, MaxBuildingRooms),
                    new Vector3Int((int)(GenerateDirection.normalized.x * 99999), (int)(GenerateDirection.normalized.y * 99999)),
                    out BuildingInfo newBuilding
                    ))
                {
                    //connect prev and current building with door
                    if (prevBuilding != null)
                    {
                        DoorGenerationPosition.GenerateDoorPair(prevBuilding.Exit, newBuilding.Enter);
                    }
                    prevBuilding = newBuilding;

                    currentBuildingEnterPosition = new Vector3Int((int)newBuilding.Exit.transform.position.x, (int)newBuilding.Exit.transform.position.y);
                    currentBuildingLayerIndex = layerIndex;

                    break;
                }
            }
        }

        //generating enviroment with OnFinishLevelEnviroment attr
        foreach (ZIndexLayer layer in LayerManager.Instance.ZLayers)
        {
            foreach (GenerateOnFinishAllBuildingEnviroment lateGenEnviroment in layer.WorldGenerationDataObjectsContainer.GetComponentsInChildren<GenerateOnFinishAllBuildingEnviroment>(false))
            {
                lateGenEnviroment.Generate();
            }
            foreach (GenerateOnFinishAllBuildingEnviroment lateGenEnviroment in layer.WorldGenerationDataObjectsContainer.GetComponentsInChildren<GenerateOnFinishAllBuildingEnviroment>(false))
            {
                lateGenEnviroment.gameObject.SetActive(false);
            }
        }
        //generating enviroment with OnFinishLevelEnviroment attr
        foreach (ZIndexLayer layer in LayerManager.Instance.ZLayers)
        {
            foreach (GenerateOnFinishLevelEnviroment veryLateGenEnviroment in layer.WorldGenerationDataObjectsContainer.GetComponentsInChildren<GenerateOnFinishLevelEnviroment>(false))
            {
                veryLateGenEnviroment.Generate();
            }
            foreach (GenerateOnFinishLevelEnviroment veryLateGenEnviroment in layer.WorldGenerationDataObjectsContainer.GetComponentsInChildren<GenerateOnFinishLevelEnviroment>(false))
            {
                veryLateGenEnviroment.gameObject.SetActive(false);
            }
        }
    }

    public bool TryGenerateBuilding(ZIndexLayer layer, Vector3Int position, int chunksAmount, Vector3Int prefferedPosition, out BuildingInfo newBuildingInfo)
    {
        //initializing building info
        GameObject newBuildingInfoGO = new GameObject("BuildingInfo");
        newBuildingInfoGO.transform.parent = layer.WorldGenerationDataObjectsContainer.transform;
        newBuildingInfo = newBuildingInfoGO.AddComponent<BuildingInfo>();

        //creating first room with enter door, if failed return false
        if (!NumberMath.PickRandomItem(Chunks).TryGenerateChunkWithDoor(layer.MultiTileMapsContainer, position, out ChunkInfo firstChunk, out newBuildingInfo.Enter)) return false;
        newBuildingInfo.AddChunk(firstChunk);

        //creating default rooms
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
                    if (NumberMath.PickRandomItem(Chunks).TryAddChunk(layer.MultiTileMapsContainer, avaibleConnection, out ChunkInfo newChunkInfo, out ChunkConnectionPosition newChunkConnection))
                    {
                        newChunkConnection.DestroyConnection();
                        avaibleConnection.DestroyConnection();
                        newBuildingInfo.AddChunk(newChunkInfo);
                        successfullGenerating = true;
                        break;
                    }
                }
                if (successfullGenerating) break;
            }
        }

        //generating extra rooms
        foreach (ChunkInfo chunk in newBuildingInfo.Chunks)
        {
            foreach (ChunkConnectionPosition connection in chunk.Connections)
            {
                if (!connection.isActiveAndEnabled) continue;

                if (UnityEngine.Random.value < ExtraRoomGenerationChance)
                {
                    NumberMath.PickRandomItem(Chunks).TryAddChunk(layer.MultiTileMapsContainer, connection, out ChunkInfo newExtraChunkInfo, out ChunkConnectionPosition newExtraConnectionPosition);
                }
            }
        }

        //setting exit door
        newBuildingInfo.Exit = NumberMath.PickRandomItem(newBuildingInfo.Chunks.Last().DoorGenPositions);

        //closing unused room connections
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

        //generating enviroment with OnFinishBuildingEnviroment attr
        foreach (GenerateOnFinishBuildingEnviroment lateGenEnviroment in layer.WorldGenerationDataObjectsContainer.GetComponentsInChildren<GenerateOnFinishBuildingEnviroment>(false))
        {
            lateGenEnviroment.Generate();
        }
        foreach (GenerateOnFinishBuildingEnviroment lateGenEnviroment in layer.WorldGenerationDataObjectsContainer.GetComponentsInChildren<GenerateOnFinishBuildingEnviroment>(false))
        {
            lateGenEnviroment.gameObject.SetActive(false);
        }

        return true;
    }

    private void Awake()
    {
        _randomState = UnityEngine.Random.state;
        UnityEngine.Random.InitState(Seed);

        GenerateLevel();
    }
}
