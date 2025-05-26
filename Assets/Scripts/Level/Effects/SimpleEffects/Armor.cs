using UnityEngine;

public class Armor : AbstractEffect
{
    public enum ArmorPierceResistantLevels
    {
        NO_ARMOR = 0,
        ARMOR = 1,
        HEAVY_ARMOR = 2
    }

    [SerializeField] private ArmorPierceResistantLevels _armorPierceResistantLevel;

    public ArmorPierceResistantLevels ArmorPierceResistantLevel
    {
        get => _armorPierceResistantLevel;
        set => _armorPierceResistantLevel = value;
    }
}
