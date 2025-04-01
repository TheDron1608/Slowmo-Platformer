using UnityEngine;

public class LimbArmor : AbstractCharacterLimbEffect
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

    protected override void OnReceivedSender(MonoBehaviour sender, CharacterPart receiverPart)
    {
        base.OnReceivedSender(sender, receiverPart);

        if (sender is CharacterEquipmentPart armorPart)
        {
            Armor = armorPart;
        }
        else
        {
            throw new UnityException("OnReceivedSender sender argument must be CharacterEquipmentPart, received " + sender.GetType().Name + " instead");
        }
    }
}
