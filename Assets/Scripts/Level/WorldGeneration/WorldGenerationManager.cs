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
    public List<List<GameObject>> PossibleLootboxContent = new();
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

                    currentBuildingEnterPosition = NumberMath.Vec3ToVec3Int(newBuilding.Exit.GetSpawnPosition());
                    currentBuildingLayerIndex = layerIndex;

                    break;
                }
            }
        }

        //generate next level door
        prevBuilding.Exit.Generate(DoorGenerationPosition.PreGeneratedDoorTempInfo.DoorGenerationTypes.NEXTLEVEL);

        //generating enviroment with OnFinishAllBuilding Enviroment attr
        foreach (ZIndexLayer layer in LayerManager.Instance.ZLayers)
        {
            foreach (ComplexGenerateionEnviroment.PreGeneratedEnviromentTempInfo lateGenEnviroment in layer.GetGenerationTempInfoByType<GenerateOnFinishAllBuildingEnviroment>(false))
            {
                lateGenEnviroment.Generate();
            }
        }
        //generating enviroment with OnFinishLevelEnviroment attr
        foreach (ZIndexLayer layer in LayerManager.Instance.ZLayers)
        {
            foreach (ComplexGenerateionEnviroment.PreGeneratedEnviromentTempInfo lateGenEnviroment in layer.GetGenerationTempInfoByType<GenerateOnFinishLevelEnviroment>(false))
            {
                lateGenEnviroment.Generate();
            }
        }
    }

    public bool TryGenerateBuilding(ZIndexLayer layer, Vector3Int position, int chunksAmount, Vector3Int prefferedPosition, out BuildingInfo newBuildingInfo)
    {
        //initializing building info
        newBuildingInfo = new();
        layer.BuildinsInfo.Add(newBuildingInfo);

        //creating first room with enter door, if failed return false
        if (!NumberMath.PickRandomItem(Chunks).TryGenerateChunkWithDoor(layer, position, newBuildingInfo, out ChunkInfo firstChunk, out newBuildingInfo.Enter)) return false;

        //creating default rooms
        for (int i = 1; i < chunksAmount; i++)
        {
            if (layer.MultiTileMapsContainer.GetHasAnyTileAt(prefferedPosition)) break;

            foreach (
                ComplexGenerateionEnviroment.PreGeneratedEnviromentTempInfo avaibleConnection in
                layer.GetGenerationTempInfoByType<ChunkConnection>(false).OrderBy(
                    (ComplexGenerateionEnviroment.PreGeneratedEnviromentTempInfo connection) => Vector3.Distance(connection.GetSpawnPosition(), prefferedPosition)
                    )
                )
            {
                if (avaibleConnection.Generated) continue;

                bool successfullGenerating = false;
                for (int j = 0; j < GENERATION_FAIL_ITERATIONS_LIMIT; j++)
                {
                    if (NumberMath.PickRandomItem(Chunks).TryAddChunk(
                        layer, 
                        avaibleConnection as ChunkConnection.PreGeneratedChunkConnectionTempInfo, 
                        newBuildingInfo,
                        out ChunkInfo newChunkInfo, 
                        out ChunkConnection.PreGeneratedChunkConnectionTempInfo newChunkConnection))
                    {
                        successfullGenerating = true;
                        break;
                    }
                }
                if (successfullGenerating)
                {
                    break;
                }
            }
        }

        //generating extra rooms
        /*
        foreach (ChunkInfo chunk in newBuildingInfo.Chunks)
        {
            foreach (ChunkConnection connection in chunk.Connections)
            {
                if (!connection.isActiveAndEnabled) continue;

                if (UnityEngine.Random.value < ExtraRoomGenerationChance)
                {
                    NumberMath.PickRandomItem(Chunks).TryAddChunk(layer, connection, newBuildingInfo, out ChunkInfo newExtraChunkInfo, out ChunkConnection newExtraConnectionPosition);
                }
            }
        }
        */

        //setting exit door
        newBuildingInfo.Exit = NumberMath.PickRandomItem(newBuildingInfo.Chunks.Last().DoorGenPositions);

        //generating enviroment with OnFinishBuildingEnviroment attr
        foreach (ComplexGenerateionEnviroment.PreGeneratedEnviromentTempInfo lateGenEnviroment in layer.GetGenerationTempInfoByType<GenerateOnFinishBuildingEnviroment>(false))
        {
            lateGenEnviroment.Generate();
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
