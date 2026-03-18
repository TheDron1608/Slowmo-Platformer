using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class BuildingInfo
{
    public static int GlobalLowestCoorY = int.MaxValue;
    public static int GlobalHighestCoorY = int.MinValue;

    public List<ChunkInfo> Chunks = new();
    public List<ChunkInfo> MainBrunchChunks = new();
    public ZIndexLayer Layer;
    public DoorGenerationPosition.PreGeneratedDoorTempInfo Enter;
    public DoorGenerationPosition.PreGeneratedDoorTempInfo Exit;
    public int LowestCoorY = int.MaxValue;
    public int HighestCoorY = int.MinValue;

    public BuildingInfo()
    {
        SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
    }
    ~BuildingInfo()
    {
        SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
    }

    private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
    {
        GlobalLowestCoorY = int.MaxValue;
        GlobalHighestCoorY = int.MinValue;
    }
}
