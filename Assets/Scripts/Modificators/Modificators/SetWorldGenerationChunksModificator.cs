using System.Collections.Generic;
using UnityEngine;

public class SetWorldGenerationChunksModificator : AbstractModificator
{
    public List<Chunk> Chunks = new();
    public List<BuildingEnterChunk> EnterBuildingChunks = new();
    public List<Chunk> UnclosedConnectionsChunks = new();
    public int ParallelRooms = 3;
    public float UnlosedConnectionChunkGenerationChance = 0.33f;

    private List<Chunk> _oldChunks;
    private List<BuildingEnterChunk> _oldEnterBuildingChunks;
    private List<Chunk> _oldUnclosedConnectionsChunks;
    private int _oldParallelRooms;
    private float _oldUnlosedConnectionChunkGenerationChance;

    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

        _oldChunks = WorldGenerationManager.Instance.Chunks;
        _oldEnterBuildingChunks = WorldGenerationManager.Instance.EnterBuildingChunks;
        _oldUnclosedConnectionsChunks = WorldGenerationManager.Instance.UnclosedConnectionsChunks;
        _oldParallelRooms = WorldGenerationManager.Instance.ParallelRooms;
        _oldUnlosedConnectionChunkGenerationChance = WorldGenerationManager.Instance.UnlosedConnectionChunkGenerationChance;

        WorldGenerationManager.Instance.Chunks = Chunks;
        WorldGenerationManager.Instance.EnterBuildingChunks = EnterBuildingChunks;
        WorldGenerationManager.Instance.UnclosedConnectionsChunks = UnclosedConnectionsChunks;
        WorldGenerationManager.Instance.ParallelRooms = ParallelRooms;
        WorldGenerationManager.Instance.UnlosedConnectionChunkGenerationChance = UnlosedConnectionChunkGenerationChance;

        if (SceneList.GetCurrentSceneIsGameplay())
        {
            if (UIManager.Instance.DifficultyCurseChoiseScreenOverlay.IsShown())
            {
                UIManager.Instance.DifficultyCurseChoiseScreenOverlay.DifficultyCurseChoiseUI.RequestSceneChangeOnFinish(SceneList.GAMEPLAY);
            }
            else
            {
                UIManager.Instance.LoadSceneWithEffect(SceneList.GAMEPLAY);
            }
        }
    }

    public override void OnModificatorRemoved()
    {
        base.OnModificatorRemoved();

        if (WorldGenerationManager.Instance != null)
        {
            WorldGenerationManager.Instance.Chunks = _oldChunks;
            WorldGenerationManager.Instance.EnterBuildingChunks = _oldEnterBuildingChunks;
            WorldGenerationManager.Instance.UnclosedConnectionsChunks = _oldUnclosedConnectionsChunks;
            WorldGenerationManager.Instance.ParallelRooms = _oldParallelRooms;
            WorldGenerationManager.Instance.UnlosedConnectionChunkGenerationChance = _oldUnlosedConnectionChunkGenerationChance;
        }
    }
}