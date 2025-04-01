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
            GameObject.Destroy(armorEffect.Sender.gameObject);
        }

        RemoveSelf();
    }
}
