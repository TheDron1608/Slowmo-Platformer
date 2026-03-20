using Unity.Mathematics;

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