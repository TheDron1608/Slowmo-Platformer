using UnityEngine;

public class PierceArmor : AbstractCharacterLimbEffectWithSender
{
    public LimbArmor.ArmorPierceResistantLevels PierceLevel;

    protected override void OnReceivedSender(MonoBehaviour sender)
    {
        if (
            AffectedPart.CharComponents.CharacterEffectsReceiver.TryGetEffect(out LimbArmor armorEffect, AffectedPart) && 
            PierceLevel >= armorEffect.ArmorPierceResistantLevel
            )
        {
            AffectedPart.CharPartEffectsReceiver.RemoveEffect<LimbArmor>();
            if (armorEffect.AffectedPart is CharacterPart)
            {
                armorEffect.AffectedPart.DestroyPart();
            }
            else
            {
                throw new UnityException("sender of armor effect must be CharacterPart class, received " + armorEffect.Sender.GetType().Name + " instead");
            }
        }

        RemoveSelf();
    }
}
