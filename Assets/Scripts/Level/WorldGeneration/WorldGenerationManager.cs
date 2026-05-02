using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1)]
public class WorldGenerationManager : MonoBehaviour
{
    const int GENERATION_FAIL_ITERATIONS_LIMIT = 4;
    const int GENERATION_BUILDING_FAIL_INTERATIONS_LIMIT = 12;

    public float ShopDoorGenerationChance = 0.2f;
    public float CurseDoorGenerationChance = 0.5f;
    public DoorGenerationPosition.PreGeneratedDoorTempInfo.DoorGenerationTypes DefaultExitDoorType = DoorGenerationPosition.PreGeneratedDoorTempInfo.DoorGenerationTypes.CURSE;
    public bool EnableExtraExitBrunchs = true;
    public int BuildingsAmount = 8;
    public int MinBuildingRooms = 3;
    public int MaxBuildingRooms = 12;
    public int BuildingDistance = 25;
    public List<Chunk> Chunks = new();
    public List<BuildingEnterChunk> EnterBuildingChunks = new();
    public List<Chunk> UnclosedConnectionsChunks = new();
    public Vector2 GenerateDirection = Vector2.one;
    public int ParallelRooms = 3;
    public float UnlosedConnectionChunkGenerationChance = 0.33f;
    public int Seed;

    private UnityEngine.Random.State _randomState;
    private List<BuildingInfo> _generatedBuildings = new();

    public static WorldGenerationManager Instance;

    public List<BuildingInfo> GeneratedBuildings
    {
        get => _generatedBuildings;
        private set => _generatedBuildings = value;
    }

    private void Awake()
    {
        if (Instance != null && !Instance.IsDestroyed()) throw new UnityException("limit of 1 WorldGenerationManager instance per scene");
        Instance = this;

        SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;

        _randomState = UnityEngine.Random.state;
        UnityEngine.Random.InitState(Seed);
    }

    private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
    {
        GeneratedBuildings = new();
    }

    public void GenerateLevel()
    {
        //setting variables
        UnityEngine.Random.state = _randomState;

        Vector3Int currentBuildingEnterPosition = Vector3Int.zero;
        int currentBuildingLayerIndex = (int)math.floor(LayerManager.Instance.ZLayers.Count / 2f) - 1;
        BuildingInfo prevBuilding = null;

        //generating buildings
        for (int i = 0; i < BuildingsAmount; i++)
        {
            //trying generating building at next/current layer til not find valid layer, if no valid layer stop generating
            int attemptingBuildingLayerIndex;
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

            for (int layerIndexAdd = 1; layerIndexAdd < LayerManager.Instance.ZLayers.Count + 1; layerIndexAdd++)
            {
                attemptingBuildingLayerIndex = (currentBuildingLayerIndex + layerIndexAdd) % LayerManager.Instance.ZLayers.Count;
                //set building's random extra exits like shop doors or curse doors
                List<DoorGenerationPosition.PreGeneratedDoorTempInfo.DoorGenerationTypes> extraExits = new();
                if (EnableExtraExitBrunchs)
                {
                    if (RandomManager.Instance.ProcRandomGoodChance(ShopDoorGenerationChance)) extraExits.Add(DoorGenerationPosition.PreGeneratedDoorTempInfo.DoorGenerationTypes.SHOP);
                    if (RandomManager.Instance.ProcRandomGoodChance(CurseDoorGenerationChance)) extraExits.Add(DoorGenerationPosition.PreGeneratedDoorTempInfo.DoorGenerationTypes.CURSE);
                }

                //trying generate building, if failed GENERATION_BUILDING_FAIL_INTERATIONS_LIMIT times finish generating
                if (TryGenerateBuilding(
                    LayerManager.Instance.ZLayers[attemptingBuildingLayerIndex],
                    currentBuildingEnterPosition,
                    NumberMath.PickRandomInRangeNoSeed(MinBuildingRooms, MaxBuildingRooms),
                    new Vector3Int((int)(GenerateDirection.normalized.x * 99999), (int)(GenerateDirection.normalized.y * 99999)),
                    extraExits,
                    out BuildingInfo newBuilding
                    ))
                {
                    GeneratedBuildings.Add(newBuilding);
                    //connect prev and current building with door
                    if (prevBuilding != null)
                    {
                        DoorGenerationPosition.GenerateDoorPair(prevBuilding.Exit, newBuilding.Enter);
                    }
                    prevBuilding = newBuilding;

                    currentBuildingEnterPosition = NumberMath.Vec3ToVec3Int(newBuilding.Exit.GetSpawnPosition());
                    currentBuildingLayerIndex = attemptingBuildingLayerIndex;

                    break;
                }
            }
        }

        //generate next level door
        prevBuilding.Exit.Generate(DefaultExitDoorType);

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

    public bool TryGenerateBuilding(ZIndexLayer layer, Vector3Int position, int chunksAmount, Vector3Int prefferedPosition, List<DoorGenerationPosition.PreGeneratedDoorTempInfo.DoorGenerationTypes> extraExits, out BuildingInfo newBuildingInfo)
    {
        //initializing building info
        newBuildingInfo = default;
        BuildingInfo newBuildingInfoResult = new();
        layer.BuildinsInfo.Add(newBuildingInfoResult);
        newBuildingInfoResult.Layer = layer;

        //creating first room with enter door, if failed generation or could not spawn any enter doors return false
        if (!NumberMath.PickRandomItem(EnterBuildingChunks).TryGenerateChunkWithEnterAt(layer, position, newBuildingInfoResult, out ChunkInfo firstChunk)) return false;
        newBuildingInfoResult.Enter = firstChunk.DoorGenPositions.First();
        ChunkInfo currentMainBrunchChunk = firstChunk;

        //generate main brunch
        for (int i = 1; i < chunksAmount; i++)
        {
            if (layer.MultiTileMapsContainer.GetHasAnyTileAt(prefferedPosition)) break;

            for (int j = 0; j < GENERATION_FAIL_ITERATIONS_LIMIT; j++)
            {
                if (NumberMath.PickRandomItem(Chunks).TryAddChunk(
                    layer,
                    currentMainBrunchChunk.Connections
                        .Where(e => e.State == ChunkConnection.PreGeneratedChunkConnectionTempInfo.ChunkConnectionState.CLOSED)
                        .OrderBy(e => Vector2.Distance(e.GetSpawnPosition(), VectorMath.Vec3IntToVec3(prefferedPosition)))
                        .FirstOrDefault(),
                    newBuildingInfoResult,
                    out ChunkInfo newChunkInfo,
                    out ChunkConnection.PreGeneratedChunkConnectionTempInfo newConnectionInfo
                    ))
                {
                    currentMainBrunchChunk = newChunkInfo;
                    newBuildingInfoResult.MainBrunchChunks.Add(newChunkInfo);

                    break;
                }
            }
        }

        //generate extra exit brunchs
        foreach (var extraExit in extraExits)
        {
            BuildingInfo.BuildingExtraExitBrunchInfo extraExitInfo = new();
            extraExitInfo.ExitType = extraExit;
            Vector2 extraExitPrefferedPosition = VectorMath.PickRandomDirection() * 100000f;

            ChunkInfo currentExtraExitChunk =
                newBuildingInfoResult.Chunks
                    .Where(e => e.Connections.Any(e =>
                        e.State == ChunkConnection.PreGeneratedChunkConnectionTempInfo.ChunkConnectionState.CLOSED) && 
                        e != newBuildingInfoResult.Enter.Chunk &&
                        e != newBuildingInfoResult.MainBrunchChunks[newBuildingInfoResult.MainBrunchChunks.Count - 1]
                        )
                    .OrderBy(e => Vector2.Distance(e.PickConnectionsAvgPosition(), extraExitPrefferedPosition))
                    .FirstOrDefault();
            if (currentExtraExitChunk == null) break;

            for (int i = 0; i < chunksAmount / 2; i++)
            {
                for (int j = 0; j < GENERATION_FAIL_ITERATIONS_LIMIT; j++)
                {
                    if (NumberMath.PickRandomItem(Chunks).TryAddChunk(
                        layer,
                        currentExtraExitChunk.Connections
                            .Where(e => e.State == ChunkConnection.PreGeneratedChunkConnectionTempInfo.ChunkConnectionState.CLOSED)
                            .OrderBy(e => Vector2.Distance(e.GetSpawnPosition(), extraExitPrefferedPosition))
                            .FirstOrDefault(),
                        newBuildingInfoResult,
                        out ChunkInfo newChunkInfo,
                        out ChunkConnection.PreGeneratedChunkConnectionTempInfo newConnectionInfo
                        ))
                    {
                        newChunkInfo.DistanceFromMainGenerationBranch = currentExtraExitChunk.DistanceFromMainGenerationBranch + 1;
                        currentExtraExitChunk = newChunkInfo;
                        extraExitInfo.Chunks.Add(newChunkInfo);

                        break;
                    }
                }
            }

            newBuildingInfoResult.ExtraExitBrunchs.Add(extraExitInfo);
        }

        //generate parallel rooms
        List<Chunk> notMainBrunchChunks = Chunks.Where(e => !e.GeneratableAtMainBrunchOnly).ToList();
        for (int mainBrunchIter = 0; mainBrunchIter < newBuildingInfoResult.MainBrunchChunks.Count - ParallelRooms; mainBrunchIter++)
        {
            List<ChunkInfo> validToGenerateChunks = new List<ChunkInfo> { newBuildingInfoResult.MainBrunchChunks[mainBrunchIter] };
            for (int i = 0; i < ParallelRooms; i++)
            {
                for (int j = 0; j < GENERATION_FAIL_ITERATIONS_LIMIT; j++)
                {
                    ChunkInfo addToChunk = NumberMath.PickRandomItem(validToGenerateChunks);
                    ChunkConnection.PreGeneratedChunkConnectionTempInfo addToConnection = addToChunk.Connections
                        .Where(e => e.State == ChunkConnection.PreGeneratedChunkConnectionTempInfo.ChunkConnectionState.CLOSED)
                        .OrderBy(e => Vector2.Distance(e.GetSpawnPosition(), VectorMath.Vec3IntToVec3(prefferedPosition)))
                        .FirstOrDefault();

                    if (addToConnection != null && NumberMath.PickRandomItem(notMainBrunchChunks).TryAddChunk(
                        layer,
                        addToConnection,
                        newBuildingInfoResult,
                        out ChunkInfo newChunkInfo,
                        out ChunkConnection.PreGeneratedChunkConnectionTempInfo newChunkConnection
                        ))
                    {
                        newChunkInfo.DistanceFromMainGenerationBranch = addToChunk.DistanceFromMainGenerationBranch + 1;
                        validToGenerateChunks.Add(newChunkInfo);
                        break;
                    }
                }
            }
        }

        //generating enviroment with GenerateBeforeExtraChunksEnviroment attr
        foreach (ComplexGenerateionEnviroment.PreGeneratedEnviromentTempInfo beforeExtraChunksEnviroment in layer.GetGenerationTempInfoByType<GenerateBeforeExtraChunksEnviroment>(false))
        {
            beforeExtraChunksEnviroment.Generate();
        }

        //add extra chunks for closed connections
        for (int chunkIter = 0; chunkIter < newBuildingInfoResult.Chunks.Count; chunkIter++)
        {
            if (UnityEngine.Random.value > UnlosedConnectionChunkGenerationChance) continue;

            foreach (ChunkConnection.PreGeneratedChunkConnectionTempInfo connection in newBuildingInfoResult.Chunks[chunkIter].Connections)
            {
                if (connection.State == ChunkConnection.PreGeneratedChunkConnectionTempInfo.ChunkConnectionState.CLOSED && !connection.Generated)
                {
                    Chunk[] validChunks = UnclosedConnectionsChunks.Where((c) => c.GetAnyConnectionIsValid(connection.GetTargetConnection())).ToArray();
                    if (validChunks.Length > 0 && NumberMath.PickRandomItem(validChunks).TryAddChunk(
                        layer,
                        connection,
                        newBuildingInfoResult,
                        out ChunkInfo newChunkInfo,
                        out ChunkConnection.PreGeneratedChunkConnectionTempInfo newChunkConnection))
                    {
                        newChunkInfo.DistanceFromMainGenerationBranch = connection.Chunk.DistanceFromMainGenerationBranch + 1;
                    }
                }
            }
        }

        //generating enviroment with OnFinishBuildingEnviroment attr
        foreach (ComplexGenerateionEnviroment.PreGeneratedEnviromentTempInfo lateGenEnviroment in layer.GetGenerationTempInfoByType<GenerateOnFinishBuildingEnviroment>(false))
        {
            lateGenEnviroment.Generate();
        }

        //setting extra brunchs' exit door
        foreach (var extraExitBrunch in newBuildingInfoResult.ExtraExitBrunchs)
        {
            for (int i = extraExitBrunch.Chunks.Count - 1; i >= 0; i--)
            {
                if (extraExitBrunch.Chunks[i].DoorGenPositions.Count > 0)
                {
                    extraExitBrunch.Exit = NumberMath.PickRandomItem(extraExitBrunch.Chunks[i].DoorGenPositions);
                    extraExitBrunch.Exit.Generate(extraExitBrunch.ExitType);

                    break;
                }
            }
        }

        //setting main brunch's exit door
        for (int i = newBuildingInfoResult.MainBrunchChunks.Count - 1; i >= 0; i--)
        {
            if (newBuildingInfoResult.MainBrunchChunks[i].DoorGenPositions.Count > 0)
            {
                newBuildingInfoResult.Exit = NumberMath.PickRandomItem(newBuildingInfoResult.MainBrunchChunks[i].DoorGenPositions);
                break;
            }
        }
        if (newBuildingInfoResult.Exit == null) return false;

        newBuildingInfo = newBuildingInfoResult;
        return true;
    }

    private void OnDestroy()
    {
        Instance = null;
        SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
    }
}
