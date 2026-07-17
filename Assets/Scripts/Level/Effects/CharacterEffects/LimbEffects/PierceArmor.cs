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

    public override bool ApplyCondition(ObjectEffectsReceiver affectWho, MonoBehaviour sender)
    {
        return base.ApplyCondition(affectWho, sender) && affectWho.TryGetComponent(out CharacterLimbPart limbPart) && GetHasArmorOnPart(limbPart);
    }

    private bool GetHasArmorOnPart(CharacterLimbPart affectedPart)
    {

        foreach (CharacterEquipmentPart equipment in affectedPart.CharComponents.CharacterPartsManager.GetCharacterPartEquipment(affectedPart as CharacterLimbPart))
        {
            if (equipment.CharPartEffectsReceiver.TryGetEffect(out Armor armor) && PierceLevel >= armor.ArmorPierceResistantLevel)
            {
                return true;
            }
        }
        return false;
    }
}
