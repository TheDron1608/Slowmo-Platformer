using System.Collections.Generic;

public class SetExitRoomGeneration : AbstractModificator
{
    public DoorGenerationPosition.PreGeneratedDoorTempInfo.DoorGenerationTypes OverrideDefaultDoorType;
    public bool OverrideEnableExtraExitBrunchsValue = true;

    private DoorGenerationPosition.PreGeneratedDoorTempInfo.DoorGenerationTypes _oldDefaultDoorType;
    private bool _oldEnableExtraExitBrunchsValue = true;

    public override void OnModificatorAdded()
    {
        base.OnModificatorAdded();

        _oldDefaultDoorType = WorldGenerationManager.Instance.DefaultExitDoorType;
        _oldEnableExtraExitBrunchsValue = WorldGenerationManager.Instance.EnableExtraExitBrunchs;

        WorldGenerationManager.Instance.DefaultExitDoorType = OverrideDefaultDoorType;
        WorldGenerationManager.Instance.EnableExtraExitBrunchs = OverrideEnableExtraExitBrunchsValue;
    }

    public override void OnModificatorRemoved()
    {
        base.OnModificatorRemoved();

        if (WorldGenerationManager.Instance != null)
        {
            WorldGenerationManager.Instance.DefaultExitDoorType = _oldDefaultDoorType;
            WorldGenerationManager.Instance.EnableExtraExitBrunchs = _oldEnableExtraExitBrunchsValue;
        }
    }
}