using UnityEngine;

public class PierceArmor : AbstractCharacterLimbEffectWithSender
{
    public Armor.ArmorPierceResistantLevels PierceLevel;

    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        foreach (CharacterEquipmentPart equipment in AffectedPart.CharComponents.CharacterPartsManager.GetCharacterPartEquipment(AffectedPart as CharacterLimbPart))
        {
            if (equipment.CharPartEffectsReceiver.TryGetEffect(out Armor armor) && PierceLevel >= armor.ArmorPierceResistantLevel)
            {
                equipment.BreakPart();
            }
        }

        RemoveSelf();
    }
}
