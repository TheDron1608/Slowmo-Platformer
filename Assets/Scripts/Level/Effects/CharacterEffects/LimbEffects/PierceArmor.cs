using UnityEngine;

public class PierceArmor : AbstractCharacterLimbEffectWithSender
{
    public Armor.ArmorPierceResistantLevels PierceLevel;
    public float ArmorDamage = 0;

    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        foreach (CharacterEquipmentPart equipment in AffectedPart.CharComponents.CharacterPartsManager.GetCharacterPartEquipment(AffectedPart as CharacterLimbPart))
        {
            if (equipment.CharPartEffectsReceiver.TryGetEffect(out Armor armor))
            {
                if (PierceLevel >= armor.ArmorPierceResistantLevel && equipment.TryGetComponent(out BreakableObject breakableObject))
                {
                    breakableObject.BreakObject(sender);
                }
                else if (equipment.TryGetComponent(out DamagableObject damagableObject))
                {
                    damagableObject.ApplyDamage(ArmorDamage, sender);
                }
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
            if (equipment.CharPartEffectsReceiver.TryGetEffect(out Armor armor))
            {
                return true;
            }
        }
        return false;
    }
}
