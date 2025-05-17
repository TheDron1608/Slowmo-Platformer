using UnityEngine;

public class LimbArmor : AbstractCharacterLimbEffectWithSender
{
    public enum ArmorPierceResistantLevels : int
    {
        NO_ARMOR = 0,
        ARMOR = 1,
        HEAVY_ARMOR = 2,
        ANY_ARMOR = 100
    }

    public ArmorPierceResistantLevels ArmorPierceResistantLevel;

    private CharacterEquipmentPart _armor;

    public CharacterEquipmentPart Armor
    {
        get => _armor;
        private set => _armor = value;
    }

    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        if (sender.TryGetComponent(out CharacterEquipmentPart armorPart))
        {
            Armor = armorPart;
        }
        else
        {
            throw new UnityException("not found CharacterEquipmentPart component at " + gameObject.name);
        }
    }
}
