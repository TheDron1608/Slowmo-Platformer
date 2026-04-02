
public interface IModificatorInfo
{
    public ModificatorLocalization Localization { get; }
    public AbstractModificator.ModificatorStatuses Status { get; }
    public bool DisabledModificator { get; }
    public float ModificatorPrice { get; }
    public bool Multiplierable { get; }
    public float ModificatorMultiplier { get; }
} 