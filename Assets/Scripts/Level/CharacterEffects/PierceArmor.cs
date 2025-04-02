using UnityEngine;

public class PierceArmor : AbstractCharacterLimbEffect
{
    public LimbArmor.ArmorPierceResistantLevels PierceLevel;

    protected override void OnReceivedSender(MonoBehaviour sender, CharacterPart receiverPart)
    {
        base.OnReceivedSender(sender, receiverPart);

        if (receiverPart.CharComponents.CharacterEffects.TryGetEffect<LimbArmor>(out LimbArmor armorEffect, AffectedLimbPart) && PierceLevel >= armorEffect.ArmorPierceResistantLevel)
        {
            receiverPart.CharComponents.CharacterEffects.RemoveEffect<LimbArmor>(AffectedLimbPart);
            if (armorEffect.Sender is CharacterPart armorPart)
            {
                armorPart.RemovePart();
            }
            else
            {
                throw new UnityException("sender of armor effect must be CharacterPart class, received " + armorEffect.Sender.GetType().Name + " instead");
            }
        }

        RemoveSelf();
    }
}
