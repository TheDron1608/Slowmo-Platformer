using Unity.Mathematics;
using UnityEngine.SceneManagement;

public class MultiplyLevelSize : AbstractModificator
{
    public float LevelSizeMultiplier;

    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

        WorldGenerationManager.Instance.MinBuildingRooms = 
            (int)math.round(WorldGenerationManager.Instance.MinBuildingRooms * LevelSizeMultiplier * ModificatorMultiplier);
        WorldGenerationManager.Instance.MaxBuildingRooms = 
            (int)math.round(WorldGenerationManager.Instance.MaxBuildingRooms * LevelSizeMultiplier * ModificatorMultiplier);

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
            WorldGenerationManager.Instance.MinBuildingRooms = 
                (int)math.round(WorldGenerationManager.Instance.MinBuildingRooms / (LevelSizeMultiplier * ModificatorMultiplier));
            WorldGenerationManager.Instance.MaxBuildingRooms = 
                (int)math.round(WorldGenerationManager.Instance.MaxBuildingRooms / (LevelSizeMultiplier * ModificatorMultiplier));
        }
    }
}