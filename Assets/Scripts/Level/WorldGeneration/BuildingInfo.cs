using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BuildingInfo
{
    public static int GlobalLowestCoorY = int.MaxValue;
    public static int GlobalHighestCoorY = int.MinValue;

    public List<ChunkInfo> Chunks = new();
    public ComplexGenerateionEnviroment.PreGeneratedEnviromentTempInfo Enter;
    public ComplexGenerateionEnviroment.PreGeneratedEnviromentTempInfo Exit;
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
