using UnityEngine;

public class PierceArmor : AbstractCharacterLimbEffectWithSender
{
    public LimbArmor.ArmorPierceResistantLevels PierceLevel;

    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        foreach (CharacterEquipmentPart equipment in AffectedPart.CharComponents.CharacterPartsManager.GetCharacterPartEquipment(AffectedPart as CharacterLimbPart))
        {
            if (equipment.CharPartEffectsReceiver.TryGetEffect(out LimbArmor armor) && PierceLevel >= armor.ArmorPierceResistantLevel)
            {
                equipment.DestroyPart();
            }
        }

        RemoveSelf();
    }
}
